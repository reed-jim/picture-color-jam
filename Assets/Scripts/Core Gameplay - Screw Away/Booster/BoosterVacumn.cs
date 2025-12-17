using System;
using PrimeTween;
using UnityEngine;

public class BoosterVacumn : MonoBehaviour
{
    [SerializeField] private BoosterHammerMaterialPropertyBlock boosterVacumnMaterialPropertyBlock;

    [SerializeField] private Transform magnet;
    [SerializeField] private ParticleSystem tornadoFx;

    [SerializeField] private AudioSource magnetSound;
    [SerializeField] private AudioSource tornadoSound;

    public void Vacumn(Action<Vector3> onCompletedAction)
    {
        gameObject.SetActive(true);

        transform.position = new Vector3(0, -24, 0);
        magnet.localPosition = new Vector3(0, 0, 0);

        Tween.PositionY(transform, -6, duration: 0.5f).OnComplete(() =>
        {
            tornadoFx.Play();

            if (GamePersistentVariable.isEnableSound)
            {
                magnetSound.volume = 0.5f;
                tornadoSound.volume = 1;

                magnetSound.Play();
                tornadoSound.Play();
            }

            Tween.ShakeLocalRotation(magnet, new Vector3(0, 16, 0), duration: 1.2f);
            Tween.Delay(1.2f).OnComplete(() =>
            {
                tornadoFx.Stop();

                // Dissolve();

                Tween.AudioVolume(magnetSound, 0, duration: 0.5f);
                Tween.AudioVolume(tornadoSound, 0, duration: 0.5f);

                Tween.PositionY(magnet, -12, duration: 0.5f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });

            onCompletedAction?.Invoke(transform.position);
        });
    }

    public void Dissolve()
    {
        boosterVacumnMaterialPropertyBlock.Dissolve();
    }
}
