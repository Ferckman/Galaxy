using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("References")]
    [SerializeField] private UIManager _uiManager;

    [Header("Systems")]
    [SerializeField] private WaveManager _waveManager;

    public bool _isGameStarted { get; private set; }

    public void SetGameStarted(bool status)
    {
        _isGameStarted = status;
    }

    private void Update()
    {
        if (!_isGameStarted && Input.GetKeyDown(KeyCode.Space)) StartGame();
    }

    private void StartGame()
    {
        _isGameStarted = true;

        Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);

        if (_uiManager != null)
        {
            _uiManager.HideMainMenu();
            _uiManager.UpdateLives(3);
            _uiManager.ResetScore();
        }

        // 3. ¡Disparamos las oleadas!
        if (_waveManager != null)
        {
            _waveManager.StartWaves();
        }
    }
}