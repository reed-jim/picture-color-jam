using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class JoystickController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform joystickContainer;
    [SerializeField] private RectTransform joystick;

    [Header("CUSTOMIZE")]
    [SerializeField] private float newInputThreshold;

    #region PRIVATE FIELD
    private Vector2 _screenSize;
    private List<Vector2> _touchPoints;

    private Image _joystickImage;

    private List<Tween> _tweens;
    private Tween _fadeInJoystickTween;
    private Tween _fadeOutJoystickTween;
    private bool _isEnableInput = true;
    private bool _isRotating;
    private Vector2 _defaultJoystickContainerPosition;
    #endregion

    #region ACTION
    public static event Action<Vector2> controlPlayerEvent;
    #endregion

    private void Awake()
    {
        _screenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

        _touchPoints = new List<Vector2>();

        BuildUI();
    }

    private void OnEnable()
    {
        StartCoroutine(ControlJoystick());
    }

    private void BuildUI()
    {
        _joystickImage = joystick.GetComponent<Image>();

        joystickContainer.sizeDelta = 0.2f * new Vector2(_screenSize.x, _screenSize.x);
        joystick.sizeDelta = 0.08f * new Vector2(_screenSize.x, _screenSize.x);
    }

    private IEnumerator ControlJoystick()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.02f);

        while (true)
        {
            if (_isEnableInput && Input.GetMouseButton(0))
            {
                Vector2 mousePosition = (Vector2)Input.mousePosition - 0.5f * _screenSize;

                if (_touchPoints.Count < 2)
                {
                    if (_touchPoints.Count == 0)
                    {
                        _touchPoints.Add(mousePosition);

                        SetJoystickContainerPosition(mousePosition);
                    }
                    else
                    {
                        float distance = Mathf.Abs(mousePosition.x - _touchPoints[0].x);

                        if (distance > newInputThreshold)
                        {
                            _touchPoints.Add(mousePosition);

                            SetJoystickPosition(mousePosition, joystickContainer.localPosition);
                        }
                    }
                }
                else
                {
                    float difference = mousePosition.x - _touchPoints[1].x;
                    float distance = Mathf.Abs(difference);

                    if (_isRotating == false)
                    {
                        // controlPlayerLevelEndlessEvent?.Invoke(mousePosition - _touchPoints[1]);

                        _isRotating = true;
                    }
                    else
                    {
                        // controlPlayerLevelEndlessEvent?.Invoke(mousePosition - (Vector2)joystickContainer.localPosition);
                    }

                    SetJoystickPosition(mousePosition, joystickContainer.localPosition);
                }
            }
            else
            {
                OnUserNotTouch();
            }

            yield return waitForSeconds;
        }
    }

    private void SetJoystickContainerPosition(Vector2 mousePosition)
    {
        joystickContainer.localPosition = mousePosition;
        joystick.localPosition = Vector2.zero;

        joystick.gameObject.SetActive(true);

        // if (_fadeInJoystickTween.isAlive)
        // {
        //     _fadeInJoystickTween.Stop();
        // }

        // if (_fadeOutJoystickTween.isAlive)
        // {
        //     _fadeOutJoystickTween.Stop();
        // }

        // if (_joystickImage.color.a != 1)
        // {
        //     _fadeInJoystickTween = Tween.Alpha(_joystickImage, 1, duration: 0.2f);
        // }
    }

    // THERE IS BETTER WAY TO DO THIS: use rotation
    private void SetJoystickPosition(Vector2 mousePosition, Vector2 joystickContainerPosition)
    {
        float radius = joystickContainer.sizeDelta.x / 2;

        Vector2 difference = mousePosition - joystickContainerPosition;
        Vector2 absoluteDifference = MathUtil.GetAbsoluteVector(difference);
        Vector2 normalizedDifference = difference;

        if (Mathf.Abs(difference.x) >= radius || Mathf.Abs(difference.y) >= radius)
        {
            float angle = Mathf.Atan2(absoluteDifference.y, absoluteDifference.x);

            normalizedDifference.x = radius * Mathf.Cos(angle);
            normalizedDifference.y = radius * Mathf.Sin(angle);

            if (difference.x < 0)
            {
                normalizedDifference.x *= -1;
            }

            if (difference.y < 0)
            {
                normalizedDifference.y *= -1;
            }
        }

        controlPlayerEvent?.Invoke(normalizedDifference);

        joystick.localPosition = normalizedDifference;
    }

    private void OnUserNotTouch()
    {
        FadeOutJoystick();

        _touchPoints.Clear();
    }

    private void FadeOutJoystick()
    {
        // if (_fadeInJoystickTween.isAlive)
        // {
        //     _fadeInJoystickTween.Stop();
        // }

        // if (_fadeOutJoystickTween.isAlive)
        // {
        //     _fadeOutJoystickTween.Stop();
        // }

        // if (_joystickImage.color.a != 0)
        // {
        //     _fadeOutJoystickTween = Tween.Alpha(_joystickImage, 0, duration: 0.2f);
        // }
    }
}
