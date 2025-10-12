using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAdvanced : MonoBehaviour
{
    public delegate void EnemyDestroyedDelegate();
    public event EnemyDestroyedDelegate OnEnemyDestroyed;

    [SerializeField]
    private float _speed = 2.5f;
    [SerializeField]
    private float _waveAmplitude = 3f;
    [SerializeField]
    private float _waveFrequency = 2f;

    private Vector3 _startPosition;
    private float _timeStarted;

    [SerializeField]
    private GameObject _missilePrefab;
    [SerializeField]
    private float _fireRate = 5f;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private int _shieldSpawnChance = 50;
    [SerializeField]
    private float _ramDetectionRange = 3.5f;
    [SerializeField]
    private float _ramSpeedMultiplier = 3f;

    private float _canFire = -1f;

    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    
    private bool _hasShield = false;
    private bool _isRamming = false;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        _audioSource = GetComponent<AudioSource>();
        if (_player == null)
        {
            Debug.LogError("The Player is NULL!");
        }
        _anim = GetComponent<Animator>();
        
        if (_shieldVisualizer != null)
        {
            int shieldRoll = Random.Range(0, 100);
            if (shieldRoll < _shieldSpawnChance)
            {
                _hasShield = true;
                _shieldVisualizer.SetActive(true);
                Debug.Log($"✓ Advanced Enemy WITH shield - Active: {_shieldVisualizer.activeSelf}");
            }
            else
            {
                _shieldVisualizer.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Shield Visualizer NOT assigned on Advanced Enemy!");
        }
        
        _startPosition = transform.position;
        _timeStarted = Time.time;
        _canFire = Time.time + Random.Range(2f, 3f);
    }

    void Update()
    {
        if (_player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
            
            if (distanceToPlayer <= _ramDetectionRange && !_isRamming)
            {
                _isRamming = true;
            }
        }
        
        if (_isRamming)
        {
            RamPlayer();
        }
        else
        {
            MoveInWavePattern();
        }
        
        if (ShouldShootAtPowerup())
        {
            _canFire = Time.time + 1.5f;
            FireMissile();
        }
        else if (Time.time > _canFire && _missilePrefab != null)
        {
            _canFire = Time.time + _fireRate;
            FireMissile();
        }
    }
    
    bool ShouldShootAtPowerup()
    {
        if (Time.time < _canFire || _missilePrefab == null) return false;
        
        GameObject[] powerups = GameObject.FindGameObjectsWithTag("Powerup");
        foreach (GameObject powerup in powerups)
        {
            if (powerup.transform.position.y < transform.position.y)
            {
                float horizontalDistance = Mathf.Abs(powerup.transform.position.x - transform.position.x);
                float verticalDistance = transform.position.y - powerup.transform.position.y;
                
                if (horizontalDistance <= 1.5f && verticalDistance <= 5f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void MoveInWavePattern()
    {
        float timeSinceStart = Time.time - _timeStarted;
        float xOffset = Mathf.Sin(timeSinceStart * _waveFrequency) * _waveAmplitude;
        float newY = transform.position.y - _speed * Time.deltaTime;
        transform.position = new Vector3(_startPosition.x + xOffset, newY, 0);

        if (transform.position.y <= -6f || transform.position.x < -10f || transform.position.x > 10f)
        {
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject);
        }
    }

    void RamPlayer()
    {
        if (_player != null)
        {
            Vector3 direction = (_player.transform.position - transform.position).normalized;
            transform.position += direction * _speed * _ramSpeedMultiplier * Time.deltaTime;
        }
    }
    
    void FireMissile()
    {
        GameObject missile = Instantiate(_missilePrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        
        if (_audioSource != null)
        {
            _audioSource.Play();
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
                _player.AddScore(20);
            }
            
            if (_anim != null)
            {
                _anim.SetTrigger("OnEnemyDeath");
            }
            _speed = 0;
            
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
            
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
            
            if (_anim != null)
            {
                _anim.SetTrigger("OnEnemyDeath");
            }
            _speed = 0;
            
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
            
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject, 1.5f);
        }
    }
}
