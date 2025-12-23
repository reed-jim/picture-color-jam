using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    [SerializeField] private List<BaseBlock> _blocks;

    public List<BaseBlock> Blocks => _blocks;

    private Coroutine _checkingWinCoroutine;

    public static event Action winLevelEvent;
    public static event Action mergeBlockEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LevelLoader.sendLevelBaseBlocksDataEvent += ReceiveBaseBlockesData;
        BaseBlock.blockCompletedEvent += OnBlockCompleted;
    }

    private void OnDestroy()
    {
        LevelLoader.sendLevelBaseBlocksDataEvent -= ReceiveBaseBlockesData;
        BaseBlock.blockCompletedEvent -= OnBlockCompleted;
    }

    private void ReceiveBaseBlockesData(BaseBlock[] blocks)
    {
        _blocks = blocks.ToList();

        if (_checkingWinCoroutine != null)
        {
            StopCoroutine(_checkingWinCoroutine);
        }

        _checkingWinCoroutine = StartCoroutine(CheckingWin());

        StartCoroutine(DelaySetupIconForLevel(blocks));
    }

    private IEnumerator DelaySetupIconForLevel(BaseBlock[] blocks)
    {
        yield return new WaitForEndOfFrame();

        LevelIconUIManager.Instance.SetupForLevel(blocks);
    }

    #region Blocks
    public void CheckMergeBlocks()
    {
        Dictionary<string, List<BaseBlock>> groupByType = GroupBlocksByType(_blocks.ToArray());

        // Debug.Log($"SAFERIO CheckMergeBlocks =================");

        foreach (var spriteType in groupByType.Keys)
        {
            List<BaseBlock> blocksWithSameType = groupByType[spriteType];

            Dictionary<int, List<BaseBlock>> groupByTotalTileCount = GroupBlocksByTotalTileCount(blocksWithSameType);

            foreach (var group in groupByTotalTileCount.Values)
            {
                CheckMergeForBlockGroup(group);
            }
        }

        void CheckMergeForBlockGroup(List<BaseBlock> group)
        {
            Dictionary<BaseBlock, List<Vector2Int>> worldTileIndexesOfBlock = new Dictionary<BaseBlock, List<Vector2Int>>();

            foreach (var block in group)
            {
                if (!block.gameObject.activeSelf)
                {
                    continue;
                }

                if (!block.IsValidToMerge())
                {
                    continue;
                }

                List<Vector2Int> blockTileIndexes = TileMapper.Instance.GetBlockTilePositions(block);
                worldTileIndexesOfBlock.Add(block, blockTileIndexes);

                // foreach (var item in blockTileIndexes)
                // {
                //     Debug.Log($"SAFERIO CheckMergeBlocks World {block.transform.GetSiblingIndex()} {block} : {item}");
                // }
            }

            List<Vector2Int> originalTileIndexes = new List<Vector2Int>();
            originalTileIndexes.AddRange(group[0].originalTileIndexes);

            int requiredMatch = originalTileIndexes.Count;
            int currentMatch = 0;

            List<Vector2Int> allLocalBlockTileIndexes = new List<Vector2Int>();

            // Debug.Log($"SAFERIO CheckMergeBlocks Local ==============================");

            for (int i = 0; i < group.Count; i++)
            {
                BaseBlock block = group[i];

                if (!block.gameObject.activeSelf)
                {
                    continue;
                }

                if (!block.IsValidToMerge())
                {
                    continue;
                }

                List<Vector2Int> localBlockTileIndexes = TileMapper.Instance.ToLocalTileIndexes(worldTileIndexesOfBlock[block], worldTileIndexesOfBlock);

                allLocalBlockTileIndexes.AddRange(localBlockTileIndexes);

                for (int j = 0; j < localBlockTileIndexes.Count; j++)
                {
                    int index = j;
                    Vector2Int tileIndex = localBlockTileIndexes[index];

                    // Debug.Log($"SAFERIO CheckMergeBlocks Local {block} : {tileIndex}");

                    if (originalTileIndexes.Contains(tileIndex))
                    {
                        currentMatch++;

                        originalTileIndexes.Remove(tileIndex);
                    }
                }

                // Last item
                if (i == group.Count - 1)
                {
                    if (ExistsLaterWithSameYSmallerX(group))
                    {
                        currentMatch = 0;
                    }
                }
            }

            // Debug.Log($"SAFERIO CheckMergeBlocks currentMatch {currentMatch} : {requiredMatch}");

            if (currentMatch == requiredMatch)
            {
                foreach (var block in worldTileIndexesOfBlock.Keys)
                {
                    for (int i = 0; i < worldTileIndexesOfBlock[block].Count; i++)
                    {
                        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockMergeFx);

                        fx.gameObject.SetActive(true);
                        fx.transform.position = TileMapper.TileToWorld(worldTileIndexesOfBlock[block][i]);
                        fx.Play();
                    }
                }

                for (int i = 0; i < group.Count; i++)
                {
                    group[i].HandleOnMerge();
                }

                mergeBlockEvent?.Invoke();

                Vector3 iconWorldPosition = AveragePosition(group);

                LevelIconUIManager.Instance.HideIcon(iconWorldPosition, group[0].spriteType);
            }
        }

        Dictionary<string, List<BaseBlock>> GroupBlocksByType(BaseBlock[] blocks)
        {
            return blocks
                .GroupBy(block => block.spriteType)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        Dictionary<int, List<BaseBlock>> GroupBlocksByTotalTileCount(List<BaseBlock> blocks)
        {
            return blocks
                .GroupBy(block => block.originalTileIndexes.Count)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        bool ExistsLaterWithSameYSmallerX(List<BaseBlock> blocksWithSameType)
        {
            for (int i = 0; i < blocksWithSameType.Count - 1; i++)
            {
                for (int j = i + 1; j < blocksWithSameType.Count; j++)
                {
                    BaseBlock firstBlock = blocksWithSameType[i];
                    BaseBlock secondBlock = blocksWithSameType[j];

                    int minXFirstBlock = firstBlock.tileIndexes.Min(item => item.x);
                    int minXSecondBlock = secondBlock.tileIndexes.Min(item => item.x);

                    if (minXFirstBlock < minXSecondBlock && firstBlock.transform.position.x > secondBlock.transform.position.x)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        Vector3 AveragePosition(List<BaseBlock> blocks)
        {
            Vector3 sum = Vector3.zero;
            int count = blocks.Count;

            for (int i = 0; i < count; i++)
                sum += blocks[i].transform.position;

            return count > 0 ? sum / count : Vector3.zero;
        }
    }
    #endregion

    private IEnumerator CheckingWin()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(3f);

        while (true)
        {
            if (!GamePersistentVariable.isPlayingHideIconAnimation)
            {
                int remainingBlocks = 0;

                for (int i = 0; i < _blocks.Count; i++)
                {
                    try
                    {
                        if (_blocks[i].gameObject.activeInHierarchy)
                        {
                            remainingBlocks++;
                        }
                    }
                    catch (System.Exception)
                    {
                        remainingBlocks++;
                    }
                }

                if (remainingBlocks == 0)
                {
                    winLevelEvent?.Invoke();
                }
            }

            yield return waitForSeconds;
        }
    }

    private void OnBlockCompleted()
    {
        return;

        int remainingBlocks = 0;

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (_blocks[i].gameObject.activeInHierarchy)
            {
                remainingBlocks++;
            }
        }

        if (remainingBlocks == 0)
        {
            winLevelEvent?.Invoke();
        }
    }
}
