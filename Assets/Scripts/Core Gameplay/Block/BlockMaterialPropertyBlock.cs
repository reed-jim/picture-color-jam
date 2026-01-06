using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class BlockMaterialPropertyBlock : MonoBehaviour
{
    [SerializeField] private BlockServiceLocator blockServiceLocator;
    [SerializeField] private MeshFilter meshFilter;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private BlockMaterialsContainer blockMaterialsContainer;

    [Header("OUTLINE")]
    [SerializeField] private Outlinable outlinable;
    // [SerializeField] private SpriteRenderer outlineSprite;
    private MaterialPropertyBlock _outlinePropertyBlock;
    // private Renderer _outlineRenderer;
    [SerializeField] private float outlineAlpha;
    [SerializeField] private float outlineThickness;

    [Header("ICON")]
    [SerializeField] public SpriteRenderer icon;
    [SerializeField] private bool isUseAndOperatorForMasking;
    [SerializeField] private bool isEnableThresholdX;
    [SerializeField] private bool isEnableThresholdY;
    [SerializeField] private bool isLessThanThresholdX;
    [SerializeField] private bool isLessThanThresholdY;
    [SerializeField] private float thresholdX;
    [SerializeField] private float thresholdY;
    private MaterialPropertyBlock _iconPropertyBlock;
    private Renderer _iconRenderer;

    [Header("CUSTOMIZE")]
    [SerializeField] private string alphaValueReference;
    private const float DEFAULT_OUTLINE_WIDTH = 2f;
    private Color DEFAULT_OUTLINE_COLOR = ColorUtil.WithAlpha(0.03f * Color.white, 1);

    #region PRIVATE FIELD
    [SerializeField] private List<Tween> _tweens;
    [SerializeField] private List<Tween> _outlineTweens;
    private Tween _outlineTween;
    [SerializeField] private Renderer _renderer;
    private Renderer _outlineSpriteRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private GameFaction _cachedFaction;
    private bool _isInTransition;
    private Vector3 _initialOutlineSpritePosition;
    #endregion

    private void Awake()
    {
        LevelLoader.updateBoundEvent += SetMaskingBound;
        BoosterUI.enableBreakObjectModeEvent += OnHammerBoosterEnabled;
        BlockSelectionInput.breakObjectEvent += OnBoosterDisabled;
        BoosterUI.enableVacumnModeEvent += OnBoosterStateChanged;
        BlockSelectionInput.vacumnEvent += OnVacumnCompleted;
        BoosterItemUI.useBoosterEvent += OnTeleportBoosterEnabled;
        BaseBlock.useTeleportBoosterEvent += OnBoosterDisabled;
        BoosterUI.disableBreakObjectModeEvent += OnBoosterDisabled;

        _tweens = new List<Tween>();
        _outlineTweens = new List<Tween>();

        Init();

        InitOutline();

        SetTextureRotation();

        SetOutlineValue();
    }

    void OnDestroy()
    {
        LevelLoader.updateBoundEvent -= SetMaskingBound;
        BoosterUI.enableBreakObjectModeEvent -= OnHammerBoosterEnabled;
        BlockSelectionInput.breakObjectEvent -= OnBoosterDisabled;
        BoosterUI.enableVacumnModeEvent -= OnBoosterStateChanged;
        BlockSelectionInput.vacumnEvent -= OnVacumnCompleted;
        BoosterItemUI.useBoosterEvent -= OnTeleportBoosterEnabled;
        BaseBlock.useTeleportBoosterEvent -= OnBoosterDisabled;
        BoosterUI.disableBreakObjectModeEvent -= OnBoosterDisabled;

        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllTweens(_outlineTweens);
    }

    private void Init()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        // if (_outlineSpriteRenderer == null && outlineSprite != null)
        // {
        //     _outlineSpriteRenderer = outlineSprite.GetComponent<Renderer>();
        // }

        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        blockServiceLocator = GetComponent<BlockServiceLocator>();
    }

    #region Icon Masking
    private void SetIconMasking()
    {
        if (_iconRenderer == null)
        {
            _iconRenderer = icon.GetComponent<Renderer>();
        }

        if (_iconPropertyBlock == null)
        {
            _iconPropertyBlock = new MaterialPropertyBlock();
        }

        _iconPropertyBlock.SetInt("_IsUseAndOperatorForMasking", isUseAndOperatorForMasking ? 1 : 0);
        _iconPropertyBlock.SetInt("_IsEnableThresholdX", isEnableThresholdX ? 1 : 0);
        _iconPropertyBlock.SetInt("_IsEnableThresholdY", isEnableThresholdY ? 1 : 0);
        _iconPropertyBlock.SetInt("_IsLessThanThresholdX", isLessThanThresholdX ? 1 : 0);
        _iconPropertyBlock.SetInt("_IsLessThanThresholdY", isLessThanThresholdY ? 1 : 0);
        _iconPropertyBlock.SetFloat("_ThresholdX", thresholdX);
        _iconPropertyBlock.SetFloat("_ThresholdY", thresholdY);

        _iconRenderer.SetPropertyBlock(_iconPropertyBlock);
    }

    [ContextMenu("SetIconPosition")]
    public void SetIconPosition()
    {
        int minIndexX = blockServiceLocator.block.originalTileIndexes.Min(item => item.x);
        int maxIndexX = blockServiceLocator.block.originalTileIndexes.Max(item => item.x);
        int minIndexY = blockServiceLocator.block.originalTileIndexes.Min(item => item.y);
        int maxIndexY = blockServiceLocator.block.originalTileIndexes.Max(item => item.y);

        int numTileX = (maxIndexX - minIndexX) + 1;
        int numTileY = (maxIndexY - minIndexY) + 1;

        float tileSize = 1.4f;
        float offsetY = 0.1f;

        Vector2 finalPosition = new Vector2();

        if (numTileY == 3)
        {
            finalPosition.y -= tileSize;
        }

        finalPosition.y += offsetY;

        icon.transform.localPosition = finalPosition;
    }
    #endregion

    public Texture GetMainTexture()
    {
        return _renderer.material.GetTexture("_MainTexture");
    }

    public void SetFaction()
    {
        // if (_propertyBlock == null)
        // {
        //     Init();
        // }

        // _propertyBlock.SetColor("_Color", FactionUtility.GetColorForFaction(blockServiceLocator.block.BlockProperty.Faction));

        // _renderer.SetPropertyBlock(_propertyBlock);

        _renderer.sharedMaterial = blockMaterialsContainer.BlockMaterials[(int)blockServiceLocator.block.BlockProperty.Faction];
    }

    public void SetTextureRotation()
    {
        _propertyBlock.SetFloat("_Rotation", -transform.eulerAngles.y);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void SetMaskingBound(float rightBound, float leftBound, float topBound, float bottomBound)
    {
        _propertyBlock.SetFloat("_BoundRight", rightBound);
        _propertyBlock.SetFloat("_BoundLeft", leftBound);
        _propertyBlock.SetFloat("_BoundTop", topBound);
        _propertyBlock.SetFloat("_BoundBottom", bottomBound);
    }

    public void SetMaskingRight(float value)
    {
        _propertyBlock.SetFloat("_BoundRight", value);
    }

    public void SetMaskingLeft(float value)
    {
        _propertyBlock.SetFloat("_BoundLeft", value);
    }

    public void SetMaskingTop(float value)
    {
        _propertyBlock.SetFloat("_BoundTop", value);
    }

    public void SetMaskingBottom(float value)
    {
        _propertyBlock.SetFloat("_BoundBottom", value);
    }

    private void SetMaskingTopBound(float topBound)
    {
        _propertyBlock.SetFloat("_BoundTop", topBound);
        _propertyBlock.SetFloat("_BoundBottom", -topBound);
    }

    public void Disintegrate(Direction direction)
    {
        if (direction == Direction.Up)
        {
            _propertyBlock.SetFloat("_WeightClipZ", -1);
        }
        else if (direction == Direction.Down)
        {
            _propertyBlock.SetFloat("_WeightClipZ", 1);
        }
        else if (direction == Direction.Right)
        {
            _propertyBlock.SetFloat("_WeightClipX", -1);
        }
        else if (direction == Direction.Left)
        {
            _propertyBlock.SetFloat("_WeightClipX", 1);
        }

        Vector3 meshSizeWorld = meshFilter.mesh.bounds.size;

        if (direction == Direction.Up || direction == Direction.Down)
        {
            _propertyBlock.SetFloat("_MinPositionY", -0.5f * meshSizeWorld.y);
            _propertyBlock.SetFloat("_MaxPositionY", 0.5f * meshSizeWorld.y);
        }
        else
        {
            _propertyBlock.SetFloat("_MinPositionX", -0.5f * meshSizeWorld.x);
            _propertyBlock.SetFloat("_MaxPositionX", 0.5f * meshSizeWorld.x);
        }

        _tweens.Add(Tween.Custom(0, 1,
            startDelay: 0.1f * GameGeneralConfiguration.DISINTEGRATION_TIME, duration: 0.5f * GameGeneralConfiguration.DISINTEGRATION_TIME, onValueChange: newVal =>
            {
                _propertyBlock.SetFloat("_ClipValue", newVal);
                _renderer.SetPropertyBlock(_propertyBlock);
            })
            .OnComplete(() =>
            {
                gameObject.SetActive(false);

                blockServiceLocator.block.BlockProperty.IsDone = true;

                blockServiceLocator.block.InvokeBlockCompletedEvent();
            })
        );

        HideOutlineCompletely();
    }

    public void StopDisintegrating()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    #region Outline
    private void InitOutline()
    {
        _outlinePropertyBlock = new MaterialPropertyBlock();
        // _outlineRenderer = outlineSprite.GetComponent<SpriteRenderer>();

        _outlinePropertyBlock.SetFloat("_OutlineAlpha", 1);
        // _outlineRenderer.SetPropertyBlock(_outlinePropertyBlock);

        EnableOutline(false);
    }

    private void SetOutlineValue()
    {
        if (outlinable == null)
        {
            outlinable = GetComponent<Outlinable>();
        }

        if (outlinable != null)
        {
            // outlinable.FrontParameters.Color = ColorUtil.WhiteWithAlpha(0);
            // outlinable.BackParameters.Color = ColorUtil.WhiteWithAlpha(0);

            outlinable.enabled = false;
        }
    }

    private void SetDefaultOutline()
    {

    }

    public void ShowOutline(bool isShow, Color color)
    {
        if (!isShow && !outlinable.enabled)
        {
            return;
        }

        if (_outlineTween.isAlive)
        {
            _outlineTween.Stop();
        }

        float startValue;
        float endValue;

        if (isShow)
        {
            startValue = 0;
            endValue = 1;
        }
        else
        {
            startValue = 1;
            endValue = 0;
        }

        outlinable.enabled = true;

        _outlineTween = Tween.Custom(startValue, endValue, duration: 0.3f, onValueChange: newVal =>
        {
            outlinable.OutlineParameters.Color = ColorUtil.WithAlpha(color, newVal);
            // outlinable.FrontParameters.Color = ColorUtil.WhiteWithAlpha(newVal);
            // outlinable.BackParameters.Color = ColorUtil.WhiteWithAlpha(newVal);
        })
        .OnComplete(() =>
        {
            if (!isShow)
            {
                outlinable.enabled = false;

                // outlineSprite.gameObject.SetActive(false);
            }
        });
    }

    public void ShowOutline(bool isShow)
    {
        ShowOutline(isShow, Color.white);
    }

    public void EnableOutline(bool isEnable)
    {
        // outlineSprite.gameObject.SetActive(isEnable);
    }

    public void DisableOutline()
    {
        _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            _outlinePropertyBlock.SetFloat("_OutlineAlpha", newVal);
            // _outlineRenderer.SetPropertyBlock(_outlinePropertyBlock);
        })
            .OnComplete(() =>
            {
                // outlineSprite.gameObject.SetActive(false);
            })
        );
    }

    public void HideOutlineCompletely()
    {
        CommonUtil.StopAllTweens(_outlineTweens);
    }

    private void OnHammerBoosterEnabled()
    {
        if (blockServiceLocator.block.tileIndexes.Count > 1 && blockServiceLocator.block.IsValidToMerge())
        {
            ShowOutline(true, Color.green);
        }
    }

    private void OnTeleportBoosterEnabled(BoosterType boosterType)
    {
        if (boosterType == BoosterType.Teleport && blockServiceLocator.block.IsValidToMerge())
        {
            ShowOutline(true, Color.green);
        }
    }

    private void OnBoosterStateChanged(bool isEnable)
    {
        if (isEnable)
        {
            OnBoosterEnabled();
        }
        else
        {
            OnBoosterDisabled();
        }
    }

    private void OnBoosterEnabled()
    {
        if (blockServiceLocator.block.IsValidToMerge())
        {
            ShowOutline(true, Color.green);
        }
    }

    private void OnBoosterDisabled()
    {
        if (blockServiceLocator.block.IsValidToMerge())
        {
            ShowOutline(false, Color.green);
        }
    }

    private void OnVacumnCompleted(string spriteType)
    {
        OnBoosterDisabled();
    }
    #endregion
}
