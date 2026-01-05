#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections.Generic;
using static GameEnum;
using Dreamteck.Utilities;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Saferio.Util.SaferioTween;

public class BoardGeneratorTool : EditorWindow
{
    private GameObject tilePrefab;
    private int numRow = 8;
    private int numColumn = 5;
    private float fillRate;
    private int iconIndex;
    private GameFaction faction;
    private float distanceRatio = 0f;
    private bool isIncludeSpecialBlocks;
    private Sprite previewIcon;
    GameFactionSpriteContainer iconContainer;

    private BaseBlock[] blocks;
    private BaseBarricade[] barricades;
    private Transform blockContainer;

    #region Freezed
    private GameObject freezedSprite;
    private GameObject freezedBlockCanvas;
    #endregion

    #region UTIL
    private float deltaPosition;
    #endregion

    private string folderPath = "Assets/Picture Block Jam/Prefab/Block/All Blocks";

    [MenuItem("Saferio/Level Tool")]
    public static void ShowWindow()
    {
        GetWindow<BoardGeneratorTool>("Level Tool");
    }

    private void LoadPrefabs()
    {
        GameObject[] prefabs = LoadAllPrefabsInFolder(folderPath);

        if (!isIncludeSpecialBlocks)
        {
            prefabs = LoadPrefabsInExactFolder(folderPath);
        }

        List<BaseBlock> blockList = new List<BaseBlock>();

        for (int i = 0; i < prefabs.Length; i++)
        {
            BaseBlock block = prefabs[i].GetComponent<BaseBlock>();

            if (block != null)
            {
                blockList.Add(block);
            }
        }

        blocks = blockList.ToArray();
    }

