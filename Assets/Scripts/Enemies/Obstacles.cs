using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacles : MonoBehaviour
{
    [SerializeField] private protected int _health;
    [SerializeField] private protected int _damage;
    [SerializeField] private protected float _speed;
    [SerializeField] private protected int _score;

    [SerializeField] private protected GameObject _explosionPrefab;

    private UIManager _uiManager;
    private protected GameManager _gameManager;

    protected virtual bool ShouldDestroyOnPlayerHit => true;

    protected virtual void Awake()
    {
        _uiManager = Object.FindFirstObjectByType<UIManager>();
        _gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        Movement();
        CheckBounds();
    }

    protected virtual void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    protected virtual void CheckBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float screenHeight = cam.orthographicSize;
        float screenWidth = cam.orthographicSize * cam.aspect;

        float margin = 5.0f; 

        if (transform.position.y < (-screenHeight - margin) || 
            transform.position.y > (screenHeight + margin) || 
            transform.position.x < (-screenWidth - margin) || 
            transform.position.x > (screenWidth + margin))
        {
            if (_gameManager != null) _gameManager.DestroyConcreteElement(gameObject);

            Destroy(gameObject);
        }
    }

    public virtual void IncreaseScore()
    {
        if (_uiManager != null) _uiManager.UpdateScore(_score);
    }

    public virtual void DestroyMyself()
    {
        //IncreaseScore();
        
        SpawnExplosion();

        if (_gameManager != null) _gameManager.DestroyConcreteElement(gameObject);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        enabled = false;

        float explosionDuration = 1.0f;
        Destroy(gameObject, explosionDuration);
    }

    protected virtual void SpawnExplosion()
    {
        if (_explosionPrefab != null)  Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (!player.IsDisabled)
                {
                    player.GetDamage(_damage);
                    if (ShouldDestroyOnPlayerHit)
                    {
                        DestroyMyself();
                        IncreaseScore();
                    }
                }
            }
        }

        else if (other.CompareTag("Shots"))
        {
            Debug.Log("me dio");
            if (other.TryGetComponent<Shots>(out var shot))
            {
                Debug.Log("el shot");
                _health -= shot.Damage;
                if (_health <= 0)
                {
                    DestroyMyself();
                    IncreaseScore();
                }
            }

            Destroy(other.gameObject);
        }

        else if (other.TryGetComponent<Obstacles>(out var otherObstacle))
        {
            DestroyMyself();
        }
    }
}