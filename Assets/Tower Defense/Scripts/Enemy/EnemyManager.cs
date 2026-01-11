using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public LevelEnemyWaveData levelEnemyWaveData;

    public Transform homeBase;

    private LevelEnemyWaveData currentLevelEnemyWaveData;

    public TMP_Text timeToNextWaveText;
    public int timeToNextWave;
    public int currentWave;

    void Start()
    {
        currentLevelEnemyWaveData = new LevelEnemyWaveData();
        currentLevelEnemyWaveData.enemyWaves = levelEnemyWaveData.enemyWaves;
        timeToNextWave = 10;

        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        WaitForSeconds waitForOneSecond = new WaitForSeconds(1);
        WaitForSeconds waitForSeconds = new WaitForSeconds(2);

        Vector3 spawnPosition = new Vector3(0, 20, -1);

        yield return new WaitUntil(() => GamePersistentVariable.isLoadingComplete);

        while (timeToNextWave > 0)
        {
            timeToNextWave--;
            timeToNextWaveText.text = $"{timeToNextWave}";

            yield return waitForOneSecond;
        }

        currentWave = 1;
        timeToNextWaveText.text = $"Wave {currentWave}";

        while (currentLevelEnemyWaveData.enemyWaves.Count > 0)
        {
            EnemyWave currentEnemyWave = currentLevelEnemyWaveData.enemyWaves.First();
            EnemyType currentEnemyType = EnemyType.Melee;

            for (int i = 0; i < currentEnemyWave.enemies.Count; i++)
            {
                if (currentEnemyWave.enemies[i].number > 0)
                {
                    currentEnemyType = currentEnemyWave.enemies[i].enemyType;

                    EnemyTypeAndNumberPair enemyTypeAndNumberPair = currentEnemyWave.enemies[i];
                    enemyTypeAndNumberPair.number--;

                    currentEnemyWave.enemies[i] = enemyTypeAndNumberPair;

                    break;
                }
            }

            BaseEnemy enemy = ObjectPoolingEverything.GetFromPool<BaseEnemy>(currentEnemyType.ToString());
            enemy.Reset();
            enemy.destination = homeBase;

            spawnPosition.x = Random.Range(-0.5f * GamePersistentVariable.screenSizeWorld.x, 0.5f * GamePersistentVariable.screenSizeWorld.x);
            enemy.transform.position = spawnPosition;

            currentEnemyWave.enemies.RemoveAt(0);

            if (currentEnemyWave.enemies.Count == 0)
            {
                currentLevelEnemyWaveData.enemyWaves.RemoveAt(0);

                currentWave++;
                timeToNextWaveText.text = $"Wave {currentWave}";
            }

            yield return waitForSeconds;
        }
    }
}
