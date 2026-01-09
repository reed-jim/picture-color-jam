using System;
using PrimeTween;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class BoosterVacumn : MonoBehaviour
{
    [SerializeField] private BoosterHammerMaterialPropertyBlock boosterVacumnMaterialPropertyBlock;

    [SerializeField] private Transform magnet;
    [SerializeField] private ParticleSystem tornadoFx;

    [SerializeField] private AudioSource magnetSound;
    [SerializeField] private AudioSource tornadoSound;


    [SerializeField] private SkeletonAnimation magnetSpine;
    [SerializeField, SpineAnimation] private string magnetAnimation;
    private int cycleCount = 0;
    private TrackEntry trackEntry;

    private Vector3 _magnetInitialPosition;
    private Vector3 _magnetInitialScale;

    void Awake()
    {
        _magnetInitialPosition = magnetSpine.transform.localPosition;
        _magnetInitialScale = magnetSpine.transform.localScale;
    }

    public void Vacumn(Action<Vector3> onCompletedAction)
    {
        magnetSpine.gameObject.SetActive(true);

        magnetSpine.transform.position = new Vector3(magnetSpine.transform.position.x, -GamePersistentVariable.screenSizeWorld.y);

        Tween.PositionY(magnetSpine.transform, -0.4f * GamePersistentVariable.screenSizeWorld.y, duration: 0.6f)
        .OnComplete(() =>
        {
            magnetSpine.AnimationState.SetAnimation(0, magnetAnimation, true);
            trackEntry = magnetSpine.AnimationState.SetAnimation(0, magnetAnimation, true);
            trackEntry.Complete += OnAnimationComplete;

            SoundManager.Instance.PlaySoundMagnet();

            onCompletedAction?.Invoke(transform.position);

            // Tween.Delay(1).OnComplete(() =>
            // {
            //     Tween.Position(magnetSpine.transform, new Vector3(magnetSpine.transform.position.x, -GamePersistentVariable.screenSizeWorld.y), duration: 0.3f)
            //     .OnComplete(() =>
            //     {
            //         magnetSpine.gameObject.SetActive(false);
            //     });
            // });
        });

        void OnAnimationComplete(TrackEntry entry)
        {
            cycleCount++;

            if (cycleCount == 3)
            {
                entry.Complete -= OnAnimationComplete;

                magnetSpine.AnimationState.ClearTrack(0);

                cycleCount = 0;

                Tween.Position(magnetSpine.transform, new Vector3(magnetSpine.transform.position.x, -GamePersistentVariable.screenSizeWorld.y), duration: 0.3f)
                .OnComplete(() =>
                {
                    magnetSpine.gameObject.SetActive(false);
                });
            }
        }
    }

    // public void Vacumn(Action<Vector3> onCompletedAction)
    // {
    //     gameObject.SetActive(true);

    //     transform.position = new Vector3(0, -24, 0);
    //     magnet.localPosition = new Vector3(0, 0, 0);

    //     Tween.PositionY(transform, -6, duration: 0.5f).OnComplete(() =>
    //     {
    //         tornadoFx.Play();

    //         if (GamePersistentVariable.isEnableSound)
    //         {
    //             magnetSound.volume = 0.5f;
    //             tornadoSound.volume = 1;

    //             magnetSound.Play();
    //             tornadoSound.Play();
    //         }

    //         Tween.ShakeLocalRotation(magnet, new Vector3(0, 16, 0), duration: 1.2f);
    //         Tween.Delay(1.2f).OnComplete(() =>
    //         {
    //             tornadoFx.Stop();

    //             // Dissolve();

    //             Tween.AudioVolume(magnetSound, 0, duration: 0.5f);
    //             Tween.AudioVolume(tornadoSound, 0, duration: 0.5f);

    //             Tween.PositionY(magnet, -12, duration: 0.5f).OnComplete(() =>
    //             {
    //                 gameObject.SetActive(false);
    //             });
    //         });

    //         onCompletedAction?.Invoke(transform.position);
    //     });
    // }

    public void Dissolve()
    {
        boosterVacumnMaterialPropertyBlock.Dissolve();
    }
}
