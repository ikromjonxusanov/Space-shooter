using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private GameObject _enemyContainer;

    [SerializeField]
    private GameObject[] _powerups;

    [SerializeField]
    private GameObject[] _rarePowerups;

    [SerializeField]
    private int _rareSpawnChance = 15; // 15% chance for rare powerup

    private bool _stopSpawning = false;

    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
    }

    void Update()
    {
        
    }

    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        while (!_stopSpawning)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        while (!_stopSpawning)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
            
            // Check if we should spawn a rare powerup
            int roll = Random.Range(0, 100);
            if (roll < _rareSpawnChance && _rarePowerups != null && _rarePowerups.Length > 0)
            {
                // Spawn rare powerup
                int randomRarePowerup = Random.Range(0, _rarePowerups.Length);
                Instantiate(_rarePowerups[randomRarePowerup], posToSpawn, Quaternion.identity);
            }
            else
            {
                // Spawn normal powerup
                int randomPowerUp = Random.Range(0, _powerups.Length);
                Instantiate(_powerups[randomPowerUp], posToSpawn, Quaternion.identity);
            }
            
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
        KillAllEnemies();
    }

    public void KillAllEnemies()
    {
        foreach (Transform child in _enemyContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