    private void OnEnable()
    {
        GameFactionSpriteContainer iconContainer =
            AssetDatabase.LoadAssetAtPath<GameFactionSpriteContainer>
            (
                "Assets/Scriptable Objects/Block Materials Container/Game Faction Sprite Container.asset"
            );
    }

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);

        GUILayout.Label("Board Generator Settings", EditorStyles.boldLabel);

        // tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);

        numColumn = EditorGUILayout.IntField("Number of Columns", numColumn);
        numRow = EditorGUILayout.IntField("Number of Rows", numRow);
        // isIncludeSpecialBlocks = EditorGUILayout.Toggle("Is Include Special Blocks", isIncludeSpecialBlocks);
        fillRate = EditorGUILayout.Slider("Fill Rate", fillRate, 0, 1);
        iconIndex = EditorGUILayout.IntField("Icon Index", iconIndex);
        faction = (GameFaction)EditorGUILayout.EnumFlagsField("Faction", faction);

        // blockContainer = (Transform)EditorGUILayout.ObjectField("Block Container", blockContainer, typeof(Transform));

        // if (GUILayout.Button("Snap"))
        // {
        //     SnapUsingRaycast(Selection.activeTransform);
        // }

        if (GUILayout.Button("Snap All"))
        {
            SnapAll();
        }

        // if (GUILayout.Button("Generate Board"))
        // {
        //     GenerateBoard(Selection.activeTransform);
        // }

        // if (GUILayout.Button("Generate Blocks"))
        // {
        //     GenerateBlocks(Selection.activeTransform);
        // }

        // if (GUILayout.Button("Generate Barricades"))
        // {
        //     GenerateBarricades(Selection.activeTransform, blockContainer);
        // }

        GUILayout.Space(16);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.padding = new RectOffset(20, 20, 20, 20);
        buttonStyle.fixedHeight = 60;

        if (GUILayout.Button("Generate Level", buttonStyle))
        {
            GenerateFullLevel();
        }

        if (GUILayout.Button("Set Icon"))
        {
            SetIcon(iconIndex);
        }

        if (GUILayout.Button("Snap Icons"))
        {
            PlaceBlockIcons();
        }

        if (GUILayout.Button("Set Faction"))
        {
            SetFaction(faction);
        }

        if (GUILayout.Button("Link 2 Blocks"))
        {
            LinkTwoBlocks();
        }

        if (GUILayout.Button("Link Many Blocks"))
        {
            LinkManyBlocks();
        }

        if (GUILayout.Button("Setup Board"))
        {
            SetupBoard();
        }

        if (GUILayout.Button("Set Board Colliders"))
        {
            SetBoardColliders();
        }

        if (GUILayout.Button("Assign Board Upper"))
        {
            AssignBoardUpper();
        }

        // if (GUILayout.Button("Calculate Difficulty", buttonStyle))
        // {
        //     CalculateDifficulty();
        // }

        // if (GUILayout.Button("Merge Gates", buttonStyle))
        // {
        //     MergeBarricades();
        // }

        deltaPosition = EditorGUILayout.FloatField("Delta Position", deltaPosition);

        if (GUILayout.Button("Change Position"))
        {
            ChangePositions();
        }

        if (GUILayout.Button("Paste Properties To Freezed Block"))
        {
            PastePropertiesToFreezedBlock();
        }

        GUILayout.EndArea();
    }

    private void GenerateFullLevel()
    {
        LoadPrefabs();

        Transform level = Selection.activeTransform;

        tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Board/Tile.prefab");

        GameObject BlockContainerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Containers/Block Container.prefab");

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < level.childCount; i++)
        {
            toRemoveList.Add(level.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item, allowDestroyingAssets: true);
        }

        Transform blockContainer = ((GameObject)PrefabUtility.InstantiatePrefab(BlockContainerPrefab, level)).transform;

        GenerateBoard();
        GenerateBlocks(blockContainer);
        // GenerateBarricades(barricadeContainer, blockContainer);
    }

    private void SnapAll()
    {
        Transform level = Selection.activeTransform;

        BaseBlock[] blocks = TransformUtil.GetComponentsFromAllChildren<BaseBlock>(level).ToArray();

        LevelBoard levelBoard = level.GetComponentInChildren<LevelBoard>();

        if (levelBoard != null)
        {
            GamePersistentVariable.levelBoard = levelBoard;
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].SnapImmediately();
        }

        EditorUtility.SetDirty(level);
    }

    private void SnapUsingRaycast(Transform target)
    {
        float tileDistance = GetTileDistance();

        BaseBlock block = target.GetComponent<BaseBlock>();

        SceneManager.GetActiveScene().GetPhysicsScene().Raycast(target.position, Vector3.down, out RaycastHit hit, 5);

        if (hit.collider != null)
        {
            Vector3 _snapPosition = hit.collider.transform.position;

            if (_snapPosition.x > target.position.x)
            {
                _snapPosition.x -= (block.BlockProperty.NumTileX - 1) / 2f * tileDistance;
            }
            else
            {
                _snapPosition.x += (block.BlockProperty.NumTileX - 1) / 2f * tileDistance;
            }

            if (_snapPosition.z > target.position.z)
            {
                _snapPosition.z -= (block.BlockProperty.NumTileZ - 1) / 2f * tileDistance;
            }
            else
            {
                _snapPosition.z += (block.BlockProperty.NumTileZ - 1) / 2f * tileDistance;
            }

            if (block.BlockProperty.NumTileX % 2 == 1)
            {
                _snapPosition.x = hit.collider.transform.position.x;
            }

            if (block.BlockProperty.NumTileZ % 2 == 1)
            {
                _snapPosition.z = hit.collider.transform.position.z;
            }

            _snapPosition.y = target.transform.position.y;

            target.transform.position = _snapPosition;
        }
    }

    private void Snap()
    {
        BaseBlock block = Selection.activeObject.GetComponent<BaseBlock>();

        float tileDistance = GetTileDistance();

        Vector3 position = block.transform.position;

        Vector2 coordinator;

        // covert to bottom-right position
        Vector3 bottomRightPosition = new Vector3();

        bottomRightPosition.x = block.transform.position.x + (block.BlockProperty.NumTileX - 1) / 2f * tileDistance;
        bottomRightPosition.z = block.transform.position.z - (block.BlockProperty.NumTileZ - 1) / 2f * tileDistance;

        coordinator.x = Mathf.Round(bottomRightPosition.x / tileDistance);
        coordinator.y = Mathf.Round(bottomRightPosition.z / tileDistance);

        Vector3 finalPosition = new Vector3(0, 2, 0);

        finalPosition.x = coordinator.x * tileDistance - (block.BlockProperty.NumTileX - 1) / 2f * tileDistance;
        finalPosition.z = coordinator.y * tileDistance + (block.BlockProperty.NumTileZ - 1) / 2f * tileDistance;

        block.transform.position = finalPosition;
    }

    private void GenerateBoard()
    {
        Transform level = Selection.activeTransform;

        GameObject boardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Picture Block Jam/Prefab/Board/Board.prefab");

        LevelBoard levelBoard = PrefabUtility.InstantiatePrefab(boardPrefab, level).GetComponent<LevelBoard>();

        levelBoard.numColumn = numColumn;
        levelBoard.numRow = numRow;

        float tileSize = GameConstants.tileSize;

        // Board Sprite
        BoardSpriteContainer boardSpriteContainer =
            AssetDatabase.LoadAssetAtPath<BoardSpriteContainer>
            (
                "Assets/Scriptable Objects/Board/Board Sprite Container.asset"
            );

        Sprite boardSprite = boardSpriteContainer.boardSprites.First(item => IsBoardSpriteMatch(numRow, numColumn, item));

        levelBoard.ground.sprite = boardSprite;

        AssignBoardUpper();
        SetBoardColliders();

        GamePersistentVariable.levelBoard = levelBoard;
    }

    private void GenerateBlocks(Transform container)
    {
        int totalTile = numRow * numColumn;
        int remainingTile = totalTile;
        float tileDistance = GameConstants.tileSize;

        bool[] isTileFull = new bool[remainingTile];

        int tryTime = 0;
        int maxTryTime = 100;

        BaseBlock selectedPrefab;

        List<BaseBlock> toGeneratedBlocks = new List<BaseBlock>();
        List<GameFaction> toGeneratedFaction = new List<GameFaction>();
        List<int> toGeneratedIconIndex = new List<int>();

        int maxFindBlockPairTryTime = 50;
        int findBlockPairTryTime = 0;

        iconContainer =
            AssetDatabase.LoadAssetAtPath<GameFactionSpriteContainer>
            (
                "Assets/Scriptable Objects/Block Materials Container/Game Faction Sprite Container.asset"
            );

        // Pick pairs
        while (remainingTile > (1 - fillRate) * totalTile && findBlockPairTryTime < maxFindBlockPairTryTime)
        {
            int random = Random.Range(0, blocks.Length);

            selectedPrefab = blocks[random];

            if (selectedPrefab.name.Contains("T-Shaped"))
            {
                continue;
            }

            BaseBlock pairBlock;

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == selectedPrefab)
                {
                    continue;
                }

                List<Vector2Int> combinedTileIndexes = new List<Vector2Int>();

                combinedTileIndexes.AddRange(selectedPrefab.tileIndexes);
                combinedTileIndexes.AddRange(blocks[i].tileIndexes.Select(item => new Vector2Int(item.x + 1, item.y)));

                bool containsAll = combinedTileIndexes.All(item => selectedPrefab.originalTileIndexes.Contains(item));

                Debug.Log("SAFERIO containsAll: " + containsAll + "/" + combinedTileIndexes.Count);

                if (containsAll && combinedTileIndexes.Count == selectedPrefab.originalTileIndexes.Count)
                {
                    pairBlock = blocks[i];

                    toGeneratedBlocks.Add(selectedPrefab);
                    toGeneratedBlocks.Add(pairBlock);

                    GameFaction faction = (GameFaction)Random.Range(0, 7);
                    int iconIndex = Random.Range(0, iconContainer.factionSprites.Length);

                    toGeneratedFaction.Add(faction);
                    toGeneratedFaction.Add(faction);
                    toGeneratedIconIndex.Add(iconIndex);
                    toGeneratedIconIndex.Add(iconIndex);

                    Debug.Log("SAFERIO pair: " + selectedPrefab.name + "/" + pairBlock.name);

                    remainingTile -= selectedPrefab.tileIndexes.Count;
                    remainingTile -= pairBlock.tileIndexes.Count;

                    break;
                }
            }

            findBlockPairTryTime++;
        }

        List<BaseBlock> generatedBlocks = new List<BaseBlock>();
        List<Vector2Int> filledTiles = new List<Vector2Int>();

        while (toGeneratedBlocks.Count > 0 && tryTime < maxTryTime)
        {
            Debug.Log("SAFERIO remainingTile: " + toGeneratedBlocks.Count + "/" + tryTime);

            selectedPrefab = toGeneratedBlocks[0];

            int sizeX = selectedPrefab.NumTileX;
            int sizeY = selectedPrefab.NumTileY;
            int numTile = sizeX * sizeY;

            Vector2Int coordinator = new Vector2Int();
            Vector2Int fallbackCoordinator = new Vector2Int(-1, -1);

            bool isFound = false;

            int randomStartY = Random.Range(0, numRow - 1);
            int randomStartX = Random.Range(0, numColumn - 1);

            coordinator = FindEmptySpace(selectedPrefab, filledTiles);

            tryTime++;

            toGeneratedBlocks.RemoveAt(0);

            BaseBlock block = PrefabUtility.InstantiatePrefab(selectedPrefab, container).GetComponent<BaseBlock>();

            generatedBlocks.Add(block);

            block.BlockServiceLocator.blockFaction.Faction = toGeneratedFaction[0];
            block.BlockServiceLocator.iconRenderer.sprite = iconContainer.factionSprites[toGeneratedIconIndex[0]];

            toGeneratedFaction.RemoveAt(0);
            toGeneratedIconIndex.RemoveAt(0);

            Vector3 placedPosition = new Vector3();

            placedPosition = TileMapper.TileToWorld(coordinator);

            block.transform.position = placedPosition;
            block.SnapImmediately();

            // #region LAYERED BLOCK
            // try
            // {
            //     BaseBlock insideBlock = TransformUtil.GetComponentsFromAllChildren<BaseBlock>(block.transform).ElementAt(1);

            //     insideBlock.BlockProperty.Faction = (GameEnum.GameFaction)RandomIntExceptValue(0, GameConstants.SCREW_FACTION.Length, randomFaction);
            //     insideBlock.BlockServiceLocator.blockMaterialPropertyBlock.SetFaction();
            // }
            // catch (System.Exception e)
            // {

            // }
            // #endregion
        }

        for (int i = 0; i < generatedBlocks.Count; i += 2)
        {
            LinkTwoBlocks(generatedBlocks[i], generatedBlocks[i + 1]);
        }

        for (int i = 0; i < generatedBlocks.Count; i++)
        {
            PlaceBlockIcon(generatedBlocks[i]);
        }

        #region FREEZED BLOCK
        try
        {
            List<BaseBlock> spawnedBlocks = TransformUtil.GetComponentsFromAllChildren<BaseBlock>(container);
            List<FreezeBlock> freezeBlocks = TransformUtil.GetComponentsFromAllChildren<FreezeBlock>(container);

            int totalSpawnBlocks = spawnedBlocks.Count;

            for (int i = 0; i < spawnedBlocks.Count; i++)
            {
                FreezeBlock freezeBlock = spawnedBlocks[i].GetComponent<FreezeBlock>();

                if (freezeBlock != null)
                {
                    freezeBlock.RemainingBlockToMelt = Random.Range(1, Mathf.Min(totalSpawnBlocks - freezeBlocks.Count, 9));
                    freezeBlock.RefreshRemainingBlockToMeltText();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("SAFERIO " + e.Message);
        }
        #endregion

        EditorUtility.SetDirty(Selection.activeGameObject);

        bool IsBlockTileEmpty(int index, BaseBlock block)
        {
            for (int i = 0; i < block.BlockProperty.EmptyTileIndexes.Length; i++)
            {
                if (index == block.BlockProperty.EmptyTileIndexes[i])
                {
                    return true;
                }
            }

            return false;
        }
    }

    private Vector2Int FindEmptySpace(BaseBlock block, List<Vector2Int> filledTiles)
    {
        int halfColumn = (numColumn - numColumn % 2) / 2;
        int halfRow = (numRow - numRow % 2) / 2;

        int startX = -halfRow + 1;
        int endX = halfRow;
        int startY = -halfColumn + 1;
        int endY = halfColumn;

        bool isValid;

        for (int i = startX; i < endX; i++)
        {
            for (int j = startY; j < endY; j++)
            {
                isValid = true;

                foreach (var tileIndex in block.tileIndexes)
                {
                    if (filledTiles.Contains(new Vector2Int(i + tileIndex.x, j + tileIndex.y)))
                    {
                        isValid = false;

                        break;
                    }
                }

                if (isValid)
                {
                    foreach (var tileIndex in block.tileIndexes)
                    {
                        filledTiles.Add(new Vector2Int(i + tileIndex.x, j + tileIndex.y));
                    }

                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(startX, startY);
    }

    private void SetIcon(int index)
    {
        BaseBlock block = Selection.activeTransform.GetComponent<BaseBlock>();

        GameFactionSpriteContainer iconContainer =
            AssetDatabase.LoadAssetAtPath<GameFactionSpriteContainer>
            (
                "Assets/Scriptable Objects/Block Materials Container/Game Faction Sprite Container.asset"
            );

        block.BlockServiceLocator.iconRenderer.sprite = iconContainer.factionSprites[index];

        EditorUtility.SetDirty(Selection.activeGameObject);
    }

    private void SetFaction(GameFaction faction)
    {
        BaseBlock block = Selection.activeTransform.GetComponent<BaseBlock>();

        block.BlockServiceLocator.blockFaction.Faction = faction;

        EditorUtility.SetDirty(Selection.activeGameObject);
    }

    private void LinkTwoBlocks()
    {
        Transform[] transforms = Selection.transforms;

        BaseBlock[] blocks = new BaseBlock[2];

        blocks[0] = transforms[0].GetComponent<BaseBlock>();
        blocks[1] = transforms[1].GetComponent<BaseBlock>();

        LinkTwoBlocks(blocks[0], blocks[1]);
    }

    private void LinkManyBlocks()
    {
        Transform[] transforms = Selection.transforms;

        List<BaseBlock> blocks = new List<BaseBlock>();

        foreach (var transform in transforms)
        {
            blocks.Add(transform.GetComponent<BaseBlock>());
        }

        LinkBlocks(blocks);
    }

    private void LinkTwoBlocks(BaseBlock firstBlock, BaseBlock secondBlock)
    {
        int maxTileX = Mathf.Max(firstBlock.NumTileX, secondBlock.NumTileX);
        int maxTileY = Mathf.Max(firstBlock.NumTileY, secondBlock.NumTileY);

        List<Vector2Int> originalTileIndexes = BuildTileIndexes(maxTileX, maxTileY);

        firstBlock.originalTileIndexes = originalTileIndexes;
        secondBlock.originalTileIndexes = originalTileIndexes;

        if (firstBlock.tileIndexes.Count > secondBlock.tileIndexes.Count)
        {
            secondBlock.tileIndexes = originalTileIndexes.Except(firstBlock.tileIndexes).ToList();
        }
        else
        {
            firstBlock.tileIndexes = originalTileIndexes.Except(secondBlock.tileIndexes).ToList();
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void LinkBlocks(List<BaseBlock> blocks)
    {
        int maxTileX = blocks.Max(item => item.NumTileX);
        int maxTileY = blocks.Max(item => item.NumTileY);

        List<Vector2Int> originalTileIndexes = BuildTileIndexes(maxTileX, maxTileY);

        foreach (var block in blocks)
        {
            block.originalTileIndexes = originalTileIndexes;
        }

        BaseBlock blockWithMostTiles = blocks.OrderByDescending(item => item.tileIndexes.Count).FirstOrDefault();

        foreach (var block in blocks)
        {
            if (block != blockWithMostTiles)
            {
                block.tileIndexes = originalTileIndexes.Except(blockWithMostTiles.tileIndexes).ToList();

                block.BlockServiceLocator.iconRenderer.sprite = blockWithMostTiles.BlockServiceLocator.iconRenderer.sprite;
                block.BlockServiceLocator.blockFaction.Faction = blockWithMostTiles.BlockServiceLocator.blockFaction.Faction;
            }
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void PlaceBlockIcon(BaseBlock block)
    {
        float tileSize = GameConstants.tileSize;

        float sizeX = block.NumTileX * tileSize;
        float sizeY = block.NumTileY * tileSize;

        Vector2 iconCenterIndex;

        iconCenterIndex.x = (block.originalTileIndexes.Max(item => item.x) + block.originalTileIndexes.Min(item => item.x)) / 2f;
        iconCenterIndex.y = (block.originalTileIndexes.Max(item => item.y) + block.originalTileIndexes.Min(item => item.y)) / 2f;

        Vector2 blockCenterIndex;

        if (block.tileIndexes.Count > 1)
        {
            blockCenterIndex.x = (block.tileIndexes.Max(item => item.x) + block.tileIndexes.Min(item => item.x)) / 2f;
            blockCenterIndex.y = (block.tileIndexes.Max(item => item.y) + block.tileIndexes.Min(item => item.y)) / 2f;
        }
        else
        {
            blockCenterIndex = block.tileIndexes[0];
        }

        Vector2 indexDistanceToCenter = new Vector2(iconCenterIndex.x - blockCenterIndex.x, blockCenterIndex.y - iconCenterIndex.y);

        Vector2 iconLocalPosition = indexDistanceToCenter * tileSize;

        block.BlockServiceLocator.iconRenderer.transform.localPosition = iconLocalPosition;
    }

    private void PlaceBlockIcons()
    {
        Transform[] transforms = Selection.transforms;

        foreach (var transform in transforms)
        {
            PlaceBlockIcon(transform.GetComponent<BaseBlock>());
        }
    }

    #region Board
    private void SetupBoard()
    {
        LevelBoard board = Selection.activeTransform.GetComponentInChildren<LevelBoard>();

        SpriteRenderer ground = board.transform.GetChild(0).GetComponent<SpriteRenderer>();

        int numColumn;
        int numRow;

        ParseSpriteName(ground.sprite.name, out numRow, out numColumn);

        board.numColumn = numColumn;
        board.numRow = numRow;

        ground.drawMode = SpriteDrawMode.Simple;
        ground.transform.localScale = Vector3.one;

        SetBoardColliders();

        void ParseSpriteName(string spriteName, out int numRows, out int numCols)
        {
            string[] parts = spriteName.Split('x');

            if (parts.Length == 2)
            {
                int.TryParse(parts[0], out numRows);
                int.TryParse(parts[1], out numCols);
            }
            else
            {
                numRows = 0;
                numCols = 0;
            }
        }
    }

    private void AssignBoardUpper()
    {
        LevelBoard board = Selection.activeTransform.GetComponentInChildren<LevelBoard>();

        BoardSpriteContainer boardSpriteContainer =
            AssetDatabase.LoadAssetAtPath<BoardSpriteContainer>
            (
                "Assets/Scriptable Objects/Board/Board Sprite Container.asset"
            );

        SpriteRenderer boardRenderer = board.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer boardUpperRenderer = board.transform.GetChild(1).GetComponent<SpriteRenderer>();

        Sprite boardUpper = boardSpriteContainer.boardUpperSprites.First(item => item.name.Contains(boardRenderer.sprite.name));

        boardUpperRenderer.sprite = boardUpper;

        EditorUtility.SetDirty(board);
    }

    public bool IsBoardSpriteMatch(int row, int column, Sprite sprite)
    {
        // Get the name of the sprite
        string spriteName = sprite.name; // Assuming it's in the format "mxn"

        // Split the name by the 'x' to separate the row and column
        string[] parts = spriteName.Split('x');

        if (parts.Length == 2)
        {
            // Parse the row and column values
            int spriteRow, spriteColumn;

            bool rowMatch = int.TryParse(parts[0], out spriteRow);
            bool colMatch = int.TryParse(parts[1], out spriteColumn);

            // Check if both row and column match
            if (rowMatch && colMatch)
            {
                return spriteRow == row && spriteColumn == column;
            }
        }

        return false;
    }
    #endregion

    #region Collider
    private void SetBoardColliders()
    {
        Transform level = Selection.activeTransform;

        LevelBoard levelBoard = level.GetComponentInChildren<LevelBoard>();

        float tileSize = GameConstants.tileSize;
        Vector2 boardSize = tileSize * new Vector2(levelBoard.numColumn, levelBoard.numRow);

        SetBoxColliderWorldSize(levelBoard.leftEdgeCollider, new Vector2(0.1f * boardSize.x, 2 * boardSize.y));
        SetBoxColliderWorldSize(levelBoard.rightEdgeCollider, new Vector2(0.1f * boardSize.x, 2 * boardSize.y));
        SetBoxColliderWorldSize(levelBoard.topEdgeCollider, new Vector2(2 * boardSize.x, 0.1f * boardSize.y));
        SetBoxColliderWorldSize(levelBoard.bottomEdgeCollider, new Vector2(2 * boardSize.x, 0.1f * boardSize.y));

        levelBoard.leftEdgeCollider.transform.localPosition = new Vector2(-(levelBoard.numColumn + 1) / 2f * tileSize, 0);
        levelBoard.rightEdgeCollider.transform.localPosition = new Vector2((levelBoard.numColumn + 1) / 2f * tileSize, 0);
        levelBoard.topEdgeCollider.transform.localPosition = new Vector2(0, (levelBoard.numRow + 1) / 2f * tileSize);
        levelBoard.bottomEdgeCollider.transform.localPosition = new Vector2(0, -(levelBoard.numRow + 1) / 2f * tileSize);

        EditorUtility.SetDirty(level);
    }
    #endregion

    #region Freezed Block
    private void PastePropertiesToFreezedBlock()
    {
        FreezeBlock freezeBlock = Selection.transforms[0].GetComponent<FreezeBlock>();
        BaseBlock block = Selection.transforms[1].GetComponent<BaseBlock>();

        freezeBlock.gameFactionSpriteContainer = block.gameFactionSpriteContainer;
        freezeBlock.originalTileIndexes = block.originalTileIndexes;
        freezeBlock.tileIndexes = block.tileIndexes;

        freezeBlock.GetComponent<BlockFaction>().Faction = block.GetComponent<BlockFaction>().Faction;
        freezeBlock.GetComponent<BlockMaterialPropertyBlock>().icon.sprite =
            block.GetComponent<BlockMaterialPropertyBlock>().icon.sprite;

        EditorUtility.SetDirty(freezeBlock);
    }
    #endregion

    #region DIFFICULTY
    private void CalculateDifficulty()
    {
        Transform level = Selection.activeTransform;

        float tileDistance = GetTileDistance();

        BoardTile[] boardTiles = TransformUtil.GetComponentsFromAllChildren<BoardTile>(level).ToArray();
        BaseBlock[] blocks = TransformUtil.GetComponentsFromAllChildren<BaseBlock>(level).ToArray();
        BaseBarricade[] barricades = TransformUtil.GetComponentsFromAllChildren<BaseBarricade>(level).ToArray();

        int totalTile = boardTiles.Length;
        int totalTileTaken = 0;

        for (int i = 0; i < blocks.Length; i++)
        {
            bool isLayeredBlock = blocks[i].transform.parent.GetComponent<BaseBlock>() != null;
            bool isFreezedBlock = blocks[i].GetComponent<FreezeBlock>() != null;

            totalTileTaken += blocks[i].BlockProperty.NumTileX * blocks[i].BlockProperty.NumTileZ - blocks[i].BlockProperty.EmptyTileIndexes.Length;

            if (isLayeredBlock || isFreezedBlock)
            {
                totalTileTaken += 1;
            }
        }

        float totalDistance = 0;

        Dictionary<Vector2Int, int> mapTiles = ConvertToIntMap(boardTiles, blocks);

        for (int i = 0; i < blocks.Length; i++)
        {
            float distance = float.MaxValue;
            bool isFound = false;

            for (int j = 0; j < barricades.Length; j++)
            {
                if (barricades[j].Faction == blocks[i].BlockProperty.Faction)
                {
                    distance = Mathf.Min(distance, Vector3.Distance(barricades[j].transform.position, blocks[i].transform.position));

                    Vector2Int startPos = new Vector2Int((int)blocks[i].transform.position.x, (int)blocks[i].transform.position.z);
                    Vector2Int targetPos = new Vector2Int((int)barricades[j].transform.position.x, (int)barricades[j].transform.position.z);

                    float min = float.MaxValue;

                    for (int k = 0; k < boardTiles.Length; k++)
                    {
                        float barricadeDistance = Vector3.Distance(barricades[j].transform.position, boardTiles[k].transform.position);

                        if (barricadeDistance < min)
                        {
                            targetPos = new Vector2Int((int)boardTiles[k].transform.position.x, (int)boardTiles[k].transform.position.z);

                            min = barricadeDistance;
                        }
                    }

                    Vector2Int blockSize = new Vector2Int(blocks[i].BlockProperty.NumTileX, blocks[i].BlockProperty.NumTileZ);

                    int moveDifficulty = GetMoveDifficulty(startPos, targetPos, blockSize, mapTiles);

                    if (moveDifficulty != -1)
                    {
                        for (int k = 0; k < blocks[i].BlockProperty.NumTileZ; k++)
                        {
                            Vector3 blockTilePosition = (blocks[i].transform.position
                                 + new Vector3(0, 0, 0.5f * (blocks[i].BlockProperty.NumTileZ * tileDistance - tileDistance) - k * tileDistance));

                            for (int m = 0; m < boardTiles.Length; m++)
                            {
                                Vector2Int mapTilePosition = mapTiles.Keys.ElementAt(m);

                                if (new Vector3(mapTilePosition.x, blockTilePosition.y, mapTilePosition.y) == blockTilePosition)
                                {
                                    mapTiles[mapTilePosition] = 0;
                                }
                            }
                        }
                    }

                    isFound = true;
                }
            }

            if (isFound)
            {
                totalDistance += distance;
            }
        }

        float WEIGHT_TILE_DENSITY = 0.8f;
        float WEIGHT_NUM_TILE = 0.2f;
        float WEIGHT_GATE_DISTANCE = 0.5f;

        float MAX_TILE = 16 * 16;

        float tileDensityDifficulty = (float)totalTileTaken / totalTile;
        float gateDistanceDifficulty = totalDistance / 1000f;

        int totalBlocks = blocks.Length;
        int maxBlocks = blocks.Length;

        float finalDifficulty = (WEIGHT_TILE_DENSITY * tileDensityDifficulty + WEIGHT_GATE_DISTANCE * gateDistanceDifficulty) * 1000;

        finalDifficulty = tileDensityDifficulty * (float)totalBlocks / maxBlocks * 1000;

        // Debug.Log("SAFERIO " + totalTileTaken + "/" + totalTile + "/" + (float)totalTileTaken / totalTile);
        // Debug.Log("SAFERIO Distance " + totalDistance);
        Debug.Log("Level Difficulty: " + finalDifficulty);
    }

    // 0 = walkable, 1 = obstacle
    private Dictionary<Vector2Int, int> ConvertToIntMap(BoardTile[] boardTiles, BaseBlock[] blocks)
    {
        float tileDistance = GetTileDistance();
        Dictionary<Vector2Int, int> mapTiles = new Dictionary<Vector2Int, int>();

        for (int i = 0; i < boardTiles.Length; i++)
        {
            mapTiles.Add(
                new Vector2Int((int)boardTiles[i].transform.position.x, (int)boardTiles[i].transform.position.z), 0);
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            for (int j = 0; j < blocks[i].BlockProperty.NumTileZ; j++)
            {
                Vector3 blockTilePosition = (blocks[i].transform.position
                     + new Vector3(0, 0, 0.5f * (blocks[i].BlockProperty.NumTileZ * tileDistance - tileDistance) - j * tileDistance));

                for (int k = 0; k < boardTiles.Length; k++)
                {
                    Vector2Int mapTilePosition = mapTiles.Keys.ElementAt(k);

                    // if(mapTilePosition.x == 4) {
                    //     Debug.Log("SAFERIO " + new Vector3(mapTilePosition.x, 0, mapTilePosition.y) + "/" + blockTilePosition);
                    // }


                    if (new Vector3(mapTilePosition.x, blockTilePosition.y, mapTilePosition.y) == blockTilePosition)
                    {
                        mapTiles[mapTilePosition] = 1;
                    }
                }

                // Debug.Log(
                //     "SAFERIO " + (blocks[i].transform.position
                //      + new Vector3(0, 0, 0.5f * (blocks[i].BlockProperty.NumTileZ * tileDistance - tileDistance) - j * tileDistance)));
            }



            for (int j = 0; j < mapTiles.Count; j++)
            {
                Vector2Int mapTilePosition = mapTiles.Keys.ElementAt(j);

                if (
                    new Vector3(mapTilePosition.x, 0, mapTilePosition.y) ==
                    blocks[i].transform.position - new Vector3(0 * (blocks[i].BlockProperty.NumTileX - 1), 0, 0) * tileDistance)
                {
                    mapTiles[mapTilePosition] = 1;

                    break;
                }

                if (
                    new Vector3(mapTilePosition.x, 0, mapTilePosition.y) ==
                    blocks[i].transform.position + new Vector3(0 * (blocks[i].BlockProperty.NumTileX - 1), 0, 0) * tileDistance)
                {
                    mapTiles[mapTilePosition] = 1;

                    break;
                }

                if (
                    new Vector3(mapTilePosition.x, 0, mapTilePosition.y) ==
                    blocks[i].transform.position - new Vector3(0, 0, 0 * (blocks[i].BlockProperty.NumTileZ - 1)) * tileDistance)
                {
                    mapTiles[mapTilePosition] = 1;

                    break;
                }

                if (
                    new Vector3(mapTilePosition.x, 0, mapTilePosition.y) ==
                    blocks[i].transform.position + new Vector3(0, 0, 0 * (blocks[i].BlockProperty.NumTileZ - 1)) * tileDistance)
                {
                    mapTiles[mapTilePosition] = 1;

                    break;
                }
            }
        }

        return mapTiles;
    }

    public int GetMoveDifficulty(Vector2Int startPos, Vector2Int targetPos, Vector2Int blockSize, Dictionary<Vector2Int, int> mapTiles)
    {
        Queue<(Vector2Int pos, int steps)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue((startPos, 0));
        visited.Add(startPos);

        Vector2Int[] directions = {
            new Vector2Int(4, 0),
            new Vector2Int(-4, 0),
            new Vector2Int(0, 4),
            new Vector2Int(0, -4)
        };

        while (queue.Count > 0)
        {
            var (current, steps) = queue.Dequeue();

            if (current == targetPos)
                return steps;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (!visited.Contains(next) && IsAreaFree(next, blockSize, mapTiles))
                {
                    visited.Add(next);
                    queue.Enqueue((next, steps + 1));
                }
            }
        }

        return -1; // Target unreachable
    }

    private bool IsAreaFree(Vector2Int pos, Vector2Int size, Dictionary<Vector2Int, int> mapTiles)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int tilePos = new Vector2Int(pos.x + x * 4, pos.y + y * 4);

                if (!mapTiles.ContainsKey(tilePos) || mapTiles[tilePos] != 0)
                    return false;
            }
        }
        return true;
    }
    #endregion

    #region Transform Util
    private void ChangePositions()
    {
        Transform[] transforms = Selection.transforms;

        foreach (var item in transforms)
        {
            item.transform.localPosition += new Vector3(0, deltaPosition);
        }
    }
    #endregion

    #region UTIL
    bool IsValidArea(bool[] isTileFull, int startX, int startZ, int sizeX, int sizeZ)
    {
        for (int i = startZ; i <= startZ + sizeZ - 1; i++)
        {
            if (i >= numRow)
            {
                return false;
            }

            for (int j = startX; j <= startX + sizeX - 1; j++)
            {
                if (j >= numColumn)
                {
                    return false;
                }

                if (isTileFull[j + numColumn * i])
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool IsValidArea(bool[] isTileFull, int startX, int startZ, int sizeX, int sizeZ, int numRow, int numColumn)
    {
        for (int i = startZ; i <= startZ + sizeZ - 1; i++)
        {
            if (i >= numRow)
            {
                return false;
            }

            for (int j = startX; j <= startX + sizeX - 1; j++)
            {
                if (j >= numColumn)
                {
                    return false;
                }

                if (isTileFull[j + numColumn * i])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private float GetTileDistance()
    {
        tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Board/Tile.prefab");

        return (1 + distanceRatio) * tilePrefab.GetComponent<MeshRenderer>().bounds.size.x;
    }

    private GameObject[] LoadAllPrefabsInFolder(string folder)
    {
        List<GameObject> prefabs = new List<GameObject>();

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folder });

        // if (prefabGUIDs.Length == 0)
        // {
        //     Debug.LogWarning("No prefabs found in folder: " + folder);
        //     return nu;
        // }

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        return prefabs.ToArray();
    }

    private GameObject[] LoadPrefabsInExactFolder(string folder)
    {
        List<GameObject> prefabs = new List<GameObject>();

        // Make sure the folder path ends with a slash
        if (!folder.EndsWith("/"))
            folder += "/";

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folder });

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Check if the asset is in the exact folder (not a subfolder)
            if (Path.GetDirectoryName(path).Replace("\\", "/") + "/" == folder)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }
        }

        return prefabs.ToArray();
    }

    public void SetBoxColliderWorldSize(BoxCollider2D boxCollider, Vector2 targetSize)
    {
        Vector3 scale = boxCollider.transform.lossyScale;

        boxCollider.size = new Vector2(
            targetSize.x / scale.x,
            targetSize.y / scale.y
        );
    }

    private void RemoveAllGameObjectsWithComponent<T>(Transform parent)
    {
        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).GetComponent<T>() != null)
            {
                toRemoveList.Add(parent.GetChild(i).gameObject);
            }
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item, allowDestroyingAssets: true);
        }
    }

    List<Vector2Int> BuildTileIndexes(int numTileX, int numTileY)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int y = 0; y < numTileY; y++)
        {
            for (int x = 0; x < numTileX; x++)
            {
                tiles.Add(new Vector2Int(x, y));
            }
        }

        return tiles;
    }

    private int RandomIntExceptValue(int from, int to, int except)
    {
        List<int> pool = new List<int>();

        for (int i = from; i <= to; i++)
        {
            int faction = i;

            if (faction != except)
            {
                pool.Add(faction);
            }
        }

        int random = pool[Random.Range(0, pool.Count)];

        return random;
    }
    #endregion
}
#endif
