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

    public List<TowerUIItem> towerPool;

    public Camera mainCamera;

    public static Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        placeTowerCanvasGroup.gameObject.SetActive(true);
        cancelCanvasGroup.gameObject.SetActive(false);
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

        BaseTower tower = ObjectPoolingEverything.GetFromPool<BaseTower>(GameConstants.tower);
        tower.isActive = false;

        GamePersistentVariable.screenSizeWorld = CommonUtil.GetScreenSizeWorld(mainCamera);

        bool isCancel = false;

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

            yield return waitForSeconds;
        }

        if (isCancel)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.tower, tower.gameObject);
        }
        else
        {
            tower.isActive = true;

            ObjectPoolingEverything.ReturnToPool(GameConstants.towerUIItem, towerUIItem.gameObject);
        }

        placeTowerCanvasGroup.gameObject.SetActive(true);
        cancelCanvasGroup.gameObject.SetActive(false);

        GamePersistentVariable.isPlacingTower = false;
    }

    public void AddTowerToPool(TowerType towerType)
    {
        TowerUIItem towerUIItem = ObjectPoolingEverything.GetFromPool<TowerUIItem>(GameConstants.towerUIItem);

        towerUIItem.transform.SetParent(placeTowerCanvasGroup.transform);
        towerUIItem.towerType = towerType;

        towerPool.Add(towerUIItem);
    }

    public void BuffTower(TowerType towerType, float buffRatio)
    {
        foreach (var item in towerPool)
        {
            if (item.towerType == towerType)
            {

            }
        }
    }

    public void ShowTowerSelectionPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.TowerSelection);
    }
}
