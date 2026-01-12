using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameEnum;

public class BlockSelectionInput : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Header("CUSTOMIZE")]
    [SerializeField] private LayerMask layerMaskCheck;
    [SerializeField] private float maxSwipeDistance;
    [SerializeField] private float timeToConfirmSelectObjectPart;
    [SerializeField] private float thresholdMove;

    #region PRIVATE FIELD
    [SerializeField] private InputMode _inputMode;
    private List<Vector3> twoTouchPoints;
    private Vector3 _prevTouchPosition;
    private BaseBlock _selectedBlock;
    private bool _isSelectedLastFrame;
    #endregion

    #region EVENT
    public static event Action mouseUpEvent;
    public static event Action selectScrewEvent;
    public static event Action breakObjectEvent;
    public static event Action<string> vacumnEvent;
    public static event Action startVacumnEvent;
    public static event Action<string, string> showNotificationEvent;
    #endregion

    void Awake()
    {
        GameStateMachine.enableInputEvent += EnableInput;
        BaseBlock.enableInputEvent += EnableInput;
        BoosterUI.enableBreakObjectModeEvent += EnableBreakBlockMode;
        BoosterUI.disableBreakObjectModeEvent += DisableBreakBlockMode;
        BoosterUI.enableVacumnModeEvent += EnableVacumnMode;

        twoTouchPoints = new List<Vector3>();
    }

    void OnDestroy()
    {
        GameStateMachine.enableInputEvent -= EnableInput;
        BaseBlock.enableInputEvent -= EnableInput;
        BoosterUI.enableBreakObjectModeEvent -= EnableBreakBlockMode;
        BoosterUI.disableBreakObjectModeEvent -= DisableBreakBlockMode;
        BoosterUI.enableVacumnModeEvent -= EnableVacumnMode;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (_inputMode == InputMode.Select)
            {
                SelectBlock();

                MoveBlock();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (_inputMode == InputMode.BreakObject)
            {
                BreakBlock();
            }
            else if (_inputMode == InputMode.Vacumn)
            {
                VacumnBlock();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_selectedBlock != null)
            {
                _selectedBlock.Stop();

                _selectedBlock = null;

                _isSelectedLastFrame = false;
            }
        }
    }

    private async void SelectBlock()
    {
        if (_selectedBlock != null)
        {
            return;
        }

        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Default"));

        if (hit.collider != null)
        {
            BaseBlock block = hit.collider.GetComponent<BaseBlock>();

            if (block != null)
            {
                if (block.IsMovable())
                {
                    _selectedBlock = block;

                    _prevTouchPosition = Input.mousePosition;
                }
                else
                {
                    block.ShakeWhenUnableToMove();
                }
            }
        }
    }

    private BaseBlock GetBlock(bool isIncludeCurrentBlock = true)
    {
        if (isIncludeCurrentBlock && _selectedBlock != null)
        {
            return _selectedBlock;
        }

        if (IsClickedOnUI())
        {
            return null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Default"));

        if (hit.collider != null)
        {
            BaseBlock block = hit.collider.GetComponent<BaseBlock>();

            if (block != null)
            {
                return block;
            }
        }

        return null;
    }

    private void MoveBlock()
    {
        if (_selectedBlock != null)
        {
            if (!_isSelectedLastFrame)
            {
                _isSelectedLastFrame = true;

                return;
            }

            Vector2 direction = Input.mousePosition - _prevTouchPosition;

            _prevTouchPosition = Input.mousePosition;

            if (direction.magnitude < thresholdMove)
            {
                _selectedBlock.Move(Vector2.zero);

                return;
            }

            _selectedBlock.Move(direction);

            if (!GamePersistentVariable.isLevelDirty)
            {
                GamePersistentVariable.isLevelDirty = true;
            }
        }
    }

    private void BreakBlock()
    {
        BaseBlock block = GetBlock(isIncludeCurrentBlock: false);

        if (block == null)
        {
            return;
        }

        if (!block.IsValidToMerge())
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.cannotUseBooster);

            return;
        }

        _inputMode = InputMode.Disabled;

        if (block != null)
        {
            block.Break();

            breakObjectEvent?.Invoke();
        }
    }

    private void VacumnBlock()
    {
        BaseBlock block = GetBlock(isIncludeCurrentBlock: false);

        if (block == null)
        {
            return;
        }

        if (!block.IsValidToMerge())
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.cannotUseBooster);

            return;
        }

        BoosterVacumn vacumn = ObjectPoolingEverything.GetFromPool<BoosterVacumn>(GameConstants.VACUMN);

        startVacumnEvent?.Invoke();

        vacumn.Vacumn(onCompletedAction: (vacumnPosition) =>
        {
            Vector3 vacumnHead = vacumnPosition + new Vector3(0, 0, 8);

            block.Vacumn(isHideIconAfterDone: true);

            SoundManager.Instance.PlaySoundBreakObject();

            vacumnEvent?.Invoke(block.spriteType);
        });

        _inputMode = InputMode.Disabled;
    }

    private void EnableInput(bool isEnable)
    {
        if (isEnable)
        {
            _inputMode = InputMode.Select;
        }
        else
        {
            _inputMode = InputMode.Disabled;
        }
    }

    private void EnableBreakBlockMode()
    {
        _inputMode = InputMode.BreakObject;
    }

    private void DisableBreakBlockMode()
    {
        _inputMode = InputMode.Select;
    }

    private void EnableVacumnMode(bool isEnable)
    {
        _inputMode = isEnable ? InputMode.Vacumn : InputMode.Select;
    }

    private bool IsClickedOnUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
