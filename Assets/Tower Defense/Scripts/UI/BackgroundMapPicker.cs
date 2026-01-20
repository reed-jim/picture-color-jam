using UnityEngine;
using UnityEngine.UI;

public class BackgroundMapPicker : MonoBehaviour
{
    public Image background;

    public Sprite[] backgroundSprites;

    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
    }

    private void OnLevelStarted()
    {
        background.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
    }
}
