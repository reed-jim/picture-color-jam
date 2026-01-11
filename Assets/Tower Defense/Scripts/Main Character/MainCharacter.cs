using System.Collections;
using PrimeTween;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    public Transform weapon;
    public Collider2D weaponCollider;

    public float speedMultiply;

    void Awake()
    {
        JoystickController.controlPlayerEvent += Move;
    }

    void OnDestroy()
    {
        JoystickController.controlPlayerEvent -= Move;
    }

    void Start()
    {
        StartCoroutine(AttackPeriodically());
    }

    public void Move(Vector2 normalizedDirection)
    {
        transform.position += speedMultiply * (Vector3)normalizedDirection;
    }

    private IEnumerator AttackPeriodically()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            weaponCollider.enabled = true;

            Tween.EulerAngles(weapon, weapon.eulerAngles, weapon.eulerAngles + new Vector3(0, 0, 720), duration: 0.15f).OnComplete(() =>
            {
                weaponCollider.enabled = false;
            });

            yield return waitForSeconds;
        }
    }
}
