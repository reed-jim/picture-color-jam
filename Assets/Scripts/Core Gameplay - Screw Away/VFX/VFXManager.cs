using UnityEngine;

public class VFXManager : Singleton<VFXManager>
{
    public void PlayVFX(string key, Vector3 position)
    {
        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(key);

        fx.gameObject.SetActive(true);
        fx.transform.position = position;
        fx.Play();
    }

    public void PlayTouchFX(Vector3 position)
    {
        PlayVFX(GameConstants.touchFx, position);
    }
}
