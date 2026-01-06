using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class LevelIconUIManager : Singleton<LevelIconUIManager>
{
    [SerializeField] private LevelIconUIItem itemPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private LevelIconUIItem[] items;

    [SerializeField] private int maxItem;

    private List<Tween> _placeItemTweens;
    private BaseBlock[] _blocks;
    private string[] _levelIcons;

    protected override void MoreActionOnAwake()
    {
        _placeItemTweens = new List<Tween>();

        Init();
    }

    private void Init()
    {
        items = new LevelIconUIItem[maxItem];

        for (int i = 0; i < maxItem; i++)
        {
            items[i] = Instantiate(itemPrefab, container);
        }
    }

    public void SetupForLevel(List<BaseBlock> blocks)
    {
        SetupForLevel(blocks.ToArray());
    }

    public void SetupForLevel(BaseBlock[] blocks)
    {
        Reset();
        HideAllItems();

        string[] uniqueIcons = blocks
            .Where(item => item.gameObject.activeSelf && item.IsValidToMerge())
            .GroupBy(p => p.spriteType)
            .Select(g => g.First().spriteType).ToArray();

        for (int i = 0; i < uniqueIcons.Length; i++)
        {
            items[i].gameObject.SetActive(true);
            items[i].SetFaction(uniqueIcons[i]);
        }

        PlaceElements(uniqueIcons.Length, 0.3f / uniqueIcons.Length);

        _blocks = blocks;
    }

    private void Reset()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].Reset();
        }
    }

    public void HideIcon(Vector3 iconWorldPosition, string spriteType)
    {
        LevelIconUIItem item = items.First(item => item.Faction == spriteType);

        item.movingIcon.gameObject.SetActive(true);
        MatchMovingIconWithBlockIcon(item, iconWorldPosition);

        // Find new unique icons
        string[] uniqueIcons = _blocks
            .Where(item => item.gameObject.activeSelf && item.IsValidToMerge())
            .GroupBy(p => p.spriteType)
            .Select(g => g.First().spriteType).ToArray();

        GamePersistentVariable.isPlayingHideIconAnimation = true;

        if (GamePersistentVariable.hidingIconQueue == null)
        {
            GamePersistentVariable.hidingIconQueue = new Queue();
        }

        GamePersistentVariable.hidingIconQueue.Enqueue(spriteType);

        TweenUtil.PunchShow(item.movingIcon.rectTransform, 2f * item.icon.transform.localScale, onCompleted: () =>
        {
            Tween.Scale(item.movingIcon.rectTransform, item.icon.transform.localScale, duration: 0.5f);
            Tween.LocalPosition(item.movingIcon.rectTransform, item.icon.rectTransform.localPosition, duration: 0.5f, ease: Ease.InOutSine)
            .OnComplete(() =>
            {
                TweenUtil.ScaleUpDown(item.container, 1.3f, onCompleted: () =>
                {

                });

                item.tick.gameObject.SetActive(true);
                item.tick.color = ColorUtil.WithAlpha(Color.white, 0);
                item.tick.transform.localScale = 2f * Vector3.one;

                Tween.Alpha(item.tick, 1, duration: 0.3f, startDelay: 0.15f);
                Tween.Scale(item.tick.transform, 0.5f, duration: 0.3f, startDelay: 0.15f)
                .OnComplete(() =>
                {
                    Tween.EulerAngles(item.container, item.container.eulerAngles, new Vector3(0, 1 * 360, 0), duration: 0.3f)
                    .OnUpdate(target: item.tick.transform, (target, tween) =>
                    {
                        target.eulerAngles = Vector3.zero;
                    })
                    .OnComplete(() =>
                    {
                        TweenUtil.PunchHide(item.container, strength: 1.2f, duration: 0.4f, onCompleted: () =>
                        {
                            PlaceElements(uniqueIcons.Length, 0.3f / uniqueIcons.Length);

                            GamePersistentVariable.hidingIconQueue.Dequeue();

                            BlockManager.Instance.CheckWin();

                            GamePersistentVariable.isPlayingHideIconAnimation = false;
                        });
                    });
                });
            });
        });
    }

    private void MatchMovingIconWithBlockIcon(LevelIconUIItem levelIconUIItem, Vector3 iconWorldPosition)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(iconWorldPosition);

        // Screen → UI
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            levelIconUIItem.container,
            screenPos,
            null,
            out Vector2 uiPos
        );

        levelIconUIItem.movingIcon.rectTransform.anchoredPosition = uiPos;

        // // Size
        // Vector2 spriteSize = block.BlockServiceLocator.iconRenderer.bounds.size;

        // float pixelsPerUnit = block.BlockServiceLocator.iconRenderer.sprite.pixelsPerUnit;
        // float scaleFactor = canvas.scaleFactor;

        // Vector2 uiSize = spriteSize * pixelsPerUnit / scaleFactor;

        // movingIcon.sizeDelta = uiSize;
    }

    private void HideAllItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(false);
        }
    }

    #region Horizontal Placement
    private void PlaceElements(int numValidItem, float distanceRatio)
    {
        if (numValidItem <= 0)
        {
            return;
        }

        Vector2 itemSize = items[0].container.sizeDelta;

        CommonUtil.StopAllTweens(_placeItemTweens);

        int validItemIndex = 0;

        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].gameObject.activeSelf)
            {
                continue;
            }

            float x = (-(numValidItem - 1) / 2f + validItemIndex) * (1 + distanceRatio) * itemSize.x;

            _placeItemTweens.Add(Tween.LocalPosition(items[i].container, new Vector2(x, 0), duration: 0.3f, ease: Ease.InOutSine));

            validItemIndex++;
        }

        Tween.Scale(container, (float)(1 - 0.03f * Math.Pow(numValidItem - 1, 1.3f)), duration: 0.3f);
    }

    private float GetHorizontalPlacementPosition(int index, int numItem, float distanceRatio)
    {
        Vector2 itemSize = items[0].container.sizeDelta;

        return (-(numItem - 1) / 2 + (index + distanceRatio)) * itemSize.x;
    }
    #endregion
}
