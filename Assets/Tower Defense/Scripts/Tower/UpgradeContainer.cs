using System;
using UnityEngine;
using static GameEnum;

[CreateAssetMenu(fileName = "UpgradeContainer", menuName = "Scriptable Objects/UpgradeContainer")]
public class UpgradeContainer : ScriptableObject
{
    public UpgradeOption[] upgradeOptions;
    public TowerTypeAndIconPair[] towerTypeAndIconPairs;
}

[Serializable]
public class UpgradeOption
{
    public TowerType towerType;
    public bool isBuff;
    public float buffRatio;
}

[Serializable]
public class TowerTypeAndIconPair
{
    public TowerType towerType;
    public Sprite icon;
}
