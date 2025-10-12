using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDodger : MonoBehaviour
{
    public delegate void EnemyDestroyedDelegate();
    public event EnemyDestroyedDelegate OnEnemyDestroyed;

    [SerializeField]
    private float _speed = 3f;
    [SerializeField]
    private float _dodgeSpeed = 6f;
    [SerializeField]
    private float _laserDetectionRange = 3f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private int _shieldSpawnChance = 20;

    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    private float _fireRate = 3.0f;
    private float _canFire = -1f;
    private bool _hasShield = false;
    private bool _isDodging = false;
    private Vector3 _dodgeDirection;

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
        CheckForIncomingLasers();
        
        if (_isDodging)
        {
            transform.position += _dodgeDirection * _dodgeSpeed * Time.deltaTime;
        }
        else
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
        }

        if (Time.time > _canFire)
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

        if (transform.position.y <= -6f || transform.position.x < -10f || transform.position.x > 10f)
        {
            if (OnEnemyDestroyed != null)
            {
                OnEnemyDestroyed();
            }
            Destroy(this.gameObject);
        }
    }

    void CheckForIncomingLasers()
    {
        GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");
        
        foreach (GameObject laserObj in lasers)
        {
            Laser laser = laserObj.GetComponent<Laser>();
            if (laser != null && !laser.IsEnemyLaser())
            {
                float distance = Vector3.Distance(transform.position, laserObj.transform.position);
                
                if (distance <= _laserDetectionRange && laserObj.transform.position.y < transform.position.y)
                {
                    float horizontalDistance = Mathf.Abs(laserObj.transform.position.x - transform.position.x);
                    
                    if (horizontalDistance <= 1f)
                    {
                        PerformDodge(laserObj.transform.position);
                        return;
                    }
                }
            }
        }
        
        _isDodging = false;
    }

    void PerformDodge(Vector3 laserPosition)
    {
        _isDodging = true;
        
        float dodgeX = (transform.position.x > laserPosition.x) ? 1f : -1f;
        
        if (transform.position.x > 8f)
        {
            dodgeX = -1f;
        }
        else if (transform.position.x < -8f)
        {
            dodgeX = 1f;
        }
        
        _dodgeDirection = new Vector3(dodgeX, 0, 0).normalized;
        
        StartCoroutine(StopDodgeAfterDelay());
    }

    IEnumerator StopDodgeAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        _isDodging = false;
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
