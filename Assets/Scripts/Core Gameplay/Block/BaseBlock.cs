using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class BaseBlock : MonoBehaviour
{
    private BlockServiceLocator blockServiceLocator;
    [SerializeField] private BlockProperty blockProperty;

    protected List<Tween> _tweens;
    private Rigidbody2D _blockRigidBody;
    private MeshCollider _blockCollider;
    private Vector3 _targetPosition;
    private bool _isMovingLastFrame;
    private Vector3 _prevTargetPosition;
    private Vector3 _prevDirection;

    [Header("CUSTOMIZE")]
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float snappingLerpRatio = 0.2f;
    [SerializeField] private LayerMask layerMaskCheckTile;
    [SerializeField] private bool isAddMoreSortingLayer;
    [SerializeField] private bool isKeepBalancedSortingLayer;

    [Header("Scriptable Object")]
    [SerializeField] public GameFactionSpriteContainer gameFactionSpriteContainer;

    #region PRIVATE FIELD
    private Camera _mainCamera;
    private Vector3 _initialPosition;
    private Vector3 _snapPosition;
    private bool _isSnapping;
    private float _tileSize;
    private bool _isInTransition;
    #endregion

    #region Teleport
    private Vector2 _positionBeforeTeleport;
    #endregion

    #region Property
    [SerializeField] public List<Vector2Int> originalTileIndexes;
    [SerializeField] public List<Vector2Int> tileIndexes;
    [SerializeField] public string spriteType;

    public int NumTileX
    {
        get
        {
            return (tileIndexes.Max(item => item.x) - tileIndexes.Min(item => item.x)) + 1;
        }
    }

    public int NumTileY
    {
        get
        {
            return (tileIndexes.Max(item => item.y) - tileIndexes.Min(item => item.y)) + 1;
        }
    }

    public int NumOriginalTileX
    {
        get
        {
            return (originalTileIndexes.Max(item => item.x) - originalTileIndexes.Min(item => item.x)) + 1;
        }
    }

    public int NumOriginalTileY
    {
        get
        {
            return (originalTileIndexes.Max(item => item.y) - originalTileIndexes.Min(item => item.y)) + 1;
        }
    }

    public float CenterX
    {
        get
        {
            int minIndexX = tileIndexes.Min(item => item.x);
            int maxIndexX = tileIndexes.Max(item => item.x);

            return (minIndexX + maxIndexX) / 2f;
        }
    }

    public float CenterY
    {
        get
        {
            int minIndexY = tileIndexes.Min(item => item.y);
            int maxIndexY = tileIndexes.Max(item => item.y);

            return (minIndexY + maxIndexY) / 2f;
        }
    }

    public float OriginalCenterX
    {
        get
        {
            int minIndexX = originalTileIndexes.Min(item => item.x);
            int maxIndexX = originalTileIndexes.Max(item => item.x);

            return (minIndexX + maxIndexX) / 2f;
        }
    }

    public float OriginalCenterY
    {
        get
        {
            int minIndexY = originalTileIndexes.Min(item => item.y);
            int maxIndexY = originalTileIndexes.Max(item => item.y);

            return (minIndexY + maxIndexY) / 2f;
        }
    }

    public Vector2 OriginalCenter
    {
        get => new Vector2(OriginalCenterX, OriginalCenterY);
    }
    #endregion

    public static event Action<string, string> showNotificationEvent;
    public static event Action<bool> enableInputEvent;
    public static event Action compensateTeleportBoosterEvent;

    #region MOVEMENT
    private Vector3 _moveDirection;
    bool _isLockedHorizontal;
    bool _isLockedVertical;
    private float _lockHorizontalTime;
    private float _lockVericalTime;
    private int _horizontalColliderInstanceId;
    private int _verticalColliderInstanceId;
    private Vector3 _prevPosition;
    private Vector3 _lastEnqueuedDestination;
    private int _countdownPreviewSnapping;
    private List<BoardTileMaterialPropertyBlock> _prevPreviewSnappingTiles;
    private Vector3 _initialExpectedPosition;
    private Vector3 _blockOffset;
    #endregion

    public BlockServiceLocator BlockServiceLocator
    {
        get => blockServiceLocator;
    }

    public BlockProperty BlockProperty
    {
        get => blockProperty;
    }

    public Rigidbody2D BlockRigidbody
    {
        get => _blockRigidBody;
    }

    public GameFaction Faction
    {
        get => blockProperty.Faction;
        set => blockProperty.Faction = value;
    }

    public static event Action disintegrateBlockEvent;
    public static event Action blockCompletedEvent;
    public static event Action<int, bool> movePairedBlock;
    public static event Action disableHighlightTilesEvent;
    public static event Action useTeleportBoosterEvent;


    #region TEST MOVE
    private Vector3 _startMovingPosition;
    private Vector3 _startMovingMousePosition;
    private Direction _prevDirectional;
    private Vector3 _safePos;
    private bool _isMoveToSafePos;
    private bool _isInCountdownBounceBack;
    private Queue<Vector3> _destinations;
    private Vector3 _currentDestination;
    private Vector2 _inputDirection;
    private BoxCollider[] _boxColliders;
    List<MeshCollider> _meshColliders;
    List<Vector3> _initialMeshColliderScales;
    private Vector3[] _boxColliderSize;
    private Vector3[] _boundSize;
    #endregion

    #region SNAP PREVIEW
    private bool _isPreviewSnap;
    private Transform _snapPreviewSprite;
    private Vector3 _snapPreviewPosition;
    private BlockPreviewSpriteContainer _blockPreviewSpriteContainer;
    #endregion

    #region LIFE CYCLE
    private void Awake()
    {
        BlockSelectionInput.vacumnEvent += Vacumn;
        BoosterUI.enableTeleportModeEvent += UseTeleportBooster;
        useTeleportBoosterEvent += OnTeleportBoosterAlreadyUsed;
        BoosterUI.disableBreakObjectModeEvent += OnTeleportBoosterAlreadyUsed;

        _tweens = new List<Tween>();

        _mainCamera = Camera.main;

        blockServiceLocator = GetComponent<BlockServiceLocator>();
        _blockRigidBody = GetComponent<Rigidbody2D>();

        // _blockRigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
        // _blockRigidBody.isKinematic = true;

        speedMultiplier = 10f;
        snappingLerpRatio = 1f / 2;

        _destinations = new Queue<Vector3>();

        // if (transform.parent.GetComponent<BaseBlock>() == null)
        // {
        //     transform.position = transform.position.ChangeY(1.3f);
        //     transform.localScale = 1.99f * Vector3.one;
        // }

        _tileSize = GameConstants.tileSize;
        _initialPosition = transform.position;

        _blockRigidBody.bodyType = RigidbodyType2D.Static;

        SetupProperty();

        MoreLogicInAwake();

        return;

        _boxColliders = GetComponents<BoxCollider>();

        _boxColliderSize = new Vector3[_boxColliders.Length];
        _boundSize = new Vector3[_boxColliders.Length];

        // Find Mesh Colliders
        _meshColliders = new List<MeshCollider>();
        _initialMeshColliderScales = new List<Vector3>();

        for (int i = 0; i < transform.childCount; i++)
        {
            MeshCollider meshCollider = transform.GetChild(i).GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                _meshColliders.Add(meshCollider);
                _initialMeshColliderScales.Add(meshCollider.transform.localScale);

                meshCollider.transform.localScale *= 0.98f;
            }
        }

        // Physics Material
        PhysicsMaterial physicsMaterial = new PhysicsMaterial();

        physicsMaterial.staticFriction = 0;
        physicsMaterial.dynamicFriction = 0;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;

        for (int i = 0; i < _boxColliders.Length; i++)
        {
            _boxColliders[i].size = 0.98f * _boxColliders[i].size.ChangeZ(1.3f * _boxColliders[i].size.z);
            _boxColliderSize[i] = _boxColliders[i].size;
            _boundSize[i] = _boxColliders[i].bounds.size;

            _boxColliders[i].material = physicsMaterial;

            _boxColliders[i].enabled = false;
        }

        // Preview Snapping
        _countdownPreviewSnapping = 8;

        // Remove Old Outline
        Outline oldOutlineComponent = GetComponent<Outline>();

        if (oldOutlineComponent != null)
        {
            Destroy(oldOutlineComponent);
        }

        ScaleOnLevelStarted();

        MoreLogicInAwake();
    }

    void Start()
    {
        SnapImmediately();
        PlaceBlockIcon();
    }

    protected virtual void MoreLogicInAwake()
    {

    }

    private void OnValidate()
    {
        if (blockServiceLocator == null)
        {
            blockServiceLocator = GetComponent<BlockServiceLocator>();
        }

        blockServiceLocator.Init();

        // blockServiceLocator.blockMaterialPropertyBlock.SetFaction(blockProperty.Faction);
    }

    private void OnDestroy()
    {
        BlockSelectionInput.vacumnEvent -= Vacumn;
        BoosterUI.enableTeleportModeEvent -= UseTeleportBooster;
        useTeleportBoosterEvent -= OnTeleportBoosterAlreadyUsed;
        BoosterUI.disableBreakObjectModeEvent -= OnTeleportBoosterAlreadyUsed;

        CommonUtil.StopAllTweens(_tweens);

        MoreLogicOnDestroy();
    }

    protected virtual void MoreLogicOnDestroy()
    {

    }

    void Update()
    {
        if (blockProperty.IsMoving && !blockProperty.IsDisintegrating)
        {
            CheckDisintegration();
        }

        if (_isSnapping)
        {
            transform.position = Vector3.Lerp(transform.position, _snapPosition, snappingLerpRatio);

            if (Vector3.Distance(transform.position, _snapPosition) < GameConstants.TINY_FLOAT_VALUE)
            {
                Vector3 direction;
                float maxDistance;

                CheckDisintegration();
                BlockManager.Instance.CheckMergeBlocks();

                _isSnapping = false;
            }
            // else
            // {
            //     transform.position = _snapPosition;
            // }
        }

        // if (blockProperty.IsMoving)
        // {
        //     _snapPreviewSprite.position = Vector3.Lerp(_snapPreviewSprite.position, _snapPreviewPosition, 1 / 3f);
        // }
        // else
        // {
        //     if (_snapPreviewSprite != null)
        //     {
        //         _snapPreviewSprite.gameObject.SetActive(false);
        //     }
        // }
    }

    private void FixedUpdate()
    {
        if (blockProperty.IsDisintegrating)
        {
            return;
        }

        if (blockProperty.IsMoving && !_isMoveToSafePos)
        {
            Vector3 expectedDestination = Vector3.zero;

            if (Input.GetMouseButton(0))
            {
                Ray toGroundRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit2D groundHit = Physics2D.GetRayIntersection(toGroundRay, distance: Mathf.Infinity, layerMask: LayerMask.GetMask("Ground"));

                if (groundHit.collider != null)
                {
                    expectedDestination = groundHit.point;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (!_isMovingLastFrame)
            {
                _isMovingLastFrame = true;

                _blockOffset = expectedDestination - transform.position;
            }
            else
            {
                Vector2 intepolatedDestination = Vector2.Lerp(transform.position, expectedDestination - _blockOffset, 1f / 3);

                _blockRigidBody.MovePosition(intepolatedDestination);
            }

            PreviewSnapping();

            _prevTargetPosition = _targetPosition;
            _prevDirection = _moveDirection;
            _prevPosition = expectedDestination;
            // _prevPosition = transform.position;
        }
    }
    #endregion

    #region Property
    private void SetupProperty()
    {
        spriteType = blockServiceLocator.iconRenderer.sprite.name;
    }
    #endregion

    private void CheckDisintegration()
    {
        return;

        Vector3 direction;
        float maxDistance;

        if (transform.position.x > 0)
        {
            direction = Vector3.right;
        }
        else
        {
            direction = -Vector3.right;
        }

        maxDistance = 0.3f * blockServiceLocator.Size.x;

        bool IsDisintegrate = blockServiceLocator.blockCollider.CheckDisintegration(direction, maxDistance);

        if (blockProperty.isCheckDisintegrationBothRightLeft)
        {
            if (!IsDisintegrate)
            {
                IsDisintegrate = blockServiceLocator.blockCollider.CheckDisintegration(-direction, maxDistance);
            }
        }

        if (!IsDisintegrate)
        {
            if (transform.position.z > 0)
            {
                direction = Vector3.forward;
            }
            else
            {
                direction = -Vector3.forward;
            }

            maxDistance = 0.3f * blockServiceLocator.Size.z;

            IsDisintegrate = blockServiceLocator.blockCollider.CheckDisintegration(direction, maxDistance);
        }
    }

    #region FOR BETTER GAME FEEL
    private void ScaleOnLevelStarted()
    {
        Vector3 _prevScale = transform.localScale;

        transform.localScale = Vector3.zero;

        _tweens.Add(Tween.Scale(transform, _prevScale, duration: 0.5f));

        CreateSnapPreviewSpriteAsync();
    }
    #endregion 

    #region PREVIEW SNAPPING
    private async Task CreateSnapPreviewSpriteAsync()
    {
        AsyncOperationHandle<BlockPreviewSpriteContainer> handle = Addressables.LoadAssetAsync<BlockPreviewSpriteContainer>(GameConstants.SNAP_PREVIEW_SPRITE_CONTAINER);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _blockPreviewSpriteContainer = handle.Result;
        }
        else
        {
            return;
        }

        AsyncOperationHandle<GameObject> loadSnapPreviewPrefabHandle = Addressables.LoadAssetAsync<GameObject>(GameConstants.SNAP_PREVIEW_SPRITE);

        loadSnapPreviewPrefabHandle.Completed += async (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _snapPreviewSprite = Instantiate(op.Result, transform).transform;

                _snapPreviewSprite.localPosition = new Vector3(0, 0, 1);
                _snapPreviewSprite.localScale = Vector3.one;
                _snapPreviewSprite.localEulerAngles = Vector3.one;
                // _snapPreviewSprite.eulerAngles = new Vector3(90, 0, 0);

                PreviewSnapBlockMaterialPropertyBlock materialPropertyBlock = _snapPreviewSprite.GetComponent<PreviewSnapBlockMaterialPropertyBlock>();

                _snapPreviewSprite.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;

                // _snapPreviewSprite.GetComponent<SpriteRenderer>().sprite = _blockPreviewSpriteContainer.BlockPreviewSnapSprites[(int)blockProperty.Faction];

                materialPropertyBlock.SetTexture(GetComponent<BlockServiceLocator>().blockMaterialPropertyBlock.GetMainTexture());

                _snapPreviewSprite.gameObject.SetActive(false);




                // _snapPreviewSprite = Instantiate(op.Result, transform).transform;

                // _snapPreviewSprite.localScale = new Vector3(0.4f * blockProperty.NumTileX, 0.4f * blockProperty.NumTileZ, 1);
                // _snapPreviewSprite.eulerAngles = new Vector3(90, 0, 0);

                // PreviewSnapSpriteMaterialPropertyBlock materialPropertyBlock = _snapPreviewSprite.GetComponent<PreviewSnapSpriteMaterialPropertyBlock>();

                // _snapPreviewSprite.GetComponent<SpriteRenderer>().sprite = _blockPreviewSpriteContainer.BlockPreviewSnapSprites[(int)blockProperty.Faction];

                // materialPropertyBlock.SetTiling(new Vector2(2 * blockProperty.NumTileX, 2 * blockProperty.NumTileZ));

                // _snapPreviewSprite.gameObject.SetActive(false);
            }
        };
    }
    #endregion

    #region MOVEMENT
    public virtual bool IsMovable()
    {
        return true;
    }

    public virtual void Move(Vector2 direction)
    {
        if (blockProperty.IsDisintegrating)
        {
            return;
        }

        if (_isSnapping)
        {
            return;
        }

        if (!blockProperty.IsMoving)
        {
            blockServiceLocator.blockMaterialPropertyBlock.ShowOutline(true);

            // blockServiceLocator.spriteRenderer.sortingOrder = 997;
            // blockServiceLocator.iconMaskSortingGroup.sortingOrder = blockServiceLocator.spriteRenderer.sortingOrder + 1;

            _blockRigidBody.bodyType = RigidbodyType2D.Dynamic;

            movePairedBlock?.Invoke(gameObject.GetInstanceID(), true);

            _isSnapping = false;

            blockProperty.IsMoving = true;

            _startMovingPosition = transform.position;
            _startMovingMousePosition = Input.mousePosition;

            // teleport booster
            if (blockProperty.IsTeleportBoosterReady)
            {
                gameObject.layer = LayerMask.NameToLayer("Teleport");

                blockProperty.IsTeleportBoosterReady = false;
                blockProperty.IsUsingTeleportBooster = true;

                _positionBeforeTeleport = transform.position;

                blockServiceLocator.spriteRenderer.sortingOrder += 10;
                blockServiceLocator.iconMaskSortingGroup.sortingOrder += 10;

                useTeleportBoosterEvent?.Invoke();
            }
        }
        else
        {
            if (blockProperty.IsUsingTeleportBooster)
            {
                blockServiceLocator.spriteRenderer.sortingOrder = 998;

                float penetrationDepth = GetColliderPenetrationDepth();

                if (penetrationDepth < 0.2f)
                {
                    BlockManager.Instance.CheckMergeBlocks();
                }
            }
            else
            {
                SetSortingLayer();
            }

            blockServiceLocator.iconMaskSortingGroup.sortingOrder = blockServiceLocator.spriteRenderer.sortingOrder + 1;
        }

        _moveDirection = new Vector2(direction.x, direction.y);

        Vector3 expectedDestination;

        expectedDestination.x = _startMovingPosition.x + (Input.mousePosition.x - _startMovingMousePosition.x) * 0.03f;
        expectedDestination.y = _startMovingPosition.y + (Input.mousePosition.y - _startMovingMousePosition.y) * 0.03f;
        expectedDestination.z = transform.position.z;

        if (Vector3.Distance(expectedDestination, _lastEnqueuedDestination) > 1f)
        {
            _destinations.Enqueue(expectedDestination);

            _lastEnqueuedDestination = expectedDestination;
        }

        _inputDirection = direction;
    }

    public void ShakeWhenUnableToMove()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        blockServiceLocator.blockMaterialPropertyBlock.ShowInvalidOutline(onCompleteAction: () =>
        {
            _isInTransition = false;
        });

        // _tweens.Add(Tween.Scale(transform, 1.03f * transform.localScale, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.15f)
        // .OnComplete(() =>
        // {
        //     _isInTransition = false;
        // }));
    }

    public virtual void Stop()
    {
        // if (blockProperty.IsDisintegrating)
        // {
        //     return;
        // }

        _blockRigidBody.bodyType = RigidbodyType2D.Static;

        movePairedBlock?.Invoke(gameObject.GetInstanceID(), false);

        Snap();

        blockProperty.IsMoving = false;

        _isMovingLastFrame = false;

        _destinations.Clear();
    }

    private void PreviewSnapping()
    {
        if (!_isPreviewSnap)
        {
            return;
        }

        if (_countdownPreviewSnapping != 0)
        {
            _countdownPreviewSnapping--;

            return;
        }
        else
        {
            _countdownPreviewSnapping = 8;
        }


        float tileDistance = GamePersistentVariable.tileDistance;

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5, layerMaskCheckTile);

        if (hit.collider != null)
        {
            _snapPreviewPosition = hit.collider.transform.position;

            if (_snapPreviewPosition.x > transform.position.x)
            {
                _snapPreviewPosition.x -= (BlockProperty.NumTileX - 1) / 2f * tileDistance;
            }
            else
            {
                _snapPreviewPosition.x += (BlockProperty.NumTileX - 1) / 2f * tileDistance;
            }

            if (_snapPreviewPosition.z > transform.position.z)
            {
                _snapPreviewPosition.z -= (BlockProperty.NumTileZ - 1) / 2f * tileDistance;
            }
            else
            {
                _snapPreviewPosition.z += (BlockProperty.NumTileZ - 1) / 2f * tileDistance;
            }

            if (BlockProperty.NumTileX % 2 == 1)
            {
                _snapPreviewPosition.x = hit.collider.transform.position.x;
            }

            if (BlockProperty.NumTileZ % 2 == 1)
            {
                _snapPreviewPosition.z = hit.collider.transform.position.z;
            }

            _snapPreviewPosition.y = transform.position.y - 0.1f;

            if (!_snapPreviewSprite.gameObject.activeSelf)
            {
                _snapPreviewSprite.transform.position = transform.position.ChangeY(_snapPreviewPosition.y);
                _snapPreviewSprite.gameObject.SetActive(true);
            }
        }


        // if (blockProperty.NumTileZ > blockProperty.NumTileX)
        // {
        //     List<BoardTileMaterialPropertyBlock> orderedListHorizontal = someList
        //         .OrderBy(item => Mathf.Abs(item.Value.z))
        //         .OrderBy(item => Mathf.Abs(item.Value.x)).Select(item => item.Key).ToList();

        //     for (int i = 0; i < orderedListHorizontal.Count; i++)
        //     {
        //         if (i / blockProperty.NumTileZ <= blockProperty.NumTileX - 1)
        //         {
        //             finalList.Add(orderedListHorizontal[i]);
        //         }
        //         else
        //         {
        //             _prevPreviewSnappingTiles.Remove(orderedListHorizontal[i]);
        //         }
        //     }
        // }
        // else if (blockProperty.NumTileZ < blockProperty.NumTileX)
        // {
        //     List<BoardTileMaterialPropertyBlock> orderedListVertical = someList
        //         .OrderBy(item => Mathf.Abs(item.Value.x))
        //         .OrderBy(item => Mathf.Abs(item.Value.z)).Select(item => item.Key).ToList();

        //     for (int i = 0; i < orderedListVertical.Count; i++)
        //     {
        //         if (i / blockProperty.NumTileX <= blockProperty.NumTileZ - 1)
        //         {
        //             finalList.Add(orderedListVertical[i]);
        //         }
        //         else
        //         {
        //             _prevPreviewSnappingTiles.Remove(orderedListVertical[i]);
        //         }
        //     }
        // }
        // else
        // {

        // }

        // for (int i = 0; i < finalList.Count; i++)
        // {
        //     finalList[i].Highlight(true);

        //     if (disableHighlightTiles == null)
        //     {
        //         continue;
        //     }

        //     for (int k = 0; k < disableHighlightTiles.Count; k++)
        //     {
        //         if (disableHighlightTiles[k].GetInstanceID() == finalList[i].GetInstanceID())
        //         {
        //             duplicateHighlightTiles.Add(finalList[i]);
        //         }
        //     }
        // }

        // if (disableHighlightTiles == null)
        // {
        //     return;
        // }

        // for (int i = 0; i < duplicateHighlightTiles.Count; i++)
        // {
        //     disableHighlightTiles.Remove(duplicateHighlightTiles[i]);
        // }

        // for (int i = 0; i < disableHighlightTiles.Count; i++)
        // {
        //     disableHighlightTiles[i].Highlight(false);
        // }
    }

    private void DisablePreviewSnapping()
    {
        if (_prevPreviewSnappingTiles == null)
        {
            return;
        }

        for (int i = 0; i < _prevPreviewSnappingTiles.Count; i++)
        {
            _prevPreviewSnappingTiles[i].Highlight(false);
        }

        _countdownPreviewSnapping = 8;
    }

    public void Snap()
    {
        GetSnapPosition();

        SetSortingLayer();

        _blockRigidBody.bodyType = RigidbodyType2D.Static;

        _isSnapping = true;

        blockServiceLocator.blockMaterialPropertyBlock.ShowOutline(false);

        // BlockManager.Instance.CheckMergeBlocks();

        if (blockProperty.IsUsingTeleportBooster)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");

            blockProperty.IsUsingTeleportBooster = false;

            ResolveBlockOverlapping(transform);
        }

        SoundManager.Instance.PlaySoundPlaceBlock();

        return;

        float tileDistance = GamePersistentVariable.tileDistance;

        DisablePreviewSnapping();

        blockServiceLocator.blockMaterialPropertyBlock.ShowOutline(false);

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5, layerMaskCheckTile);

        if (hit.collider != null)
        {
            _snapPosition = hit.collider.transform.position;

            if (_snapPosition.x > transform.position.x)
            {
                _snapPosition.x -= (BlockProperty.NumTileX - 1) / 2f * tileDistance;
            }
            else
            {
                _snapPosition.x += (BlockProperty.NumTileX - 1) / 2f * tileDistance;
            }

            if (_snapPosition.z > transform.position.z)
            {
                _snapPosition.z -= (BlockProperty.NumTileZ - 1) / 2f * tileDistance;
            }
            else
            {
                _snapPosition.z += (BlockProperty.NumTileZ - 1) / 2f * tileDistance;
            }

            if (BlockProperty.NumTileX % 2 == 1)
            {
                _snapPosition.x = hit.collider.transform.position.x;
            }

            if (BlockProperty.NumTileZ % 2 == 1)
            {
                _snapPosition.z = hit.collider.transform.position.z;
            }

            // _snapPosition.x -= (BlockProperty.NumTileX - 1) / 2f * tileDistance;
            // _snapPosition.z += (BlockProperty.NumTileZ - 1) / 2f * tileDistance;

            _snapPosition.y = _initialPosition.y;

            _blockRigidBody.isKinematic = true;

            _isSnapping = true;
        }
    }

    public virtual void SnapImmediately()
    {
        GetSnapPosition();

        transform.position = _snapPosition;

        // x2 help tell layers apart better
        SetSortingLayer();
    }

    private void GetSnapPosition()
    {
        int numColumn = GamePersistentVariable.levelBoard.numColumn;
        int numRow = GamePersistentVariable.levelBoard.numRow;
        float tileSize = GameConstants.tileSize;

        float offsetX = (numColumn % 2 == 0) ? 0.5f * tileSize : 0f;
        float offsetY = (numRow % 2 == 0) ? 0.5f * tileSize : 0f;

        _snapPosition = new Vector2
        (
            Mathf.Round(transform.position.x / tileSize) * tileSize,
            Mathf.Round(transform.position.y / tileSize) * tileSize
        );

        if (Mathf.Abs((_snapPosition.x + offsetX) - transform.position.x) < Mathf.Abs((_snapPosition.x - offsetX) - transform.position.x))
        {
            _snapPosition.x += offsetX;
        }
        else
        {
            _snapPosition.x -= offsetX;
        }

        if (Mathf.Abs((_snapPosition.y + offsetY) - transform.position.y) < Mathf.Abs((_snapPosition.y - offsetY) - transform.position.y))
        {
            _snapPosition.y += offsetY;
        }
        else
        {
            _snapPosition.y -= offsetY;
        }

        if (NumTileX % 2 == 0)
        {
            if (Mathf.Abs((_snapPosition.x + 0.5f * tileSize) - transform.position.x) < Mathf.Abs((_snapPosition.x - 0.5f * tileSize) - transform.position.x))
            {
                _snapPosition.x += 0.5f * tileSize;
            }
            else
            {
                _snapPosition.x -= 0.5f * tileSize;
            }
        }

        if (NumTileY % 2 == 0)
        {
            if (Mathf.Abs((_snapPosition.y + 0.5f * tileSize) - transform.position.y) < Mathf.Abs((_snapPosition.y - 0.5f * tileSize) - transform.position.y))
            {
                _snapPosition.y += 0.5f * tileSize;
            }
            else
            {
                _snapPosition.y -= 0.5f * tileSize;
            }
        }

        _snapPosition.y -= 0.1f;
    }

    public void SetSortingLayer()
    {
        float positionY = transform.position.y + (NumTileY - 1) / 2f * GameConstants.tileSize;

        if (isAddMoreSortingLayer)
        {
            positionY = transform.position.y - (NumTileY - 1) / 2f * GameConstants.tileSize;
        }

        if (isKeepBalancedSortingLayer)
        {
            positionY = transform.position.y;
        }

        blockServiceLocator.spriteRenderer.sortingOrder = (int)(2 * (15 - positionY));
        blockServiceLocator.iconMaskSortingGroup.sortingOrder = blockServiceLocator.spriteRenderer.sortingOrder + 1;
    }

    public void SetSortingLayer(Vector3 position)
    {
        if (!isKeepBalancedSortingLayer)
        {
            if (isAddMoreSortingLayer)
            {
                position.y -= (NumTileY - 1) / 2f * GameConstants.tileSize;
            }
            else
            {
                position.y += (NumTileY - 1) / 2f * GameConstants.tileSize;
            }
        }

        blockServiceLocator.spriteRenderer.sortingOrder = (int)(2 * (15 - position.y));
        blockServiceLocator.iconMaskSortingGroup.sortingOrder = blockServiceLocator.spriteRenderer.sortingOrder + 1;
    }
    #endregion

    #region Icon
    private void PlaceBlockIcon()
    {
        float tileSize = GameConstants.tileSize;

        float sizeX = NumTileX * tileSize;
        float sizeY = NumTileY * tileSize;

        Vector2 iconCenterIndex;

        iconCenterIndex.x = (originalTileIndexes.Max(item => item.x) + originalTileIndexes.Min(item => item.x)) / 2f;
        iconCenterIndex.y = (originalTileIndexes.Max(item => item.y) + originalTileIndexes.Min(item => item.y)) / 2f;

        Vector2 blockCenterIndex;

        if (tileIndexes.Count > 1)
        {
            blockCenterIndex.x = (tileIndexes.Max(item => item.x) + tileIndexes.Min(item => item.x)) / 2f;
            blockCenterIndex.y = (tileIndexes.Max(item => item.y) + tileIndexes.Min(item => item.y)) / 2f;
        }
        else
        {
            blockCenterIndex = tileIndexes[0];
        }

        Vector2 indexDistanceToCenter = new Vector2(iconCenterIndex.x - blockCenterIndex.x, blockCenterIndex.y - iconCenterIndex.y);

        Vector2 iconLocalPosition = indexDistanceToCenter * tileSize;

        BlockServiceLocator.iconRenderer.transform.localPosition = iconLocalPosition;
    }
    #endregion

    #region Merge
    public virtual bool IsValidToMerge()
    {
        return true;
    }

    public void HandleOnMerge()
    {
        gameObject.SetActive(false);

        InvokeBlockCompletedEvent();
    }
    #endregion

    #region DISINTEGRATE
    public async Task Disintegrate(Direction direction)
    {
        if (blockProperty.IsDisintegrating)
        {
            return;
        }

        // blockServiceLocator.blockMaterialPropertyBlock.ShowOutline(false);
        blockServiceLocator.blockMaterialPropertyBlock.HideOutlineCompletely();

        Snap();

        blockServiceLocator.blockMaterialPropertyBlock.DisableOutline();

        blockProperty.IsMoving = false;
        blockProperty.IsDisintegrating = true;

        blockProperty.IsReadyTriggerDisintegrateFx = true;

        while (_isSnapping)
        {
            await Task.Delay(2);
        }

        // if (blockProperty.IsPreventDisintegrating)
        // {
        //     blockProperty.IsDisintegrating = false;

        //     return;
        // }

        // blockProperty.IsReadyTriggerDisintegrateFx = true;

        disintegrateBlockEvent?.Invoke();

        // _blockCollider.enabled = false;

        if (GamePersistentVariable.IsVibrate)
        {
            CandyCoded.HapticFeedback.HapticFeedback.HeavyFeedback();
        }

        SoundManager.Instance.PlaySoundBreakObject();

        // Disable Collider
        for (int i = 0; i < _meshColliders.Count; i++)
        {
            _meshColliders[i].isTrigger = true;
        }

        if (direction == Direction.Right)
        {
            blockServiceLocator.blockMaterialPropertyBlock.SetMaskingRight(transform.position.x + 0.5f * (blockProperty.NumTileX + 1) * _tileSize);

            _tweens.Add(Tween.PositionX(transform, transform.position.x + (blockProperty.NumTileX + 1) * _tileSize, duration: GameGeneralConfiguration.DISINTEGRATION_TIME));
        }
        if (direction == Direction.Left)
        {
            blockServiceLocator.blockMaterialPropertyBlock.SetMaskingLeft(transform.position.x - 0.5f * (blockProperty.NumTileX + 1) * _tileSize);

            _tweens.Add(Tween.PositionX(transform, transform.position.x - (blockProperty.NumTileX + 1) * _tileSize, duration: GameGeneralConfiguration.DISINTEGRATION_TIME));
        }
        if (direction == Direction.Up)
        {
            blockServiceLocator.blockMaterialPropertyBlock.SetMaskingTop(transform.position.z + 0.5f * (blockProperty.NumTileZ + 1) * _tileSize);

            _tweens.Add(Tween.PositionZ(transform, transform.position.z + (blockProperty.NumTileZ + 1) * _tileSize, duration: GameGeneralConfiguration.DISINTEGRATION_TIME));
        }
        else if (direction == Direction.Down)
        {
            blockServiceLocator.blockMaterialPropertyBlock.SetMaskingBottom(transform.position.z - 0.5f * (blockProperty.NumTileZ + 1) * _tileSize);

            _tweens.Add(Tween.PositionZ(transform, transform.position.z - (blockProperty.NumTileZ + 1) * _tileSize, duration: GameGeneralConfiguration.DISINTEGRATION_TIME));
        }

        blockServiceLocator.blockMaterialPropertyBlock.Disintegrate(direction);

        // DOUBLE BLOCK
        EnableInsideBlock();

        // KEY
        UnlockKey();

        // ELEMENT
        UseElement();
    }

    public void StopDisintegrating()
    {
        CommonUtil.StopAllTweens(_tweens);

        blockServiceLocator.blockMaterialPropertyBlock.StopDisintegrating();

        blockProperty.IsDisintegrating = false;
    }
    #endregion

    #region BOOSTER
    public void Break()
    {
        if (NumTileX == 1 && NumTileY == 1)
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.already1x1Tile);

            GamePersistentVariable.isUseHammerToBreak1x1Tile = true;

            enableInputEvent?.Invoke(true);

            return;
        }

        BoosterHammer hammer = ObjectPoolingEverything.GetFromPool<BoosterHammer>(GameConstants.HAMMER);

        enableInputEvent?.Invoke(false);

        hammer.HitTarget(transform, onCompletedAction: () =>
        {
            Tween.ShakeScale(transform, 1.2f * Vector3.one, duration: 0.3f).OnComplete(() =>
            {
                gameObject.SetActive(false);

                SpawnBlocksAfterBreak();

                blockProperty.IsDone = true;

                blockCompletedEvent?.Invoke();
                enableInputEvent?.Invoke(true);
            });
        });

        // KEY
        UnlockKey();
    }

    private void SpawnBlocksAfterBreak()
    {
        Dictionary<BaseBlock, List<Vector2Int>> worldTileIndexesOfBlock = new Dictionary<BaseBlock, List<Vector2Int>>();

        BaseBlock[] smallBlocks = TileMapper.Instance.SplitBlock(this);

        BlockManager.Instance.Blocks.AddRange(smallBlocks);
        BlockManager.Instance.Blocks = BlockManager.Instance.Blocks.Distinct().ToList();

        Vector2 smallBlockIconPosition;

        for (int i = 0; i < smallBlocks.Length; i++)
        {
            BaseBlock smallBlock = smallBlocks[i];

            smallBlock.originalTileIndexes = this.originalTileIndexes;
            smallBlock.tileIndexes = new List<Vector2Int>() { this.tileIndexes[i] };
            smallBlock.spriteType = this.spriteType;
            smallBlock.Faction = this.Faction;
            smallBlock.blockServiceLocator.blockFaction.Faction = blockServiceLocator.blockFaction.Faction;
            smallBlock.blockServiceLocator.blockFaction.UpdateFaction();

            smallBlock.blockServiceLocator.iconRenderer.sprite = gameFactionSpriteContainer.factionSprites[int.Parse(smallBlock.spriteType) - 1];

            smallBlockIconPosition.x = ((NumOriginalTileX - 1) / 2f - this.tileIndexes[i].x) * _tileSize;
            smallBlockIconPosition.y = (-(NumOriginalTileY - 1) / 2f + this.tileIndexes[i].y) * _tileSize;

            smallBlock.blockServiceLocator.iconRenderer.transform.localPosition = smallBlockIconPosition;
        }
    }

    public void Vacumn(bool isHideIconAfterDone = false)
    {
        if (!IsValidToMerge())
        {
            return;
        }

        Vector2 endPosition = TileMapper.Instance.GetBlockVacumnPosition(this);

        _blockRigidBody.simulated = false;

        blockProperty.IsMagneting = true;

        blockServiceLocator.spriteRenderer.sortingOrder = GameConstants.maxSortingLayer - (int)endPosition.y;
        blockServiceLocator.iconMaskSortingGroup.sortingOrder = blockServiceLocator.spriteRenderer.sortingOrder + 1;

        ParticleSystem magnetFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.magnetFx);

        magnetFx.transform.SetParent(transform);
        magnetFx.transform.localPosition = Vector3.zero;
        magnetFx.Play();

        Tween.Position(transform, endPosition, duration: 0.6f)
        .OnComplete(() =>
        {
            blockProperty.IsDone = true;

            blockCompletedEvent?.Invoke();

            magnetFx.Stop();

            Tween.Delay(0.1f, onComplete: () =>
            {
                gameObject.SetActive(false);
            });

            if (isHideIconAfterDone)
            {
                // wait for other blocks to be magneted
                Tween.Delay(1.1f * 0.1f, onComplete: () =>
                {
                    Vector2 originalCenter = OriginalCenter;

                    for (int i = 0; i < originalTileIndexes.Count; i++)
                    {
                        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockMergeFx);

                        fx.gameObject.SetActive(true);
                        fx.transform.position = TileMapper.TileToWorld(originalTileIndexes[i] - originalCenter);
                        fx.Play();
                    }

                    LevelIconUIManager.Instance.HideIcon(Vector3.zero, spriteType);

                    enableInputEvent?.Invoke(true);
                });
            }
        });

        // KEY
        UnlockKey();
    }

    public void Vacumn(string spriteType)
    {
        if (blockProperty.IsMagneting)
        {
            return;
        }

        if (spriteType == this.spriteType)
        {
            Vacumn();
        }
    }

    private void UseTeleportBooster()
    {
        blockProperty.IsTeleportBoosterReady = true;
    }

    private void OnTeleportBoosterAlreadyUsed()
    {
        if (blockProperty.IsTeleportBoosterReady)
        {
            blockProperty.IsTeleportBoosterReady = false;
        }
    }

    private void ResolveBlockOverlapping(Transform block)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);
        float step = 0.2f;
        float max = 100f;
        float moved = 0f;

        blockProperty.isResolveBlockOverlapping = true;
        _isSnapping = false;

        int maxCount = 32;
        int count = 0;

        // Auto move out of other blocks
        // while (count < maxCount)
        // {
        //     Collider2D col = blockServiceLocator.blockCollider2D;

        //     Collider2D[] hits = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0, LayerMask.GetMask("Default"));

        //     if (hits.Length == 0)
        //     {
        //         yield break;
        //     }
        //     else
        //     {

        //     }

        //     foreach (var h in hits)
        //     {
        //         if (h == col) continue;

        //         var dist = col.Distance(h);     // gives direction + distance out of overlap
        //         col.transform.position += 0.1f * (Vector3)dist.normal * dist.distance;
        //     }

        //     count++;

        //     yield return waitForSeconds;
        // }

        float penetrationDepth = GetColliderPenetrationDepth();

        if (penetrationDepth > 0.2f)
        {
            enableInputEvent?.Invoke(false);

            blockServiceLocator.spriteRenderer.sortingOrder = 998;
            blockServiceLocator.iconMaskSortingGroup.sortingOrder = blockServiceLocator.spriteRenderer.sortingOrder + 1;

            Tween.Position(transform, _positionBeforeTeleport, duration: 0.3f)
            .OnComplete(() =>
            {
                Snap();

                enableInputEvent?.Invoke(true);
            });

            compensateTeleportBoosterEvent?.Invoke();
        }
        else
        {
            Snap();
        }

        blockProperty.isResolveBlockOverlapping = false;
    }

    private float GetColliderPenetrationDepth()
    {
        Collider2D col = blockServiceLocator.blockCollider2D;

        Collider2D[] hits = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0, LayerMask.GetMask("Default"));

        if (hits.Length == 0)
        {
            return 0;
        }
        else
        {
            float penetrationDepth = 0;

            foreach (var h in hits)
            {
                if (h != col)
                {
                    ColliderDistance2D dist = h.Distance(col);

                    if (-dist.distance > penetrationDepth)
                    {
                        penetrationDepth = -dist.distance;
                    }
                }
            }

            return penetrationDepth;
        }
    }
    #endregion

    #region UTIL
    public void InvokeBlockCompletedEvent()
    {
        blockCompletedEvent?.Invoke();
    }
    #endregion

    #region DOUBLE BLOCK
    private void InitInsideBlock()
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseBlock insideBlock = transform.GetChild(i).GetComponent<BaseBlock>();

                if (insideBlock == null)
                {
                    continue;
                }

                insideBlock.BlockServiceLocator.blockMaterialPropertyBlock.EnableOutline(false);

                Collider[] colliders = TransformUtil.GetComponentsFromAllChildren<Collider>(insideBlock.transform).ToArray();

                foreach (var collider in colliders)
                {
                    collider.enabled = false;
                }
            }
        }
    }

    private void EnableInsideBlock()
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseBlock insideBlock = transform.GetChild(i).GetComponent<BaseBlock>();

                if (insideBlock == null)
                {
                    continue;
                }

                insideBlock.transform.SetParent(transform.parent);

                insideBlock.transform.position = insideBlock.transform.position.ChangeY(1.3f);
                insideBlock.transform.localScale = 1.99f * Vector3.one;

                // insideBlock.GetComponent<MeshFilter>().mesh = blockServiceLocator.meshFilter.mesh;

                insideBlock.BlockServiceLocator.blockMaterialPropertyBlock.EnableOutline(true);

                Tween.Scale(insideBlock.transform, transform.localScale, duration: 0.3f)
                .OnComplete(() =>
                {
                    MeshCollider[] colliders = TransformUtil.GetComponentsFromAllChildren<MeshCollider>(insideBlock.transform).ToArray();

                    foreach (var collider in colliders)
                    {
                        collider.enabled = true;
                    }
                });

                for (int j = 0; j < insideBlock.transform.childCount; j++)
                {
                    if (insideBlock.transform.GetChild(j).name == "Outline Sprite (1)")
                    {
                        insideBlock.transform.GetChild(j).gameObject.SetActive(false);

                        break;
                    }
                }
            }
        }
    }
    #endregion

    #region KEY
    private void UnlockKey()
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Key key = transform.GetChild(i).GetComponent<Key>();

                if (key != null)
                {
                    key.transform.SetParent(transform.parent);

                    key.Unlock();
                }
            }
        }
    }
    #endregion

    #region ELEMENTS
    private void UseElement()
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseElement element = transform.GetChild(i).GetComponent<BaseElement>();

                if (element != null)
                {
                    element.transform.SetParent(transform.parent);

                    element.Use();
                }
            }
        }
    }
    #endregion
}
