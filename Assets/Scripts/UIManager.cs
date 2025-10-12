using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Text _scoreText;

    [SerializeField]
    private Image _livesImage;

    [SerializeField]
    private Sprite[] _livesSprites;

    [SerializeField]
    private Text _gameOverText;

    [SerializeField]
    private Text _restartText;

    private GameManager _gameManager;

    [SerializeField]
    private Image _thrusterFill;
    [SerializeField]
    private Color _thrusterNormalColor = new Color(0f, 0.8f, 1f);
    [SerializeField]
    private Color _thrusterCooldownColor = Color.red;


    [SerializeField]
    private Text _ammoText;
    [SerializeField]
    private Color _ammoNormalColor = Color.white;
    [SerializeField]
    private Color _ammoEmptyColor = Color.red;

    [SerializeField]
    private Text _waveText;

    private Coroutine _noAmmoRoutine;

    void Start()
    {
        _scoreText.text = "Score: " + 0;
        _gameOverText.gameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if (_thrusterFill != null)
        {
            _thrusterFill.fillAmount = 1f;
            _thrusterFill.color = _thrusterNormalColor;
        }

        if (_gameManager == null)
        {
            Debug.LogError("GameManager is NULL!");
        }
        if (_ammoText != null)
        {
            _ammoText.text = "Ammo: 15/15";
            _ammoText.color = _ammoNormalColor;
        }
    }

    void Update()
    {

    }

    public void UpdateScore(int playerScore)
    {
        _scoreText.text = "Score: " + playerScore;
    }
    public void UpdateLives(int currentLives)
    {
        _livesImage.sprite = _livesSprites[currentLives];
        if (currentLives == 0)
        {
            GameOverSequence();
        }
    }

    void GameOverSequence()
    {
        _gameManager.GameOver();
        _restartText.gameObject.SetActive(true);
        _gameOverText.gameObject.SetActive(true);
        StartCoroutine(GameOverFlickerRoutine());
    }
    IEnumerator GameOverFlickerRoutine()
    {
        while (true)
        {
            _gameOverText.text = "Game Over";
            yield return new WaitForSeconds(0.5f);
            _gameOverText.text = "";
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpdateThruster(float normalized, bool cooling)
    {
        if (_thrusterFill == null)
        {
            return;
        }
        _thrusterFill.fillAmount = Mathf.Clamp01(normalized);
        _thrusterFill.color = cooling ? _thrusterCooldownColor : _thrusterNormalColor;
    }

    public void UpdateAmmo(int current, int max)
    {
        if (_ammoText == null)
        {
            return;
        }
        _ammoText.text = $"Ammo: {current}/{max}";
        _ammoText.color = current <= 0 ? _ammoEmptyColor : _ammoNormalColor;
    }

    public void NotifyNoAmmo()
    {
        if (_ammoText == null)
        {
            return;
        }
        if (_noAmmoRoutine != null)
        {
            StopCoroutine(_noAmmoRoutine);
        }
        _noAmmoRoutine = StartCoroutine(NoAmmoFlashRoutine());
    }

    private IEnumerator NoAmmoFlashRoutine()
    {
        // Briefly flicker the ammo text to draw attention
        for (int i = 0; i < 2; i++)
        {
            _ammoText.enabled = false;
            yield return new WaitForSeconds(0.1f);
            _ammoText.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        _noAmmoRoutine = null;
    }

    public void UpdateWave(int waveNumber)
    {
        if (_waveText != null)
        {
            _waveText.text = "WAVE " + waveNumber;
        }
    }
}
