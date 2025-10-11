using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5f;
    private float _speedMultiplier = 2;

    [SerializeField]
    private float _boostMultiplier = 1.5f; // Left Shift boost factor

    // Thruster system
    [SerializeField]
    private float _thrusterMax = 3.0f;
    private float _thrusterDrainRate = 1.0f;
    private float _thrusterRechargeRate = 0.5f;
    private float _thrusterCooldownDuration = 2.0f;

    private float _thrusterCharge = 0f;
    private bool _thrusterCoolingDown = false;
    private float _thrusterCooldownTimer = 0f;

    [SerializeField]
    private GameObject _laserPrefab;

    [SerializeField]
    private GameObject _tripleShotPrefab;

    [SerializeField]
    private GameObject _spreadShotPrefab;

    [SerializeField]
    private float _fireRate = 0f;

    private float _canFire = -1f;
    
    [SerializeField]
    private int _maxAmmo = 15;
    private int _currentAmmo = 0;
    
    private bool _gameStarted = false;

    [SerializeField]
    private int _lives = 3;

    private SpawnManager _spawnManager;

    private bool _isTripleShotActive = false;
    private bool _isSpreadShotActive = false;
    private bool _isShieldActive = false;

    [SerializeField]
    private GameObject _shieldVisualizer;

    [SerializeField]
    private int _shieldMaxStrength = 3;
    private int _shieldStrength = 0;

    [SerializeField]
    private Color[] _shieldStrengthColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1f, 0.9f),
        new Color(1f, 0.85f, 0.2f, 0.85f), 
        new Color(1f, 0.2f, 0.2f, 0.8f) 
    };

    [SerializeField]
    private int _score;

    private UIManager _uiManager;

    [SerializeField]
    private GameObject _rightEngine, _leftEngine;

    [SerializeField]
    private AudioClip _laserSoundClip;
    private AudioSource _audioSource;
    
    [SerializeField]
    private AudioClip _noAmmoClip;

    private CameraShake _cameraShake;

    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
        _uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
        _audioSource = GetComponent<AudioSource>();
        _cameraShake = Camera.main.GetComponent<CameraShake>();

        if (_spawnManager == null)
        {
            Debug.LogError("SpawnManager is NULL!");
        }
        if (_uiManager == null)
        {
            Debug.LogError("UI Manager is NULL!");
        }
        if (_audioSource == null)
        {
            Debug.LogError("AudioSource on the player is NULL!");
        }
        else
        {
            _audioSource.clip = _laserSoundClip;
        }
        if (_cameraShake == null)
        {
            Debug.LogError("CameraShake is NULL!");
        }
        // Initialize thruster charge
        _thrusterCharge = _thrusterMax;
        
        // Don't initialize ammo yet - wait for game to start (asteroid destroyed)
        _currentAmmo = 0;
        if (_uiManager != null)
        {
            _uiManager.UpdateAmmo(_currentAmmo, _maxAmmo);
        }
    }

    void Update()
    {
        CalculateMovement();

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0)) && Time.time > _canFire)
        {
            // Allow unlimited shooting before game starts (to destroy asteroid)
            if (!_gameStarted)
            {
                FaireLaser();
            }
            else if (_currentAmmo > 0)
            {
                FaireLaser();
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);
                if (_uiManager != null)
                {
                    _uiManager.UpdateAmmo(_currentAmmo, _maxAmmo);
                }
            }
            else
            {
                // Throttle empty feedback a bit
                _canFire = Time.time + 0.2f;
                if (_noAmmoClip != null)
                {
                    AudioSource.PlayClipAtPoint(_noAmmoClip, Camera.main.transform.position);
                }
                if (_uiManager != null)
                {
                    _uiManager.NotifyNoAmmo();
                }
            }
        }
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        // Thruster boost gating by charge/cooldown
        bool boostRequested = Input.GetKey(KeyCode.LeftShift);
        bool canBoost = !_thrusterCoolingDown && _thrusterCharge > 0f;
        float currentSpeed = _speed * (boostRequested && canBoost ? _boostMultiplier : 1f);
        transform.Translate(direction * currentSpeed * Time.deltaTime);

        if (transform.position.y >= 0)
        {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
        else if (transform.position.y <= -3.8f)
        {
            transform.position = new Vector3(transform.position.x, -3.8f, 0);
        }

        if (transform.position.x > 11f)
        {
            transform.position = new Vector3(-11f, transform.position.y, 0);
        }
        else if (transform.position.x < -11f)
        {
            transform.position = new Vector3(11f, transform.position.y, 0);
        }

        // Update thruster charge, cooldown, and UI
        if (boostRequested && canBoost)
        {
            _thrusterCharge -= _thrusterDrainRate * Time.deltaTime;
            if (_thrusterCharge <= 0f)
            {
                _thrusterCharge = 0f;
                _thrusterCoolingDown = true;
                _thrusterCooldownTimer = _thrusterCooldownDuration;
            }
        }
        else
        {
            if (_thrusterCoolingDown)
            {
                _thrusterCooldownTimer -= Time.deltaTime;
                if (_thrusterCooldownTimer <= 0f)
                {
                    _thrusterCoolingDown = false;
                }
            }
            if (!_thrusterCoolingDown && _thrusterCharge < _thrusterMax)
            {
                _thrusterCharge = Mathf.Min(_thrusterMax, _thrusterCharge + _thrusterRechargeRate * Time.deltaTime);
            }
        }

        if (_uiManager != null)
        {
            float normalized = _thrusterMax > 0f ? (_thrusterCharge / _thrusterMax) : 0f;
            _uiManager.UpdateThruster(normalized, _thrusterCoolingDown);
        }
    }

    void FaireLaser()
    {
        _canFire = Time.time + _fireRate;
        if (_isSpreadShotActive)
        {
            FireSpreadShot();
        }
        else if (_isTripleShotActive)
        {
            Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(_laserPrefab, transform.position + new Vector3(0, 0.8f, 0), Quaternion.identity);
        }
        AudioSource.PlayClipAtPoint(_laserSoundClip, Camera.main.transform.position);
    }

    void FireSpreadShot()
    {
        // Fire 5 lasers in a spread pattern: -30, -15, 0, 15, 30 degrees
        float[] angles = { -30f, -15f, 0f, 15f, 30f };
        
        foreach (float angle in angles)
        {
            GameObject laser = Instantiate(_laserPrefab, transform.position + new Vector3(0, 0.8f, 0), Quaternion.identity);
            // Rotate the laser to fire at an angle
            laser.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Damage()
    {
        if (_isShieldActive)
        {
            _shieldStrength = Mathf.Max(0, _shieldStrength - 1);
            if (_shieldStrength <= 0)
            {
                _isShieldActive = false;
                if (_shieldVisualizer != null)
                {
                    _shieldVisualizer.SetActive(false);
                }
            }
            else
            {
                UpdateShieldVisual();
            }
            // Camera shake even with shield
            if (_cameraShake != null)
            {
                _cameraShake.Shake(0.2f, 0.15f);
            }
            return;
        }
        
        _lives--;
        
        // Trigger camera shake on damage
        if (_cameraShake != null)
        {
            _cameraShake.Shake(0.3f, 0.25f);
        }
        
        if (_lives == 2)
        {
            _rightEngine.SetActive(true);
        }
        else if (_lives == 1)
        {
            _leftEngine.SetActive(true);
        }
        _uiManager.UpdateLives(_lives);

        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();
            Destroy(this.gameObject);
        }
    }

    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());

    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isTripleShotActive = false;
    }

    public void SpreadShotActive()
    {
        _isSpreadShotActive = true;
        StartCoroutine(SpreadShotPowerDownRoutine());
    }

    IEnumerator SpreadShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isSpreadShotActive = false;
    }

    public void SpeedBoostActive()
    {
        _speed *= _speedMultiplier;
        StartCoroutine(SpeedBoostPowerDown());

    }

    IEnumerator SpeedBoostPowerDown()
    {
        yield return new WaitForSeconds(5.0f);
        _speed /= _speedMultiplier;
    }

    public void ShieldActive()
    {
        _isShieldActive = true;
        _shieldStrength = _shieldMaxStrength;
        if (_shieldVisualizer != null)
        {
            _shieldVisualizer.SetActive(true);
        }
        UpdateShieldVisual();
    }

    public void AmmoRefill()
    {
        _currentAmmo = _maxAmmo;
        if (_uiManager != null)
        {
            _uiManager.UpdateAmmo(_currentAmmo, _maxAmmo);
        }
    }

    public void Heal()
    {
        if (_lives < 3)
        {
            _lives++;
            _uiManager.UpdateLives(_lives);
            
            // Update engine visuals based on health
            if (_lives == 3)
            {
                _rightEngine.SetActive(false);
                _leftEngine.SetActive(false);
            }
            else if (_lives == 2)
            {
                _leftEngine.SetActive(false);
            }
        }
    }

    // method to add 10 to score!
    // communicate with the ui to update score
    public void AddScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }

    void UpdateShieldVisual()
    {
        if (_shieldVisualizer == null)
        {
            return;
        }
        // Use only the discrete color array for visualization
        if (_shieldStrengthColors == null || _shieldStrengthColors.Length == 0)
        {
            return;
        }
        int hitsTaken = Mathf.Clamp(_shieldMaxStrength - Mathf.Clamp(_shieldStrength, 0, _shieldMaxStrength), 0, int.MaxValue);
        int idx = Mathf.Clamp(hitsTaken, 0, _shieldStrengthColors.Length - 1);
        Color c = _shieldStrengthColors[idx];
        var sr = _shieldVisualizer.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = c;
            return;
        }
        var r = _shieldVisualizer.GetComponent<Renderer>();
        if (r != null && r.material != null)
        {
            r.material.color = c;
        }
    }

    public void StartGame()
    {
        _gameStarted = true;
        _currentAmmo = _maxAmmo;
        if (_uiManager != null)
        {
            _uiManager.UpdateAmmo(_currentAmmo, _maxAmmo);
        }
    }
}
