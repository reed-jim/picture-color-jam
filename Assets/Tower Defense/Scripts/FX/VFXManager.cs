using UnityEngine;

public class VFXManager : Singleton<VFXManager>
{
    public void PlayBuffFX(Vector3 position)
    {
        PlayFX(GameConstants.buffFx, position);
    }

    public void PlayPlaceTowerFX(Vector3 position)
    {
        PlayFX(GameConstants.placeTowerFx, position);
    }

    private void PlayFX(string key, Vector3 position)
    {
        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(key);

        fx.transform.position = position;

        fx.Play();
    }
}
