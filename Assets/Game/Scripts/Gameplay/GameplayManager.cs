using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField]
    private InputManager _inputManager;
    [SerializeField]
    private PlayerMovement _playerMovement;
    [SerializeField]
    private TextMeshProUGUI _scoreText;
    [SerializeField]
    private TextMeshProUGUI _highScoreText;

    private void Start() {
        _inputManager.OnMainMenuInput += BackToMainMenu;
        _playerMovement.OnGameOver += GameOver;
        _playerMovement.OnYouWin += YouWin;
    }

    private void OnDestroy() {
        _inputManager.OnMainMenuInput -= BackToMainMenu;
        _playerMovement.OnGameOver -= GameOver;
        _playerMovement.OnYouWin -= YouWin;
    }

    private void BackToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    private void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        string dataScoreToKeep = _scoreText.text;
        StaticData.scoreValueToKeep = dataScoreToKeep;
        string dataHighScoreToKeep = _highScoreText.text;
        StaticData.highScoreValueToKeep = dataHighScoreToKeep;
        SceneManager.LoadScene("GameOver");
    }

    private void YouWin()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        string dataScoreToKeep = _scoreText.text;
        StaticData.scoreValueToKeep = dataScoreToKeep;
        string dataHighScoreToKeep = _highScoreText.text;
        StaticData.highScoreValueToKeep = dataHighScoreToKeep;
        SceneManager.LoadScene("YouWin");
    }
}
