using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public delegate void BossDestroyedDelegate();
    public event BossDestroyedDelegate OnBossDestroyed;

    [SerializeField]
    private int _maxHealth = 50;
    [SerializeField]
    private float _moveSpeed = 2f;
    [SerializeField]
    private Vector3 _centerPosition = new Vector3(0, 3f, 0);
    [SerializeField]
    private float _strafeSpeed = 3f;
    [SerializeField]
    private float _strafeRange = 4f;

    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _missilePrefab;
    [SerializeField]
    private Transform[] _firePoints;

    [SerializeField]
    private float _burstFireRate = 0.3f;
    [SerializeField]
    private float _missileFireRate = 4f;
    [SerializeField]
    private float _waveAttackRate = 6f;

    private int _currentHealth;
    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    
    private bool _hasReachedCenter = false;
    private float _strafeDirection = 1f;
    private float _nextBurstFire = 0f;
    private float _nextMissileFire = 0f;
    private float _nextWaveAttack = 0f;
    
    private Vector3 _startStrafePosition;

    void Start()
    {
        _currentHealth = _maxHealth;
        _player = GameObject.Find("Player").GetComponent<Player>();
        _audioSource = GetComponent<AudioSource>();
        if (_player == null)
        {
            Debug.LogError("The Player is NULL!");
        }
        _anim = GetComponent<Animator>();
        
        _startStrafePosition = _centerPosition;
    }

    void Update()
    {
        if (!_hasReachedCenter)
        {
            MoveToCenter();
        }
        else
        {
            StrafeMovement();
            PerformAttacks();
        }
    }

    void MoveToCenter()
    {
        transform.position = Vector3.MoveTowards(transform.position, _centerPosition, _moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, _centerPosition) < 0.1f)
        {
            _hasReachedCenter = true;
            _startStrafePosition = transform.position;
            _nextBurstFire = Time.time + 1f;
            _nextMissileFire = Time.time + 2f;
            _nextWaveAttack = Time.time + 3f;
        }
    }

    void StrafeMovement()
    {
        float targetX = _startStrafePosition.x + (_strafeRange * _strafeDirection);
        Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, _strafeSpeed * Time.deltaTime);
        
        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            _strafeDirection *= -1f;
        }
    }

    void PerformAttacks()
    {
        if (Time.time >= _nextBurstFire)
        {
            StartCoroutine(BurstFire());
            _nextBurstFire = Time.time + 3f;
        }

        if (Time.time >= _nextMissileFire)
        {
            FireMissiles();
            _nextMissileFire = Time.time + _missileFireRate;
        }

        if (Time.time >= _nextWaveAttack)
        {
            WaveAttack();
            _nextWaveAttack = Time.time + _waveAttackRate;
        }
    }

    IEnumerator BurstFire()
    {
        for (int i = 0; i < 5; i++)
        {
            if (_firePoints != null && _firePoints.Length > 0)
            {
                foreach (Transform firePoint in _firePoints)
                {
                    if (firePoint != null)
                    {
                        GameObject laser = Instantiate(_laserPrefab, firePoint.position, Quaternion.identity);
                        Laser[] lasers = laser.GetComponentsInChildren<Laser>();
                        foreach (Laser l in lasers)
                        {
                            l.AssignEnemyLaser();
                        }
                    }
                }
            }
            
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
            
            yield return new WaitForSeconds(_burstFireRate);
        }
    }

    void FireMissiles()
    {
        if (_missilePrefab != null && _player != null)
        {
            Vector3 leftPos = transform.position + new Vector3(-1f, 0, 0);
            Vector3 rightPos = transform.position + new Vector3(1f, 0, 0);
            
            Instantiate(_missilePrefab, leftPos, Quaternion.identity);
            Instantiate(_missilePrefab, rightPos, Quaternion.identity);
            
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
        }
    }

    void WaveAttack()
    {
        if (_laserPrefab != null)
        {
            float[] angles = { -45f, -30f, -15f, 0f, 15f, 30f, 45f };
            
            foreach (float angle in angles)
            {
                GameObject laser = Instantiate(_laserPrefab, transform.position, Quaternion.Euler(0, 0, angle));
                Laser[] lasers = laser.GetComponentsInChildren<Laser>();
                foreach (Laser l in lasers)
                {
                    l.AssignEnemyLaser();
                }
            }
            
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (_player != null)
        {
            _player.AddScore(500);
        }
        
        if (_anim != null)
        {
            _anim.SetTrigger("OnEnemyDeath");
        }
        
        _moveSpeed = 0;
        _strafeSpeed = 0;
        
        if (_audioSource != null)
        {
            _audioSource.Play();
        }
        
        if (OnBossDestroyed != null)
        {
            OnBossDestroyed();
        }
        
        Destroy(this.gameObject, 3f);
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
            TakeDamage(1);
        }

        if (other.CompareTag("Player"))
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
        }
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
    }
}
