using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _lifeIcons;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Image _mainMenu;
    
    private int _score;

    private void ShowScore()
    {
        _scoreText.text = $"Score: {_score}";
    }

    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < _lifeIcons.Length; i++)
        {
            if (_lifeIcons[i] != null) _lifeIcons[i].SetActive(i < currentLives);
        }
    }

    public void UpdateScore(int score)
    {
        _score += score;
        ShowScore();
    }

    public void ResetScore()
    {
        _score = 0;
        ShowScore();
    }

    public void HideMainMenu()
    {
        _mainMenu.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    public void ShowMainMenu()
    {
        _mainMenu.gameObject.SetActive(true);
        Cursor.visible = true;
    }
}