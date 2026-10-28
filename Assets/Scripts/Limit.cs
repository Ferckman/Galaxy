using UnityEngine;

public class Limit : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private void Awake()
    {
        if (_gameManager == null) _gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<Enemy>(out var enemy)) enemy.Limit();
        }
        else if (other.CompareTag("Shots"))
        {
            if (other.TryGetComponent<Shots>(out var shot)) Destroy(other.gameObject);
        }
        else if (other.CompareTag("Powerup"))
        {
            if (_gameManager != null) _gameManager.DestroyConcreteElement(other.gameObject);
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Shots")) Destroy(other.gameObject);
    }
}