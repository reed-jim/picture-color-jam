using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    public float speedMultiply;

    void Awake()
    {
        JoystickController.controlPlayerEvent += Move;
    }

    void OnDestroy()
    {
        JoystickController.controlPlayerEvent -= Move;
    }

    public void Move(Vector2 normalizedDirection)
    {
        transform.position += speedMultiply * (Vector3)normalizedDirection;
    }
}
