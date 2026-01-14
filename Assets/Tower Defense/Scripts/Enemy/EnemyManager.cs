using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public LevelEnemyWaveData levelEnemyWaveData;

    public Transform homeBase;

    private LevelEnemyWaveData currentLevelEnemyWaveData;

    public TMP_Text waveText;
    public TMP_Text timeLeftForWaveText;
    public TMP_Text enemyLeftOfWaveText;
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
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        bool isNewWave = true;
        int numEnemyToSpawn = 1;

        yield return new WaitUntil(() => GamePersistentVariable.isLoadingComplete && GamePersistentVariable.isGameStarted);

        currentWave = 1;
        int totalWave = currentLevelEnemyWaveData.enemyWaves.Count;

        while (currentLevelEnemyWaveData.enemyWaves.Count > 0)
        {
            EnemyWave currentEnemyWave = currentLevelEnemyWaveData.enemyWaves.First();

            if (isNewWave)
            {
                timeToNextWave = currentEnemyWave.totalSeconds;
                waveText.text = $"Wave: {currentWave}/{totalWave}";

                isNewWave = false;
            }

            timeLeftForWaveText.text = ConvertSecondsToMinutesSeconds(timeToNextWave);

            timeToNextWave--;
            int enemyLeft = currentEnemyWave.enemies.Sum(item => item.number);
            numEnemyToSpawn = (int)Mathf.Max(Random.Range(1, 0.5f * enemyLeft), 1);
            int numSpawnedEnemy = numEnemyToSpawn;

            while (numSpawnedEnemy > 0)
            {
                SpawnSingleEnemy(currentEnemyWave);

                numSpawnedEnemy--;
            }

            enemyLeft = currentEnemyWave.enemies.Sum(item => item.number);

            enemyLeftOfWaveText.text = $"{enemyLeft}";

            if (enemyLeft == 0)
            {
                currentLevelEnemyWaveData.enemyWaves.RemoveAt(0);

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
            if (enemyWave.enemies[i].number > 0)
            {
                EnemyType enemyType = enemyWave.enemies[i].enemyType;

                BaseEnemy enemy = ObjectPoolingEverything.GetFromPool<BaseEnemy>(enemyType.ToString());
                enemy.Reset();
                enemy.destination = homeBase;

                Vector3 spawnPosition = new Vector3(0, 20, -1);
                spawnPosition.x = Random.Range(-0.5f * GamePersistentVariable.screenSizeWorld.x, 0.5f * GamePersistentVariable.screenSizeWorld.x);
                enemy.transform.position = spawnPosition;

                enemy.MoveToBase();

                EnemyTypeAndNumberPair enemyTypeAndNumberPair = enemyWave.enemies[i];
                enemyTypeAndNumberPair.number--;

                enemyWave.enemies[i] = enemyTypeAndNumberPair;

                break;
            }
        }
    }

    string ConvertSecondsToMinutesSeconds(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);

        return string.Format("{0:D2}:{1:D2}", minutes, remainingSeconds);
    }
}
