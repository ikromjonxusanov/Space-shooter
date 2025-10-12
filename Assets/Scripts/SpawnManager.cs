using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject _enemyAdvancedPrefab;
    [SerializeField]
    private GameObject _enemyBackfiringPrefab;
    [SerializeField]
    private GameObject _enemyDodgerPrefab;
    [SerializeField]
    private int _advancedEnemySpawnChance = 20;
    [SerializeField]
    private int _backfiringEnemySpawnChance = 15;
    [SerializeField]
    private int _dodgerEnemySpawnChance = 15;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject _ammoPowerup;
    [SerializeField]
    private int _ammoSpawnWeight = 40;
    [SerializeField]
    private GameObject _tripleShot;
    [SerializeField]
    private int _tripleShotWeight = 25;
    [SerializeField]
    private GameObject _speedPowerup;
    [SerializeField]
    private int _speedWeight = 15;
    [SerializeField]
    private GameObject _shieldPowerup;
    [SerializeField]
    private int _shieldWeight = 12;
    [SerializeField]
    private GameObject _spreadShotPowerup;
    [SerializeField]
    private int _spreadShotWeight = 10;
    [SerializeField]
    private GameObject _healthPowerup;
    [SerializeField]
    private int _healthWeight = 5;
    [SerializeField]
    private GameObject _slowdownPowerup;
    [SerializeField]
    private int _slowdownWeight = 6;
    [SerializeField]
    private GameObject _bossPrefab;
    [SerializeField]
    private int _bossWave = 10;
    [SerializeField]
    private int _currentWave = 1;
    [SerializeField]
    private int _baseEnemiesPerWave = 5;
    [SerializeField]
    private float _baseSpawnDelay = 3f;
    [SerializeField]
    private float _minSpawnDelay = 0.5f;
    [SerializeField]
    private float _waveBreakDuration = 5f;
    [SerializeField]
    private UIManager _uiManager;

    private bool _stopSpawning = false;
    private int _enemiesSpawnedThisWave = 0;
    private int _enemiesAlive = 0;
    private float _currentSpawnDelay;

    public void StartSpawning()
    {
        _currentWave = 1;
        _currentSpawnDelay = _baseSpawnDelay;
        StartCoroutine(WaveController());
        StartCoroutine(SpawnPowerupRoutine());
    }

    IEnumerator WaveController()
    {
        yield return new WaitForSeconds(3.0f);
        
        while (!_stopSpawning)
        {
            if (_currentWave == _bossWave)
            {
                yield return StartCoroutine(SpawnBossWave());
                yield break;
            }
            
            int enemiesThisWave = _baseEnemiesPerWave + (_currentWave - 1) * 2;
            _currentSpawnDelay = Mathf.Max(_minSpawnDelay, _baseSpawnDelay - (_currentWave - 1) * 0.2f);
            
            if (_uiManager != null)
            {
                _uiManager.UpdateWave(_currentWave);
            }
            
            _enemiesSpawnedThisWave = 0;
            _enemiesAlive = 0;
            
            StartCoroutine(SpawnWaveEnemies(enemiesThisWave));
            
            yield return new WaitUntil(() => _enemiesSpawnedThisWave >= enemiesThisWave && _enemiesAlive <= 0);
            yield return new WaitForSeconds(_waveBreakDuration);
            
            _currentWave++;
        }
    }
    
    IEnumerator SpawnBossWave()
    {
        if (_uiManager != null)
        {
            _uiManager.UpdateWave(_currentWave);
        }
        
        Debug.Log("=== BOSS WAVE ===");
        yield return new WaitForSeconds(2f);
        
        if (_bossPrefab != null)
        {
            Vector3 bossSpawnPos = new Vector3(0, 8f, 0);
            GameObject boss = Instantiate(_bossPrefab, bossSpawnPos, Quaternion.identity);
            boss.transform.parent = _enemyContainer.transform;
            
            BossEnemy bossScript = boss.GetComponent<BossEnemy>();
            if (bossScript != null)
            {
                bossScript.OnBossDestroyed += BossDestroyed;
            }
            
            _enemiesAlive = 1;
            
            yield return new WaitUntil(() => _enemiesAlive <= 0);
            
            Debug.Log("=== BOSS DEFEATED! VICTORY! ===");
            _stopSpawning = true;
        }
    }
    
    void BossDestroyed()
    {
        _enemiesAlive--;
    }
    
    IEnumerator SpawnWaveEnemies(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (_stopSpawning) yield break;
            
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
            GameObject newEnemy;
            
            int spawnRoll = Random.Range(0, 100);
            if (spawnRoll < _advancedEnemySpawnChance && _enemyAdvancedPrefab != null)
            {
                newEnemy = Instantiate(_enemyAdvancedPrefab, posToSpawn, Quaternion.identity);
                newEnemy.transform.parent = _enemyContainer.transform;
                
                EnemyAdvanced advancedScript = newEnemy.GetComponent<EnemyAdvanced>();
                if (advancedScript != null)
                {
                    advancedScript.OnEnemyDestroyed += EnemyDestroyed;
                }
            }
            else if (spawnRoll < _advancedEnemySpawnChance + _backfiringEnemySpawnChance && _enemyBackfiringPrefab != null)
            {
                newEnemy = Instantiate(_enemyBackfiringPrefab, posToSpawn, Quaternion.identity);
                newEnemy.transform.parent = _enemyContainer.transform;
                
                EnemyBackfiring backfiringScript = newEnemy.GetComponent<EnemyBackfiring>();
                if (backfiringScript != null)
                {
                    backfiringScript.OnEnemyDestroyed += EnemyDestroyed;
                }
            }
            else if (spawnRoll < _advancedEnemySpawnChance + _backfiringEnemySpawnChance + _dodgerEnemySpawnChance && _enemyDodgerPrefab != null)
            {
                newEnemy = Instantiate(_enemyDodgerPrefab, posToSpawn, Quaternion.identity);
                newEnemy.transform.parent = _enemyContainer.transform;
                
                EnemyDodger dodgerScript = newEnemy.GetComponent<EnemyDodger>();
                if (dodgerScript != null)
                {
                    dodgerScript.OnEnemyDestroyed += EnemyDestroyed;
                }
            }
            else
            {
                newEnemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
                newEnemy.transform.parent = _enemyContainer.transform;
                
                Enemy enemyScript = newEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.SetRandomMovementPattern();
                    enemyScript.OnEnemyDestroyed += EnemyDestroyed;
                }
            }
            
            _enemiesSpawnedThisWave++;
            _enemiesAlive++;
            
            yield return new WaitForSeconds(_currentSpawnDelay);
        }
    }
    
    private void EnemyDestroyed()
    {
        _enemiesAlive--;
    }

    public int GetCurrentWave()
    {
        return _currentWave;
    }

    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        
        while (!_stopSpawning)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
            int totalWeight = _ammoSpawnWeight + _tripleShotWeight + _speedWeight + _shieldWeight + _spreadShotWeight + _healthWeight + _slowdownWeight;
            int roll = Random.Range(0, totalWeight);
            GameObject powerupToSpawn = null;
            
            if (roll < _ammoSpawnWeight && _ammoPowerup != null)
            {
                powerupToSpawn = _ammoPowerup;
            }
            else if (roll < _ammoSpawnWeight + _tripleShotWeight && _tripleShot != null)
            {
                powerupToSpawn = _tripleShot;
            }
            else if (roll < _ammoSpawnWeight + _tripleShotWeight + _speedWeight && _speedPowerup != null)
            {
                powerupToSpawn = _speedPowerup;
            }
            else if (roll < _ammoSpawnWeight + _tripleShotWeight + _speedWeight + _shieldWeight && _shieldPowerup != null)
            {
                powerupToSpawn = _shieldPowerup;
            }
            else if (roll < _ammoSpawnWeight + _tripleShotWeight + _speedWeight + _shieldWeight + _spreadShotWeight && _spreadShotPowerup != null)
            {
                powerupToSpawn = _spreadShotPowerup;
            }
            else if (roll < _ammoSpawnWeight + _tripleShotWeight + _speedWeight + _shieldWeight + _spreadShotWeight + _healthWeight && _healthPowerup != null)
            {
                powerupToSpawn = _healthPowerup;
            }
            else if (_slowdownPowerup != null)
            {
                powerupToSpawn = _slowdownPowerup;
            }
            
            if (powerupToSpawn != null)
            {
                Instantiate(powerupToSpawn, posToSpawn, Quaternion.identity);
            }
            
            yield return new WaitForSeconds(Random.Range(3, 7));
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
