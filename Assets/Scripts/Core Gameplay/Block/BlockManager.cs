using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using CandyCoded.HapticFeedback;
using PrimeTween;
using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    [SerializeField] private List<BaseBlock> _blocks;

    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private GameSetting gameSetting;

    public List<BaseBlock> Blocks
    {
        get => _blocks;
        set => _blocks = value;
    }

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

        levelObserver.Progress = 0;

        // if (_checkingWinCoroutine != null)
        // {
        //     StopCoroutine(_checkingWinCoroutine);
        // }

        // _checkingWinCoroutine = StartCoroutine(CheckingWin());

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

                if (GamePersistentVariable.IsVibrate)
                {
                    // HapticFeedback.MediumFeedback();
                }

                for (int i = 0; i < group.Count; i++)
                {
                    group[i].HandleOnMerge();
                }

                levelObserver.Progress = _blocks.Count(item => !item.gameObject.activeInHierarchy) / (float)_blocks.Count;

                mergeBlockEvent?.Invoke();

                List<Vector2Int> worldTileCoordinators = new List<Vector2Int>();

                foreach (var value in worldTileIndexesOfBlock.Values)
                {
                    worldTileCoordinators.AddRange(value);
                }

                Vector3 iconWorldPosition = AveragePosition(worldTileCoordinators);

                // Block Break Fx
                ParticleSystem blockBreakFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockBreakFx);

                blockBreakFx.gameObject.SetActive(true);
                blockBreakFx.transform.position = iconWorldPosition;
                var main = blockBreakFx.transform.GetChild(2).GetComponent<ParticleSystem>().main;
                main.startColor = group[0].BlockServiceLocator.blockFaction.Color;
                blockBreakFx.Play();

                // Sound
                SoundManager.Instance.PlaySoundClearBlock();

                LevelIconUIManager.Instance.HideIcon(iconWorldPosition, group[0].spriteType);

                // Teleport Booster
                foreach (var block in worldTileIndexesOfBlock.Keys)
                {
                    if (block.BlockProperty.IsUsingTeleportBooster)
                    {
                        block.InvokeConfirmUseBoosterTeleportEvent();

                        break;
                    }
                }
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
                if (!blocksWithSameType[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                for (int j = i + 1; j < blocksWithSameType.Count; j++)
                {
                    if (!blocksWithSameType[j].gameObject.activeInHierarchy)
                    {
                        continue;
                    }

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

        Vector2 AveragePosition(List<Vector2Int> worldTileCoordinators)
        {
            Vector2 sum = Vector2.zero;
            int count = worldTileCoordinators.Count;

            for (int i = 0; i < count; i++)
            {
                sum += TileMapper.TileToWorld(worldTileCoordinators[i]);
            }

            return count > 0 ? sum / count : Vector2.zero;
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
                CheckWin();
            }

            yield return waitForSeconds;
        }
    }

    public void CheckWin()
    {
        if (GamePersistentVariable.hidingIconQueue.Count > 0)
        {
            return;
        }

        int remainingBlocks = 0;

        for (int i = 0; i < _blocks.Count; i++)
        {
            try
            {
                if (_blocks[i].gameObject.activeInHierarchy && _blocks[i].IsValidToMerge())
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
