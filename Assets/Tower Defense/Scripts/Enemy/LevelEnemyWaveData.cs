using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Slime,
    Ork,
    Bat,
    Monster,
    Boss
}

[CreateAssetMenu(fileName = "LevelEnemyWaveData", menuName = "Scriptable Objects/LevelEnemyWaveData")]
public class LevelEnemyWaveData : ScriptableObject
{
    public List<EnemyWave> enemyWaves;
}

[Serializable]
public class EnemyWave
{
    public int totalSeconds;
    public List<EnemyTypeAndNumberPair> enemies;
}

[Serializable]
public struct EnemyTypeAndNumberPair
{
    public EnemyType enemyType;
    public int number;
}
