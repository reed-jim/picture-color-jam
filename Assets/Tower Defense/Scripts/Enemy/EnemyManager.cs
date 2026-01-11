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

    void Start()
    {
        currentLevelEnemyWaveData = levelEnemyWaveData;
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

        while (currentLevelEnemyWaveData.enemyWaves.Count > 0)
        {
            EnemyWave currentEnemyWave = currentLevelEnemyWaveData.enemyWaves.First();
            EnemyType currentEnemyType = currentEnemyWave.enemies.First();

            BaseEnemy enemy = ObjectPoolingEverything.GetFromPool<BaseEnemy>(currentEnemyType.ToString());

            enemy.destination = homeBase;

            spawnPosition.x = Random.Range(-0.5f * GamePersistentVariable.screenSizeWorld.x, 0.5f * GamePersistentVariable.screenSizeWorld.x);
            enemy.transform.position = spawnPosition;

            currentEnemyWave.enemies.RemoveAt(0);

            if (currentEnemyWave.enemies.Count == 0)
            {
                currentLevelEnemyWaveData.enemyWaves.RemoveAt(0);
            }

            yield return waitForSeconds;
        }
    }
}
