using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBackfiring : MonoBehaviour
{
    public delegate void EnemyDestroyedDelegate();
    public event EnemyDestroyedDelegate OnEnemyDestroyed;

    [SerializeField]
    private float _speed = 3f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private int _shieldSpawnChance = 25;
    [SerializeField]
    private float _fireRate = 2.5f;
    [SerializeField]
    private float _backfireDetectionRange = 2f;

    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    private float _canFire = -1f;
    private bool _hasShield = false;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        _audioSource = GetComponent<AudioSource>();
        if (_player == null)
        {
            Debug.LogError("The Player is NULL!");
        }
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("The Animator is NULL!");
        }

        if (_shieldVisualizer != null)
        {
            int shieldRoll = Random.Range(0, 100);
            if (shieldRoll < _shieldSpawnChance)
            {
                _hasShield = true;
                _shieldVisualizer.SetActive(true);
            }
            else
            {
                _shieldVisualizer.SetActive(false);
            }
        }
    }

    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (ShouldShootAtPowerup())
        {
            FireAtPowerup();
        }
        else if (Time.time > _canFire && _player != null)
        {
            CheckAndFire();
        }

        if (transform.position.y <= -6f)
        {
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject);
        }
    }
    
    bool ShouldShootAtPowerup()
    {
        if (Time.time < _canFire) return false;
        
        GameObject[] powerups = GameObject.FindGameObjectsWithTag("Powerup");
        foreach (GameObject powerup in powerups)
        {
            if (powerup.transform.position.y < transform.position.y)
            {
                float horizontalDistance = Mathf.Abs(powerup.transform.position.x - transform.position.x);
                float verticalDistance = transform.position.y - powerup.transform.position.y;
                
                if (horizontalDistance <= 1f && verticalDistance <= 4f)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    void FireAtPowerup()
    {
        _fireRate = Random.Range(1f, 2f);
        _canFire = Time.time + _fireRate;
        FireForward();
    }

    void CheckAndFire()
    {
        float playerY = _player.transform.position.y;
        float enemyY = transform.position.y;
        float horizontalDistance = Mathf.Abs(_player.transform.position.x - transform.position.x);

        if (playerY > enemyY && horizontalDistance <= _backfireDetectionRange)
        {
            FireBackward();
        }
        else
        {
            FireForward();
        }

        _fireRate = Random.Range(2f, 4f);
        _canFire = Time.time + _fireRate;
    }

    void FireForward()
    {
        GameObject enemyLaser = Instantiate(_laserPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
        for (int i = 0; i < lasers.Length; i++)
        {
            lasers[i].AssignEnemyLaser();
        }
    }

    void FireBackward()
    {
        GameObject enemyLaser = Instantiate(_laserPrefab, transform.position + Vector3.up * 0.5f, Quaternion.Euler(0, 0, 180));
        Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
        for (int i = 0; i < lasers.Length; i++)
        {
            lasers[i].AssignEnemyLaser();
            lasers[i].ReverseDirection();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser != null && laser.IsEnemyLaser())
            {
                return;
            }

            Destroy(other.gameObject);

            if (_hasShield)
            {
                _hasShield = false;
                if (_shieldVisualizer != null)
                {
                    _shieldVisualizer.SetActive(false);
                }
                if (_audioSource != null)
                {
                    _audioSource.Play();
                }
                return;
            }

            if (_player != null)
            {
                _player.AddScore(15);
            }
            _anim.SetTrigger("OnEnemyDeath");
            _speed = 0;
            _audioSource.Play();
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject, 2f);
        }

        if (other.CompareTag("Player"))
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
            _anim.SetTrigger("OnEnemyDeath");
            _speed = 0;
            _audioSource.Play();
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject, 1.5f);
        }
    }
}
