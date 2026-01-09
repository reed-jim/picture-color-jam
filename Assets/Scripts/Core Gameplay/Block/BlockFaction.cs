using System.Linq;
using UnityEngine;
using static GameEnum;

public class BlockFaction : MonoBehaviour
{
    [SerializeField] private Renderer blockRenderer;

    [SerializeField] private GameFaction faction;

    [SerializeField] private Material[] materials;

    public GameFaction Faction
    {
        get => faction;
        set
        {
            faction = value;

            UpdateFaction();
        }
    }

    public Color Color
    {
        get => blockRenderer.material.GetColor("_Color");
    }

    private void OnValidate()
    {
        UpdateFaction();
    }

    public void UpdateFaction()
    {
        if (blockRenderer == null)
        {
            return;
        }

        blockRenderer.material = materials[(int)faction];
    }
}
