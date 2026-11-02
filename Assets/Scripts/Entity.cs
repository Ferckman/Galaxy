using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats Base")]
    [SerializeField] protected int _health = 100;
    [SerializeField] protected int _damage = 50;
    [SerializeField] protected int _score = 20;
    [SerializeField] protected GameObject _explosionPrefab;
    [SerializeField] protected bool _shouldDestroyOnPlayerHit = true;

    protected GameManager _gameManager;
    protected UIManager _uiManager;

    protected virtual bool ShouldDestroyOnPlayerHit => _shouldDestroyOnPlayerHit;

    protected virtual void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    // Método virtual para que cada hijo lo mueva como quiera (el asteroide baja recto, el enemigo puede hacer otra cosa)
    protected virtual void Update()
    {
        Movement();
        CheckBounds();
    }

    protected abstract void Movement(); // Obliga a los hijos a implementar su propio movimiento

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
        SpawnExplosion();

        if (_gameManager != null) _gameManager.DestroyConcreteElement(gameObject);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        enabled = false;

        Destroy(gameObject);
    }

    protected virtual void SpawnExplosion()
    {
        if (_explosionPrefab != null) 
        {
            // Añadimos un pequeño desfase aleatorio en el eje Z (-0.1f a -0.5f) 
            // para que una explosión siempre esté matemáticamente "delante" de la otra.
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, Random.Range(-0.1f, -0.5f));
            
            GameObject explosion = Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);

            // OPCIONAL EXTRA: Si tu explosión usa un SpriteRenderer, puedes forzar 
            // un Order in Layer dinámico para evitar parpadeos de orden.
            if (explosion.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) sr.sortingOrder = 10; // Ponlo por encima de los enemigos normales
            DropLoot();
        }
    }

    protected abstract void DropLoot();

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
            if (other.TryGetComponent<Shots>(out var shot))
            {
                _health -= shot.Damage;
                if (_health <= 0)
                {
                    DestroyMyself();
                    IncreaseScore();
                }
            }

            Destroy(other.gameObject);
        }
        else if (other.TryGetComponent<Kamikaze>(out var kamikaze))
        {
            //DestroyMyself();
        }
    }
}