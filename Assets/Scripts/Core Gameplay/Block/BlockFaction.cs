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
