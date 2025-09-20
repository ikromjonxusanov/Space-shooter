using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _isGameOver = false;
    public bool IsGameOver { get { return _isGameOver; } }

    private void Update()
    {
        if (_isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(1); // current Game Scene
        }
    }
    public void GameOver()
    {
        _isGameOver = true;
    }


}
