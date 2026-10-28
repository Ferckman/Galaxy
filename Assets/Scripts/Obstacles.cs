using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacles : MonoBehaviour
{
    [SerializeField] private protected int _health = 100;
    [SerializeField] private protected int _damage = 10;
    [SerializeField] private int _speed = 4;
    [SerializeField] private int _score = 10;

    [SerializeField] private GameObject _explosionPrefab;

    private UIManager _uiManager;
    private GameManager _gameManager;

    protected virtual void Awake()
    {
        _uiManager = Object.FindFirstObjectByType<UIManager>();
        _gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    public virtual void IncreaseScore()
    {
        if (_uiManager != null) _uiManager.UpdateScore(_score);
    }

    public virtual void DestroyMyself()
    {
        IncreaseScore();

        if (_explosionPrefab != null) Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        if (_gameManager != null) _gameManager.DestroyConcreteElement(gameObject);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (!player.IsDisabled)
                {
                    if (!player.ShieldActivated)
                    {
                        player.GetDamage(_damage);
                        player.CheckLife();
                    }
                    else
                    {
                        Transform shieldTransform = other.transform.Find("Shield");
                        if (shieldTransform != null)
                        {
                            Shield shield = shieldTransform.GetComponent<Shield>();
                            if (shield != null) shield.WeakenShield();
                        }
                    }
                    DestroyMyself();
                }
            }
        }
        else if (other.CompareTag("Shots"))
        {
            if (other.TryGetComponent<Shots>(out var shot))
            {
                _health -= shot.Damage;
                if (_health <= 0) DestroyMyself();
            }

            Destroy(other.gameObject);
        }
    }
}