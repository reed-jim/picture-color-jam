using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class TowerManager : Singleton<TowerManager>
{
    [SerializeField] private CanvasGroup placeTowerCanvasGroup;
    [SerializeField] private CanvasGroup cancelCanvasGroup;
    [SerializeField] private Image cancelArea;

    public SpriteRenderer placeTowerPreview;

    public List<TowerUIItem> towerPool;
    public List<BaseTower> towers;

    public Camera mainCamera;

    public static Action<ScreenRoute> switchRouteEvent;
    public static Action<TowerProperty> showTowerDetailPopup;
    public static Action<bool> enableJoystickEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        placeTowerCanvasGroup.gameObject.SetActive(true);
        cancelCanvasGroup.gameObject.SetActive(false);
    }

    void Start()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Tutorial);

        placeTowerPreview.gameObject.SetActive(false);
    }

    void Update()
    {
        SelectTower();
    }

    public void PlaceTower(TowerUIItem towerUIItem)
    {
        StartCoroutine(PlacingTower(towerUIItem));

        GamePersistentVariable.isPlacingTower = true;
    }

    public IEnumerator PlacingTower(TowerUIItem towerUIItem)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        placeTowerCanvasGroup.gameObject.SetActive(false);
        cancelCanvasGroup.gameObject.SetActive(true);
        placeTowerPreview.gameObject.SetActive(true);

        BaseTower tower = ObjectPoolingEverything.GetFromPool<BaseTower>(towerUIItem.towerType.ToString());
        tower.isActive = false;

        GamePersistentVariable.screenSizeWorld = CommonUtil.GetScreenSizeWorld(mainCamera);

        bool isCancel = false;

        enableJoystickEvent?.Invoke(false);

        while (Input.GetMouseButton(0))
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

            isCancel = mouseWorldPos.y < -0.35f * GamePersistentVariable.screenSizeWorld.y;

            mouseWorldPos.y += 5;
            mouseWorldPos.z = -2;

            tower.transform.position = mouseWorldPos;

            if (isCancel)
            {
                cancelArea.color = Color.red;
            }
            else
            {
                cancelArea.color = Color.white;
            }

            float penetrationDepth = GetColliderPenetrationDepth(tower.towerCollider);

            placeTowerPreview.color = Color.Lerp(Color.green, Color.red, penetrationDepth);

            if (penetrationDepth > 0.2f)
            {
                isCancel = true;
            }

            yield return waitForSeconds;
        }

        if (isCancel)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.tower, tower.gameObject);
        }
        else
        {
            tower.isActive = true;

            towers.Add(tower);

            ObjectPoolingEverything.ReturnToPool(GameConstants.towerUIItem, towerUIItem.gameObject);
        }

        placeTowerCanvasGroup.gameObject.SetActive(true);
        cancelCanvasGroup.gameObject.SetActive(false);
        placeTowerPreview.gameObject.SetActive(false);

        GamePersistentVariable.isPlacingTower = false;

        enableJoystickEvent?.Invoke(true);
    }

    public void AddTowerToPool(TowerType towerType)
    {
        TowerUIItem towerUIItem = ObjectPoolingEverything.GetFromPool<TowerUIItem>(GameConstants.towerUIItem);

        towerUIItem.transform.SetParent(placeTowerCanvasGroup.transform);
        towerUIItem.towerType = towerType;
        towerUIItem.UpdateItem();

        towerPool.Add(towerUIItem);
    }

    public void BuffTower(TowerType towerType, float buffRatio)
    {
        foreach (var item in towers)
        {
            item.TryApplyUpgrade(towerType, buffRatio);
        }
    }

    public void ShowTowerSelectionPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.TowerSelection);
    }

    public static void SelectTower()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                BaseTower tower = hit.collider.GetComponent<BaseTower>();

                if (tower != null)
                {
                    showTowerDetailPopup?.Invoke(tower.towerProperty);
                }
            }
        }
    }

    #region Util
    private float GetColliderPenetrationDepth(Collider2D collider2D)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(collider2D.bounds.center, collider2D.bounds.size, 0, LayerMask.GetMask("Default"));

        if (hits.Length == 0)
        {
            return 0;
        }
        else
        {
            float penetrationDepth = 0;

            foreach (var h in hits)
            {
                if (h != collider2D)
                {
                    ColliderDistance2D dist = h.Distance(collider2D);

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
}
