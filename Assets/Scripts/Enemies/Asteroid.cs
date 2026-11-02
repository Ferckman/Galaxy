/*using UnityEngine;

public class Asteroid : Obstacles
{
    private Vector3 _customDirection;

    protected override void Awake()
    {
        base.Awake();

        Vector3 targetCenter = new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, 2f), 0f);
        _customDirection = (targetCenter - transform.position).normalized;
    }

    public void SetAsteroidProperties(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);

        _speed = _speed * (1f / scale);

        _health = Mathf.RoundToInt(scale * 125f);

        _damage = Mathf.RoundToInt(scale * 60f);

        _score = Mathf.RoundToInt(scale * 20f);
    }

    protected override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * _customDirection);
    }
}
*/
/*
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    private int _health;
    private int _damage;
    private int _score;

    public void Initialize(float scale)
    {
        SetAsteroidProperties(scale);
    }

    public void SetAsteroidProperties(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        _speed = _speed * (1f / scale);
        _health = Mathf.RoundToInt(scale * 125f);
        _damage = Mathf.RoundToInt(scale * 60f);
        _score = Mathf.RoundToInt(scale * 20f);
    }

    private void Update()
    {
        // Los asteroides caen rectos hacia abajo
        transform.Translate(Vector3.down * _speed * Time.deltaTime, Space.World);

        // Destruir si sale por debajo de la pantalla
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            // Aquí puedes sumar puntos (_score) y soltar efectos/partículas
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Si choca con el jugador, le hace daño y el asteroide se destruye
            // collision.GetComponent<Player>()?.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
*/
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private bool _shouldDestroyOnPlayerHit = true;

    private int _health;
    private int _damage;
    private int _score;

    private GameManager _gameManager;
    private UIManager _uiManager;

    public bool ShouldDestroyOnPlayerHit => _shouldDestroyOnPlayerHit;

    private void Awake()
    {
        _uiManager = Object.FindFirstObjectByType<UIManager>();
        _gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    // --- NUEVO MÉTODO DE INICIALIZACIÓN ---
    public void Initialize(float scale)
    {
        SetAsteroidProperties(scale);
    }

    public void SetAsteroidProperties(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);

        _speed = _speed * (1f / scale);

        _health = Mathf.RoundToInt(scale * 75f);

        _damage = Mathf.RoundToInt(scale * 60f);

        _score = Mathf.RoundToInt(scale * 20f);
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

        else if (other.TryGetComponent<Obstacles>(out var otherObstacle))
        {
            DestroyMyself();
        }
    }
}