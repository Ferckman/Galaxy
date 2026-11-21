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
        if (other.CompareTag("Shots")) ReturnShot(other.gameObject);
        else if (other.CompareTag("Collectible"))
        {
            if (LootPool.Instance != null) LootPool.Instance.ReturnToPool(other.gameObject);
            else Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Shots")) ReturnShot(other.gameObject);
    }

    private void ReturnShot(GameObject shotObj)
    {
        if (ProjectilePool.Instance != null) ProjectilePool.Instance.ReturnToPool(shotObj);
        else
            Destroy(shotObj);
    }
}