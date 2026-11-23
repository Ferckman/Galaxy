/*using UnityEngine;
using System.Collections;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats Base")]
    [SerializeField] protected int _health = 100;
    [SerializeField] protected int _damage = 50;
    [SerializeField] protected int _score = 20;
    [SerializeField] protected GameObject _explosionPrefab;
    [SerializeField] protected bool _shouldDestroyOnPlayerHit = true;

    [Header("Target Reticle")]
    [SerializeField] private GameObject _targetReticle;

    protected GameManager _gameManager;
    protected UIManager _uiManager;

    protected virtual bool ShouldDestroyOnPlayerHit => _shouldDestroyOnPlayerHit;

    protected virtual void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    protected virtual void OnDisable()
    {
        SetTargetReticle(false);
    }

    // Dentro de tu clase Entity:
    public virtual void OnSpawnedFromPool()
    {
        // Resetear vida, estados, corrutinas, etc. al reutilizarla
    }

    public virtual void OnReturnedToPool()
    {
        // Limpiar estados o detener acciones al volver al pool
    }

    public void DespawnToPool()
    {
        if (EntityPool.Instance != null) EntityPool.Instance.ReturnToPool(this);
        else gameObject.SetActive(false);
    }

    // Método virtual para que cada hijo lo mueva como quiera
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
            Destroy(gameObject);
        }
    }

    public virtual void IncreaseScore()
    {
        if (_uiManager != null) _uiManager.UpdateScore(_score);
    }

    public virtual void DestroyMyself()
    {
        // 1. Spawneamos la explosión al instante para que aparezca ya mismo
        SpawnExplosion();

        // 2. Desactivamos el colisionador y el script para que deje de disparar o moverse
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        enabled = false;

        // 3. En lugar de destruir al instante, iniciamos una corrutina para mantener 
        // la nave visible unos milisegundos mientras la explosión ya está reproduciéndose encima.
        StartCoroutine(DelayedDestroyRoutine(0.15f)); // Puedes ajustar este tiempo (ej. 0.15s o 0.2s)
    }

    private IEnumerator DelayedDestroyRoutine(float delay)
    {
        // Esperamos el tiempo indicado manteniendo la nave en pantalla junto a la explosión
        yield return new WaitForSeconds(delay);

        // Ahora sí, eliminamos el objeto por completo
        Destroy(gameObject);
    }

    protected virtual void SpawnExplosion()
    {
        if (_explosionPrefab != null)
        {
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, Random.Range(-0.1f, -0.5f));

            GameObject explosion = ExplosionPool.Instance != null
                ? ExplosionPool.Instance.SpawnExplosion(explosionPos, Quaternion.identity)
                : Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);

            if (explosion != null && explosion.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.sortingOrder = 10;
            }

            DropLoot();
        }
    }

    protected abstract void DropLoot();

    public virtual void SetTargetReticle(bool active)
    {
        if (_targetReticle != null && _targetReticle.activeSelf != active)
        {
            _targetReticle.SetActive(active);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spaceship"))
        {
            if (other.TryGetComponent<Spaceship>(out var spaceship))
            {
                if (!spaceship.IsDisabled)
                {
                    spaceship.GetDamage(_damage);
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
                // 1. Obtenemos el daño calculado del disparo del jugador (con min, max y crítico)
                int finalDamage = shot.GetCalculatedDamage(out bool isCrit);

                // 2. Restamos el daño a la vida de la entidad (enemigo, asteroide, etc.)
                _health -= finalDamage;

                if (isCrit)
                    Debug.Log($"<color=orange>¡CRÍTICO AL ENEMIGO! -{finalDamage}</color>");

                // 3. Comprobamos si la entidad ha muerto
                if (_health <= 0)
                {
                    DestroyMyself();
                    IncreaseScore();
                }
            }
            if (!other.TryGetComponent<HyperBeam>(out var hyperBeam))
            {
                if (other.TryGetComponent<Missile>(out var missile)) missile.TerminateMissile();
                else if (ProjectilePool.Instance != null) ProjectilePool.Instance.ReturnToPool(other.gameObject);
                else Destroy(other.gameObject);
            }
        }
    }
}
*/
using System;
using System.Collections;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats Base")]
    [SerializeField] protected int _health = 100;
    [SerializeField] protected int _damage = 50;
    [SerializeField] protected int _score = 20;
    [SerializeField] protected GameObject _explosionPrefab;
    [SerializeField] protected bool _shouldDestroyOnPlayerHit = true;

    [Header("Target Reticle")]
    [SerializeField] private GameObject _targetReticle;

    protected GameManager _gameManager;
    protected UIManager _uiManager;

    private int _maxHealth;
    private Collider2D _collider;
    private bool _hasNotifiedWave;

    public event Action<Entity> OnDespawned;

    protected virtual bool ShouldDestroyOnPlayerHit => _shouldDestroyOnPlayerHit;

    protected virtual void Awake()
    {
        _maxHealth = _health;
        _collider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    protected virtual void OnDisable()
    {
        SetTargetReticle(false);
    }

    public virtual void OnSpawnedFromPool()
    {
        StopAllCoroutines();
        _health = _maxHealth;
        _hasNotifiedWave = false;

        if (_collider != null) _collider.enabled = true;
        enabled = true;
        SetTargetReticle(false);
    }

    public virtual void OnReturnedToPool()
    {
        StopAllCoroutines();
        NotifyWaveOnce();
        OnDespawned = null;
    }

    protected void NotifyWaveOnce()
    {
        if (_hasNotifiedWave) return;
        _hasNotifiedWave = true;
        OnDespawned?.Invoke(this);
    }

    public void DespawnToPool()
    {
        NotifyWaveOnce();

        if (EntityPool.Instance != null)
        {
            EntityPool.Instance.ReturnToPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        Movement();
        CheckBounds();
    }

    protected abstract void Movement();

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
            DespawnToPool();
        }
    }

    public virtual void IncreaseScore()
    {
        if (_uiManager != null) _uiManager.UpdateScore(_score);
    }

    public virtual void DestroyMyself()
    {
        SpawnExplosion();

        if (_collider != null) _collider.enabled = false;
        enabled = false;

        StartCoroutine(DelayedDespawnRoutine(0.15f));
    }

    private IEnumerator DelayedDespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        DespawnToPool();
    }

    protected virtual void SpawnExplosion()
    {
        if (_explosionPrefab != null)
        {
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, UnityEngine.Random.Range(-0.1f, -0.5f));

            GameObject explosion = ExplosionPool.Instance != null
                ? ExplosionPool.Instance.SpawnExplosion(explosionPos, Quaternion.identity)
                : Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);

            if (explosion != null && explosion.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.sortingOrder = 10;
            }

            DropLoot();
        }
    }

    protected abstract void DropLoot();

    public virtual void SetTargetReticle(bool active)
    {
        if (_targetReticle != null && _targetReticle.activeSelf != active)
        {
            _targetReticle.SetActive(active);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spaceship"))
        {
            if (other.TryGetComponent<Spaceship>(out var spaceship))
            {
                if (!spaceship.IsDisabled)
                {
                    spaceship.GetDamage(_damage);
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
                int finalDamage = shot.GetCalculatedDamage(out bool isCrit);
                _health -= finalDamage;

                if (isCrit)
                    Debug.Log($"<color=orange>¡CRÍTICO AL ENEMIGO! -{finalDamage}</color>");

                if (_health <= 0)
                {
                    DestroyMyself();
                    IncreaseScore();
                }
            }

            if (!other.TryGetComponent<HyperBeam>(out var hyperBeam))
            {
                if (other.TryGetComponent<Missile>(out var missile)) missile.TerminateMissile();
                else if (ProjectilePool.Instance != null) ProjectilePool.Instance.ReturnToPool(other.gameObject);
                else Destroy(other.gameObject);
            }
        }
    }
}