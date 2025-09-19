using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private GameObject _enemyContainer;

    private bool _stopSpawning = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // spawn game ivhects every 5 seconds
    // create coroutine of type IEnumerator -- yield event
    // while loop

    IEnumerator SpawnRoutine()
    {

        // while loop
        //instantiate enemy prefab
        //  yield wait for 5 seconds
        while (!_stopSpawning)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(3f);
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
