using System.Collections.Generic;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class TileMapper : Singleton<TileMapper>
{
    public float tileSize = 1.4f;

    public int boardWidth = 10;
    public int boardHeight = 10;

    public List<Vector2Int> GetBlockTilePositions(BaseBlock block)
    {
        List<Vector2Int> tilePositions = new List<Vector2Int>();

        int minIndexX = block.tileIndexes.Min(item => item.x);
        int maxIndexX = block.tileIndexes.Max(item => item.x);
        int minIndexY = block.tileIndexes.Min(item => item.y);
        int maxIndexY = block.tileIndexes.Max(item => item.y);

        int numTileX = (maxIndexX - minIndexX) + 1;
        int numTileY = (maxIndexY - minIndexY) + 1;

        Vector2 leftTopPositionWorld = block.transform.position;

        leftTopPositionWorld.x -= (numTileX - 1) / 2f * tileSize;
        leftTopPositionWorld.y += (numTileY - 1) / 2f * tileSize;

        // Vector2 snappedLeftTopPosition = GetSnapPosition(leftTopPositionWorld, numTileX, numTileY);

        Vector2Int leftTopIndex = WorldToTile(leftTopPositionWorld);

        foreach (Vector2Int localTileIndex in block.tileIndexes)
        {
            Vector2Int globalTilePosition = leftTopIndex + new Vector2Int(localTileIndex.x - minIndexX, -(localTileIndex.y - minIndexY));
            tilePositions.Add(globalTilePosition);
        }

        return tilePositions;
    }

    public List<Vector2Int> ToLocalTileIndexes(List<Vector2Int> worldTileIndexes, Dictionary<BaseBlock, List<Vector2Int>> worldTileIndexesOfBlock)
    {
        int minX = worldTileIndexesOfBlock.SelectMany(entry => entry.Value).Min(item => item.x);
        int maxY = worldTileIndexesOfBlock.SelectMany(entry => entry.Value).Max(item => item.y);

        List<Vector2Int> result = new List<Vector2Int>();
        result.AddRange(worldTileIndexes);

        for (int i = 0; i < result.Count; i++)
        {
            Vector2Int modifiedResult = result[i];

            modifiedResult.x = modifiedResult.x - minX;
            modifiedResult.y = maxY - modifiedResult.y;
            // result[i] += new Vector2Int(Mathf.Abs(minX), Mathf.Abs(minY));

            result[i] = modifiedResult;
        }

        return result;
    }

    public Vector2Int WorldToTile(Vector2 worldPosition)
    {
        float offsetX = GamePersistentVariable.levelBoard.numColumn % 2 == 0 ? 0.5f * tileSize : 0;

        int xTile = (int)Mathf.Round((worldPosition.x - offsetX) / tileSize);
        int yTile = (int)Mathf.Round(worldPosition.y / tileSize);

        return new Vector2Int(xTile, yTile);
    }

    public static Vector2 TileToWorld(Vector2 tilePosition)
    {
        float offsetX = GamePersistentVariable.levelBoard.numColumn % 2 == 0 ? 0.5f * GameConstants.tileSize : 0;

        float xWorld = tilePosition.x * GameConstants.tileSize + offsetX;
        float yWorld = tilePosition.y * GameConstants.tileSize;

        return new Vector2(xWorld, yWorld);
    }

    // when knowing left top coordinator of a block
    public static Vector2 LeftTopCoordinatorToBlockPosition(Vector2 leftTopCoordinator, BaseBlock block)
    {
        int numTileX = block.NumTileX;
        int numTileY = block.NumTileY;

        Vector2 position;

        position.x = (leftTopCoordinator.x + (numTileX - 1) / 2f) * GameConstants.tileSize;
        position.y = (leftTopCoordinator.y - (numTileY - 1) / 2f) * GameConstants.tileSize;

        return position;
    }

    public Vector2 LeftTopToBlockPosition(Vector2 leftTopPosWorld, int numTileX, int numTileY, float tileSize)
    {
        Vector2 blockPos = leftTopPosWorld;

        blockPos.x += (numTileX - 1) / 2f * tileSize;
        blockPos.y -= (numTileY - 1) / 2f * tileSize;

        return blockPos;
    }

    public BaseBlock[] SplitBlock(BaseBlock block)
    {
        int minIndexX = block.tileIndexes.Min(item => item.x);
        int maxIndexX = block.tileIndexes.Max(item => item.x);
        int minIndexY = block.tileIndexes.Min(item => item.y);
        int maxIndexY = block.tileIndexes.Max(item => item.y);

        int numTileX = (maxIndexX - minIndexX) + 1;
        int numTileY = (maxIndexY - minIndexY) + 1;

        Vector2 leftTopPositionWorld = block.transform.position;

        leftTopPositionWorld.x -= (numTileX - 1) / 2f * tileSize;
        leftTopPositionWorld.y += (numTileY - 1) / 2f * tileSize;

        Vector2Int leftTopIndex = WorldToTile(leftTopPositionWorld);

        List<BaseBlock> smallBlocks = new List<BaseBlock>();

        foreach (Vector2Int localTileIndex in block.tileIndexes)
        {
            Vector2Int globalTilePosition = leftTopIndex + new Vector2Int(localTileIndex.x - minIndexX, -(localTileIndex.y - minIndexY));

            BaseBlock smallBlock = SpawnBlock1x1At(globalTilePosition);
            smallBlocks.Add(smallBlock);

            smallBlock.transform.SetParent(block.transform.parent);
        }

        return smallBlocks.ToArray();
    }

    public BaseBlock SpawnBlock1x1At(Vector2Int coordinator)
    {
        BaseBlock block = ObjectPoolingEverything.GetFromPool<BaseBlock>(GameConstants.block1x1);

        block.gameObject.SetActive(true);

        Vector2 position = new Vector2();

        float offsetX = GamePersistentVariable.levelBoard.numColumn % 2 == 0 ? 0.5f : 0;
        float offsetY = GamePersistentVariable.levelBoard.numRow % 2 == 0 ? 0.5f : 0;

        position.x = (coordinator.x + offsetX) * tileSize;
        position.y = (coordinator.y + offsetY) * tileSize;

        block.transform.position = position;

        block.SetSortingLayer(block.transform.position);

        return block;
    }

    private Vector2 GetSnapPosition(Vector2 position, int numTileX, int numTileY)
    {
        Vector2 snappedPosition = new Vector2(
            Mathf.Round(position.x / tileSize) * tileSize,
            Mathf.Round(position.y / tileSize) * tileSize
        );

        if (numTileX % 2 == 0)
        {
            if (Mathf.Abs((snappedPosition.x - 0.5f * tileSize) - position.x) < Mathf.Abs((snappedPosition.x + 0.5f * tileSize) - position.x))
            {
                snappedPosition.x -= 0.5f * tileSize;
            }
            else
            {
                snappedPosition.x += 0.5f * tileSize;
            }
        }

        if (numTileY % 2 == 0)
        {
            if (Mathf.Abs((snappedPosition.y - 0.5f * tileSize) - position.y) < Mathf.Abs((snappedPosition.y + 0.5f * tileSize) - position.y))
            {
                snappedPosition.y -= 0.5f * tileSize;
            }
            else
            {
                snappedPosition.y += 0.5f * tileSize;
            }
        }

        return snappedPosition;
    }

    public static void GetNumTile(BaseBlock block, out int numTileX, out int numTileY)
    {
        int minIndexX = block.tileIndexes.Min(item => item.x);
        int maxIndexX = block.tileIndexes.Max(item => item.x);
        int minIndexY = block.tileIndexes.Min(item => item.y);
        int maxIndexY = block.tileIndexes.Max(item => item.y);

        numTileX = (maxIndexX - minIndexX) + 1;
        numTileY = (maxIndexY - minIndexY) + 1;
    }

    #region Vacumn
    public Vector2 GetBlockVacumnPosition(BaseBlock block)
    {
        float centerX = block.CenterX;
        float centerY = block.CenterY;
        float originalCenterX = block.OriginalCenterX;
        float originalCenterY = block.OriginalCenterY;

        Vector2 vacumnPosition = new Vector2();

        vacumnPosition.x = (centerX - originalCenterX) * tileSize;
        vacumnPosition.y = -(centerY - originalCenterY) * tileSize;

        return vacumnPosition;
    }
    #endregion
}
