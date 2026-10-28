using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Sprite[] _lives;
    [SerializeField] private Image _livesImage;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Image _mainMenu;
    
    private int _score;

    private void ShowScore()
    {
        _scoreText.text = $"Score: {_score}";
    }

    public void UpdateLives(int currentLives)
    {
        _livesImage.sprite = _lives[currentLives];
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