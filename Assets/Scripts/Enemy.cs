using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum MovementType
    {
        Straight,
        SideToSide,
        Circular,
        Angled
    }

    public delegate void EnemyDestroyedDelegate();
    public event EnemyDestroyedDelegate OnEnemyDestroyed;

    [SerializeField]
    private MovementType _movementType = MovementType.Straight;
    [SerializeField]
    private float _speed = 4f;
    
    [SerializeField]
    private float _sideToSideAmplitude = 3f;
    
    [SerializeField]
    private float _sideToSideFrequency = 2f;
    
    [SerializeField]
    private float _circleRadius = 2f;

    [SerializeField]
    private float _angledEntryAngle = 30f;

    private Vector3 _startPosition;
    private float _timeStarted;
    private Vector3 _direction;

    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private int _shieldSpawnChance = 30;
    [SerializeField]
    private float _ramDetectionRange = 3f;
    [SerializeField]
    private float _ramSpeedMultiplier = 2.5f;

    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;

    private float _fireRate = 3.0f;
    private float _canFire = -1f;
    
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
                Debug.Log($"✓ Enemy WITH shield - Active: {_shieldVisualizer.activeSelf}");
            }
            else
            {
                _shieldVisualizer.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Shield Visualizer NOT assigned!");
        }
        
        _startPosition = transform.position;
        _timeStarted = Time.time;
        
        if (_movementType == MovementType.Angled)
        {
            float angleInRadians = _angledEntryAngle * Mathf.Deg2Rad;
            _direction = new Vector3(Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0).normalized;
        }
    }

    void Update()
    {
        CalculateMovement();
        
        if (ShouldShootAtPowerup())
        {
            _fireRate = Random.Range(1f, 2f);
            _canFire = Time.time + _fireRate;
            GameObject enemyLaser = Instantiate(_laserPrefab, transform.position, Quaternion.identity);
            Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
            for (int i = 0; i < lasers.Length; i++)
            {
                lasers[i].AssignEnemyLaser();
            }
        }
        else if (Time.time > _canFire)
        {
            _fireRate = Random.Range(3f, 7f);
            _canFire = Time.time + _fireRate;
            GameObject enemyLaser = Instantiate(_laserPrefab, transform.position, Quaternion.identity);
            Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
            for (int i = 0; i < lasers.Length; i++)
            {
                lasers[i].AssignEnemyLaser();
            }
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

    public void SetRandomMovementPattern()
    {
        int randomType = Random.Range(0, 4);
        _movementType = (MovementType)randomType;
        
        switch (_movementType)
        {
            case MovementType.Straight:
                _speed = Random.Range(3f, 6f);
                break;
                
            case MovementType.SideToSide:
                _speed = Random.Range(2f, 5f);
                _sideToSideAmplitude = Random.Range(2f, 4f);
                _sideToSideFrequency = Random.Range(1.5f, 3f);
                break;
                
            case MovementType.Circular:
                _speed = Random.Range(2f, 4f);
                _circleRadius = Random.Range(1.5f, 3f);
                break;
                
            case MovementType.Angled:
                _speed = Random.Range(3f, 5f);
                _angledEntryAngle = Random.Range(20f, 50f) * (Random.value > 0.5f ? 1 : -1);
                float angleInRadians = _angledEntryAngle * Mathf.Deg2Rad;
                _direction = new Vector3(Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0).normalized;
                break;
        }
        
        _startPosition = transform.position;
        _timeStarted = Time.time;
    }

    void CalculateMovement()
    {
        if (_player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
            
            if (distanceToPlayer <= _ramDetectionRange && !_isRamming)
            {
                _isRamming = true;
            }
            
            if (_isRamming)
            {
                RamPlayer();
            }
            else
            {
                switch (_movementType)
                {
                    case MovementType.Straight:
                        MoveStraight();
                        break;
                    case MovementType.SideToSide:
                        MoveSideToSide();
                        break;
                    case MovementType.Circular:
                        MoveCircular();
                        break;
                    case MovementType.Angled:
                        MoveAngled();
                        break;
                }
            }
        }
        
        if (transform.position.y <= -6f || transform.position.x < -10f || transform.position.x > 10f)
        {
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject);
        }    
    }
    
    void MoveStraight()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
    }
    
    void MoveSideToSide()
    {
        float timeSinceStart = Time.time - _timeStarted;
        float xOffset = Mathf.Sin(timeSinceStart * _sideToSideFrequency) * _sideToSideAmplitude;
        float newY = transform.position.y - _speed * Time.deltaTime;
        transform.position = new Vector3(_startPosition.x + xOffset, newY, 0);
    }
    
    void MoveCircular()
    {
        float timeSinceStart = Time.time - _timeStarted;
        float xOffset = Mathf.Cos(timeSinceStart * _speed) * _circleRadius;
        float yOffset = Mathf.Sin(timeSinceStart * _speed) * _circleRadius;
        float newY = _startPosition.y - timeSinceStart * (_speed * 0.5f);
        transform.position = new Vector3(_startPosition.x + xOffset, newY + yOffset, 0);
    }
    
    void MoveAngled()
    {
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }
    
    void RamPlayer()
    {
        if (_player != null)
        {
            Vector3 direction = (_player.transform.position - transform.position).normalized;
            transform.position += direction * _speed * _ramSpeedMultiplier * Time.deltaTime;
        }
    }
    
    void RespawnAtTop()
    {
        float randomX = Random.Range(-8f, 8f);
        transform.position = new Vector3(randomX, 6f, 0);
        _startPosition = transform.position;
        _timeStarted = Time.time;
        
        if (_movementType == MovementType.Angled)
        {
            float angleInRadians = _angledEntryAngle * Mathf.Deg2Rad;
            _direction = new Vector3(Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0).normalized;
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
                _player.AddScore(10);
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
