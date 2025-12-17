using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class IceMaterialPropertyBlock : MonoBehaviour
{
    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    #endregion

    private void Awake()
    {
        _tweens = new List<Tween>();

        Init();
    }

    void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private void Init()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }
    }

    public void Dissolve()
    {
        _tweens.Add(Tween.Custom(0, 1, duration: 1f, onValueChange: newVal =>
        {
            _propertyBlock.SetFloat("_DissolveStrength", newVal);

            _renderer.SetPropertyBlock(_propertyBlock);
        })
        .OnComplete(() => gameObject.SetActive(false))
        );
    }
}
