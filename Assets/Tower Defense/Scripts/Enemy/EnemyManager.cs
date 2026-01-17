using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    public LevelEnemyWaveData levelEnemyWaveData;

    public Transform homeBase;

    private LevelEnemyWaveData currentLevelEnemyWaveData;

    public TMP_Text waveText;
    public TMP_Text timeLeftForWaveText;
    public TMP_Text totalEnemyText;
    public int timeToNextWave;
    public int currentWave;

    public static event Action winLevelEvent;

    void Start()
    {
        currentLevelEnemyWaveData = new LevelEnemyWaveData();
        // currentLevelEnemyWaveData.enemyWaves = levelEnemyWaveData.enemyWaves;

        currentLevelEnemyWaveData.enemyWaves = new List<EnemyWave>();

        for (int i = 0; i < 8; i++)
        {
            EnemyWave enemyWave = new EnemyWave();

            enemyWave.totalSeconds = 10;
            List<EnemyTypeAndNumberPair> list = new List<EnemyTypeAndNumberPair>
            {
                new EnemyTypeAndNumberPair()
                {
                    enemyType = (EnemyType)UnityEngine.Random.Range(0, 1),
                    number = 10
                }
            };

            enemyWave.enemies = list;

            currentLevelEnemyWaveData.enemyWaves.Add(enemyWave);
        }

        timeToNextWave = 10;

        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        WaitForSeconds waitForOneSecond = new WaitForSeconds(1);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        bool isNewWave = true;
        int numEnemyToSpawn = 1;

        yield return new WaitUntil(() => GamePersistentVariable.isLoadingComplete && GamePersistentVariable.isGameStarted);

        currentWave = 1;
        int totalWave = currentLevelEnemyWaveData.enemyWaves.Count;

        while (currentWave < currentLevelEnemyWaveData.enemyWaves.Count)
        {
            EnemyWave currentEnemyWave = currentLevelEnemyWaveData.enemyWaves[currentWave];

            if (isNewWave)
            {
                timeToNextWave = currentEnemyWave.totalSeconds;
                waveText.text = $"Wave: {currentWave}/{totalWave}";

                isNewWave = false;
            }

            timeLeftForWaveText.text = ConvertSecondsToMinutesSeconds(timeToNextWave);

            timeToNextWave--;
            int enemyLeft = currentEnemyWave.enemies.Sum(item => item.number);
            numEnemyToSpawn = (int)Mathf.Max(UnityEngine.Random.Range(1, 0.5f * enemyLeft), 1);
            int numSpawnedEnemy = numEnemyToSpawn;

            while (numSpawnedEnemy > 0)
            {
                SpawnSingleEnemy(currentEnemyWave);

                GamePersistentVariable.totalEnemy++;

                numSpawnedEnemy--;
            }

            enemyLeft = currentEnemyWave.enemies.Sum(item => item.number);

            UpdateTotalEnemyCount();

            if (enemyLeft == 0)
            {
                // currentLevelEnemyWaveData.enemyWaves.RemoveAt(0);

                currentWave++;
                isNewWave = true;
            }

            yield return waitForSeconds;
        }
    }

    private void SpawnSingleEnemy(EnemyWave enemyWave)
    {
        for (int i = 0; i < enemyWave.enemies.Count; i++)
        {
            int index = i;

            if (enemyWave.enemies[index].number > 0)
            {
                EnemyType enemyType = enemyWave.enemies[index].enemyType;

                BaseEnemy enemy = ObjectPoolingEverything.GetFromPool<BaseEnemy>(enemyType.ToString());
                enemy.Reset();
                enemy.destination = homeBase;

                Vector3 spawnPosition = new Vector3(0, 20, -1);
                spawnPosition.x = UnityEngine.Random.Range(-0.5f * GamePersistentVariable.screenSizeWorld.x, 0.5f * GamePersistentVariable.screenSizeWorld.x);
                enemy.transform.position = spawnPosition;

                enemy.MoveToBase();

                EnemyTypeAndNumberPair enemyTypeAndNumberPair = enemyWave.enemies[index];
                enemyTypeAndNumberPair.number--;

                enemyWave.enemies[index] = enemyTypeAndNumberPair;

                break;
            }
        }
    }

    public void UpdateTotalEnemyCount()
    {
        totalEnemyText.text = $"{GamePersistentVariable.totalEnemy}";

        if (GamePersistentVariable.totalEnemy <= 0)
        {
            winLevelEvent?.Invoke();
        }
    }

    string ConvertSecondsToMinutesSeconds(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);

        return string.Format("{0:D2}:{1:D2}", minutes, remainingSeconds);
    }
}
