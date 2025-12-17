using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private GameObject roundedCornerPrefab;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;

    private Camera _mainCamera;
    private int _maxLevel;

    public static event Action startLevelEvent;
    public static event Action setLevelNumberEvent;
    public static event Action<int> setLevelScrewNumberEvent;
    public static event Action<int> setMultiPhaseLevelScrewNumberEvent;
    public static event Action showSceneTransitionEvent;
    public static event Action<BaseBlock[]> sendLevelBaseBlocksDataEvent;
    public static event Action<float, float, float, float> updateBoundEvent;
    public static event Action<float> setLevelCameraOrthographicSize;
    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action setIsLevelLoadedEvent;

    private void Awake()
    {
        PausePopup.replayLevelEvent += Replay;
        ReplayPopup.replayLevelEvent += Replay;
        LosePopup.replayLevelEvent += Replay;
        GameplayScreen.replayLevelEvent += Replay;
        DebugPopup.toLevelEvent += GoLevel;
        DebugPopup.nextLevelEvent += NextLevel;
        DebugPopup.prevLevelEvent += PrevLevel;
        WinPopup.nextLevelEvent += NextLevel;
        WinPopup.goLevelEvent += GoLevel;

        currentLevel.Load();

        _mainCamera = Camera.main;
        _maxLevel = levelDataContainer.MaxLevel;

        GoLevel(currentLevel.Value);
    }

    void OnDestroy()
    {
        PausePopup.replayLevelEvent -= Replay;
        ReplayPopup.replayLevelEvent -= Replay;
        LosePopup.replayLevelEvent -= Replay;
        GameplayScreen.replayLevelEvent -= Replay;
        DebugPopup.toLevelEvent -= GoLevel;
        DebugPopup.nextLevelEvent -= NextLevel;
        DebugPopup.prevLevelEvent -= PrevLevel;
        WinPopup.nextLevelEvent -= NextLevel;
        WinPopup.goLevelEvent -= GoLevel;
    }

    private async Task LoadLevel()
    {
        GamePersistentVariable.currentLevel = currentLevel.Value;

        int nextLevel = 1;

        bool isLoopLevel = false;

        if (!UserData.IsLoopLevel)
        {
            if (currentLevel.Value <= _maxLevel)
            {
                nextLevel = currentLevel.Value;
            }
            else
            {
                nextLevel = UnityEngine.Random.Range(11, _maxLevel);

                await UserData.SetIsLoopLevel(true);
                await UserData.SetIsLastLevelBeforeLoop(_maxLevel);
            }
        }
        else
        {
            if (currentLevel.Value - UserData.NumLevelLoop >= _maxLevel)
            {
                nextLevel = UnityEngine.Random.Range(11, _maxLevel);

                // await UserData.SetNumLevelLoop(UserData.NumLevelLoop + 1);
            }
            else
            {
                nextLevel = currentLevel.Value - UserData.NumLevelLoop;
            }

            isLoopLevel = true;
        }

        // // NOT LOOP
        // if (isLoopLevel)
        // {
        //     showSceneTransitionEvent?.Invoke();
        //     setIsLevelLoadedEvent?.Invoke();
        //     switchRouteEvent?.Invoke(ScreenRoute.LevelComingSoon);

        //     return;
        // }

        nextLevel = Mathf.Clamp(nextLevel, 1, _maxLevel);

        setLevelNumberEvent?.Invoke();

        nextLevel = RemoteConfigLevelManager.GetReplacedLevelIndex(nextLevel);

        GamePersistentVariable.levelPrefab = nextLevel;

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>($"Level {nextLevel}");

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = Instantiate(op.Result, transform);

                GamePersistentVariable.isLevelDirty = false;

                // MANUAL CAMERA FOV MODIFIER
                // LevelCameraModifier levelCameraModifier = level.GetComponent<LevelCameraModifier>();

                // if (levelCameraModifier != null)
                // {
                //     setLevelCameraOrthographicSize?.Invoke(levelCameraModifier.FieldOfView);
                // }
                // else
                // {
                //     setLevelCameraOrthographicSize?.Invoke(GameConstants.DEFAULT_CAMERA_FIELD_OF_VIEW);
                // }

                ManageBaseBlocks(level.transform);

                AutoFitLevelCamera();

                // AssignPhysicsMaterialAll(level.transform);

                if (!isLoopLevel)
                {
                    SaferioTracking.TrackLevelStart(currentLevel.Value);
                }

                GamePersistentVariable.isLevelLost = false;
            }
        };

        showSceneTransitionEvent?.Invoke();
    }

    private async void GoLevel(int level)
    {
        await Task.Delay(500);

        if (gameObject.transform.childCount > 0)
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }

        currentLevel.Value = level;

        currentLevel.Save();

        await currentLevel.SaveAsync();

        startLevelEvent?.Invoke();

        LoadLevel();
    }

    private void NextLevel()
    {
        GoLevel(currentLevel.Value + 1);
    }

    private void PrevLevel()
    {
        GoLevel(currentLevel.Value - 1);
    }

    private void Replay()
    {
        GoLevel(currentLevel.Value);
    }

    private void AssignPhysicsMaterialAll(Transform level)
    {
        // Physics Material
        PhysicsMaterial physicsMaterial = new PhysicsMaterial();

        physicsMaterial.staticFriction = 0;
        physicsMaterial.dynamicFriction = 0;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;

        Collider[] colliders = TransformUtil.GetComponentsFromAllChildren<Collider>(level).ToArray();

        foreach (var collider in colliders)
        {
            collider.material = physicsMaterial;
        }
    }

    private void ManageBaseBlocks(Transform level)
    {
        BaseBlock[] blocks = TransformUtil.GetComponentsFromAllChildren<BaseBlock>(level).ToArray();

        sendLevelBaseBlocksDataEvent?.Invoke(blocks);

        Debug.Log("SAFERIO Check later");
        return;

        BoardTile[] boardTiles = TransformUtil.GetComponentsFromAllChildren<BoardTile>(level).ToArray();
        BaseBarricade[] barricades = TransformUtil.GetComponentsFromAllChildren<BaseBarricade>(level).ToArray();

        // // Combine meshes
        // MeshFilter[] boardTileMeshes = new MeshFilter[boardTiles.Length];

        // for (int i = 0; i < boardTiles.Length; i++)
        // {
        //     boardTileMeshes[i] = boardTiles[i].GetComponent<MeshFilter>();
        // }

        // CombineInstance[] combine = new CombineInstance[boardTileMeshes.Length];

        // for (int i = 0; i < boardTileMeshes.Length; i++)
        // {
        //     combine[i].mesh = boardTileMeshes[i].sharedMesh;
        //     combine[i].transform = boardTileMeshes[i].transform.localToWorldMatrix;
        //     boardTileMeshes[i].gameObject.SetActive(false);
        // }

        // Mesh combinedMesh = new Mesh();
        // combinedMesh.CombineMeshes(combine);

        // GameObject combinedObject = new GameObject("Combined Board Tiles");
        // MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        // MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();

        // meshFilter.mesh = combinedMesh;
        // meshRenderer.material = boardTileMeshes[0].GetComponent<MeshRenderer>().sharedMaterial;
        // //

        float tileSize = boardTiles[0].GetComponent<MeshRenderer>().bounds.size.x;

        float rightBound = 0;
        float leftBound = float.MaxValue;
        float topBound = 0;
        float bottomBound = float.MaxValue;

        for (int i = 0; i < boardTiles.Length; i++)
        {
            Vector3 position = boardTiles[i].transform.position;

            if (position.x > rightBound)
            {
                rightBound = position.x;
            }

            if (position.x < leftBound)
            {
                leftBound = position.x;
            }

            if (position.z > topBound)
            {
                topBound = position.z;
            }

            if (position.z < bottomBound)
            {
                bottomBound = position.z;
            }
        }

        rightBound += 0.5f * tileSize;
        leftBound -= 0.5f * tileSize;
        topBound += 0.5f * tileSize;
        bottomBound -= 0.5f * tileSize;

        updateBoundEvent?.Invoke(rightBound, leftBound, topBound, bottomBound);

        for (int i = 0; i < barricades.Length; i++)
        {
            if (barricades[i].transform.position.x <= leftBound)
            {
                barricades[i].Direction = Direction.Left;

                barricades[i].transform.eulerAngles = new Vector3(0, 90, 0);
            }
            else if (barricades[i].transform.position.x >= rightBound)
            {
                barricades[i].Direction = Direction.Right;

                barricades[i].transform.eulerAngles = new Vector3(0, 270, 0);
            }
            else if (barricades[i].transform.position.z >= topBound)
            {
                barricades[i].Direction = Direction.Up;

                barricades[i].transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else if (barricades[i].transform.position.z <= bottomBound)
            {
                barricades[i].Direction = Direction.Down;

                barricades[i].transform.eulerAngles = new Vector3(0, 0, 0);
            }

            barricades[i].GateCollider.isTrigger = true;

            if (barricades[i].BarricadeProperty.Direction == Direction.Up)
            {
                if (barricades[i].transform.position.z < topBound)
                {
                    barricades[i].GateCollider.isTrigger = false;
                }
            }
            else if (barricades[i].BarricadeProperty.Direction == Direction.Down)
            {
                if (barricades[i].transform.position.z > bottomBound)
                {
                    barricades[i].GateCollider.isTrigger = false;
                }
            }
            else if (barricades[i].BarricadeProperty.Direction == Direction.Right)
            {
                if (barricades[i].transform.position.x < rightBound)
                {
                    barricades[i].GateCollider.isTrigger = false;
                }
            }
            else if (barricades[i].BarricadeProperty.Direction == Direction.Left)
            {
                if (barricades[i].transform.position.x > leftBound)
                {
                    barricades[i].GateCollider.isTrigger = false;
                }
            }
        }

        // CAMERA
        float fieldOfView;

        if (rightBound < 12)
        {
            fieldOfView = 50 + 0.6f * rightBound;
        }
        else
        {
            fieldOfView = 50 + 1.7f * rightBound;
        }

        // ManualCameraFOV manualCameraFOV = level.GetComponent<ManualCameraFOV>();

        // if (manualCameraFOV == null)
        // {
        //     setLevelCameraOrthographicSize?.Invoke(fieldOfView + GamePersistentVariable.canvasSize.x / 1080f * 5);
        // }
        // else
        // {
        //     setLevelCameraOrthographicSize?.Invoke(manualCameraFOV.fieldOfView + GamePersistentVariable.canvasSize.x / 1080f * 5);
        // }

        fieldOfView = GetFitFOV(1.3f * (rightBound * 2), 1.3f * 1.6f * (topBound * 2));

        setLevelCameraOrthographicSize(fieldOfView);

        float GetFitFOV(float expectedScreenSizeX, float expectedScreenSizeY)
        {
            for (int i = 50; i < 90; i++)
            {
                _mainCamera.fieldOfView = i;

                Vector2 screenSizeWorld = CommonUtil.GetScreenWorldSizePerspective(_mainCamera, _mainCamera.transform.position.y);

                if (screenSizeWorld.x > expectedScreenSizeX
                    && screenSizeWorld.y > expectedScreenSizeY
                )
                {
                    return i;
                }
            }

            return 90;
        }

        // Generate bound collider
        // Physics Material
        PhysicsMaterial physicsMaterial = new PhysicsMaterial();

        physicsMaterial.staticFriction = 0;
        physicsMaterial.dynamicFriction = 0;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;

        GameObject boundContainer = new GameObject("Bound Container");

        boundContainer.transform.SetParent(barricades[0].transform.parent.parent);

        GameObject topBoundCollider = new GameObject("topBoundCollider");

        topBoundCollider.transform.SetParent(boundContainer.transform);
        topBoundCollider.transform.position = new Vector3(0, 0, topBound + 1);
        BoxCollider topBoundColliderComponent = topBoundCollider.AddComponent<BoxCollider>();
        topBoundColliderComponent.size = new Vector3(100, 33, 1.8f);
        topBoundColliderComponent.material = physicsMaterial;

        GameObject bottomBoundCollider = new GameObject("bottomBoundCollider");

        bottomBoundCollider.transform.SetParent(boundContainer.transform);
        bottomBoundCollider.transform.position = new Vector3(0, 0, bottomBound - 1);
        BoxCollider bottomBoundColliderComponent = bottomBoundCollider.AddComponent<BoxCollider>();
        bottomBoundColliderComponent.size = new Vector3(100, 33, 1.8f);
        bottomBoundColliderComponent.material = physicsMaterial;

        GameObject rightBoundCollider = new GameObject("rightBoundCollider");

        rightBoundCollider.transform.SetParent(boundContainer.transform);
        rightBoundCollider.transform.position = new Vector3(rightBound + 1, 0, 0);
        BoxCollider rightBoundColliderComponent = rightBoundCollider.AddComponent<BoxCollider>();
        rightBoundColliderComponent.size = new Vector3(1.8f, 33, 100);
        rightBoundColliderComponent.material = physicsMaterial;

        GameObject leftBoundCollider = new GameObject("leftBoundCollider");

        leftBoundCollider.transform.SetParent(boundContainer.transform);
        leftBoundCollider.transform.position = new Vector3(leftBound - 1, 0, 0);
        BoxCollider leftBoundColliderComponent = leftBoundCollider.AddComponent<BoxCollider>();
        leftBoundColliderComponent.size = new Vector3(1.8f, 33, 100);
        leftBoundColliderComponent.material = physicsMaterial;

        topBoundCollider.layer = LayerMask.NameToLayer("Tile");
        bottomBoundCollider.layer = LayerMask.NameToLayer("Tile");
        rightBoundCollider.layer = LayerMask.NameToLayer("Tile");
        leftBoundCollider.layer = LayerMask.NameToLayer("Tile");

        if (level.GetComponent<ManualLevelCorner>() != null)
        {
            return;
        }

        // Generate Rounded corner
        Transform topLeftCorner = Instantiate(roundedCornerPrefab, boundContainer.transform).transform;

        topLeftCorner.position = new Vector3(leftBound - 1f, 1.4f, topBound + 1f);
        topLeftCorner.eulerAngles = new Vector3(0, 0, 0);

        Transform topRightCorner = Instantiate(roundedCornerPrefab, boundContainer.transform).transform;

        topRightCorner.position = new Vector3(rightBound + 1f, 1.4f, topBound + 1f);
        topRightCorner.eulerAngles = new Vector3(0, 90, 0);

        Transform bottomRightCorner = Instantiate(roundedCornerPrefab, boundContainer.transform).transform;

        bottomRightCorner.position = new Vector3(rightBound + 1f, 1.4f, bottomBound - 1f);
        bottomRightCorner.eulerAngles = new Vector3(0, 180, 0);

        Transform bottomLeftCorner = Instantiate(roundedCornerPrefab, boundContainer.transform).transform;

        bottomLeftCorner.position = new Vector3(leftBound - 1f, 1.4f, bottomBound - 1f);
        bottomLeftCorner.eulerAngles = new Vector3(0, -90, 0);
    }

    private void AutoFitLevelCamera()
    {
        Camera camera = Camera.main;

        float tileSize = GameConstants.tileSize;

        LevelBoard levelBoard = GamePersistentVariable.levelBoard;

        float sizeX = tileSize * levelBoard.numColumn;
        float sizeY = tileSize * levelBoard.numRow;

        float finalCameraFOV = 10 * sizeX;

        camera.fieldOfView = finalCameraFOV;
    }
}
