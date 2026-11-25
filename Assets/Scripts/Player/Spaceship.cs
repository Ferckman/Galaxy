using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class Spaceship : MonoBehaviour
{
    [Header("Spaceship Stats")]
    [SerializeField] protected int _health;
    [SerializeField] protected int _speed;
    [SerializeField] private bool _shieldActivated;
    private bool _disabled = true;
    private bool _invincibility = false;

    private int _initialHealth;
    private int _initialSpeed;

    [Header("Visuals & Effects")]
    [SerializeField] private GameObject[] _engines;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private PlayerAnimation _playerAnimation;

    private SpriteRenderer _spriteRenderer;

    // Propiedad para que PlayerController pueda encenderlo/apagarlo
    public bool ShieldActivated 
    { 
        get => _shieldActivated; 
        set => _shieldActivated = value; 
    }

    public bool InvincibilityActivated 
    { 
        get => _invincibility; 
        set => _invincibility = value; 
    }

    // Propiedades públicas para que el PlayerController o los Power-ups puedan consultarlas/modificarlas
    public int Speed 
    { 
        get => _speed; 
        set => _speed = value; 
    }

    public bool IsDisabled 
    { 
        get => _disabled; 
        set => _disabled = value; 
    }

    public int Health 
    { 
        get => _health; 
        set => _health = value; 
    }

    // Evento para avisar al PlayerController cuando la nave explota (vida llega a 0)
    public event Action OnSpaceshipDestroyed;

    private Camera _mainCamera;

    protected virtual void Awake()
    {
        _mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialHealth = _health;
        _initialSpeed = _speed;

        StartRespawnSequence();
    }

    protected virtual void Update()
    {
        Movement();
        Limit();
    }

    protected virtual void Movement()
    {
        if (!_disabled)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);
            transform.Translate(_speed * Time.deltaTime * direction);

            if (_playerAnimation != null) 
            {
                _playerAnimation.UpdateAnimation(horizontalInput);
            }
        }
    }

    protected virtual void Limit()
    {
        Camera cam = _mainCamera != null ? _mainCamera : Camera.main;
        if (cam == null) return;

        Vector3 pos = transform.position;

        float verticalLimit = cam.orthographicSize;
        float horizontalLimit = verticalLimit * cam.aspect;

        float marginX = 2.5f;
        float bottomMargin = 4.0f;
        float topMargin = 4.0f;

        pos.y = Mathf.Clamp(pos.y, -verticalLimit + bottomMargin, verticalLimit - topMargin);
        pos.x = Mathf.Clamp(pos.x, -horizontalLimit + marginX, horizontalLimit - marginX);

        transform.position = pos;
    }

    public virtual void GetDamage(int damage)
    {
        if (_disabled) return;
        if (_invincibility) return;

        if (_shieldActivated)
        {
            Debug.Log("¡El escudo ha absorbido el impacto!");
            
            Transform shieldTransform = transform.Find("Shield");
            if (shieldTransform != null)
            {
                if (shieldTransform.TryGetComponent<Shield>(out var shield)) 
                {
                    shield.WeakenShield();
                }
            }
        }
        else
        {
            _health -= damage;
            CheckHealthStatus(); // (Tu antiguo CheckLife renombrado)
        }
    }

    protected virtual void CheckHealthStatus()
    {
        if (_health <= 0)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            // Terremoto al morir
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.4f, 0.3f);

            // Avisamos al PlayerController de que hemos muerto
            OnSpaceshipDestroyed?.Invoke();
        }
        else if (_health <= (_initialHealth * 0.4f)) // 40% de vida
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.25f);

            if (_engines.Length >= 2)
            {
                _engines[0].SetActive(true);
                _engines[1].SetActive(true);
            }
        }
        else if (_health <= (_initialHealth * 0.6f)) // 60% de vida
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.15f);

            if (_engines.Length > 0)
            {
                int randomIndex = Random.Range(0, _engines.Length);
                _engines[randomIndex].SetActive(true);
            }
        }
    }

    public void ResetSpaceshipState()
    {
        _health = _initialHealth;
        _speed = _initialSpeed;
        _shieldActivated = false;
        _disabled = true;
        _invincibility = false;

        if (_engines != null)
        {
            foreach (var engine in _engines)
            {
                if (engine != null) engine.SetActive(false);
            }
        }
    }

    public void HideSpaceshipVisuals()
    {
        // Ocultamos todos los SpriteRenderer de la nave y sus hijos
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in allRenderers)
        {
            if (renderer != null) renderer.enabled = false;
        }

        // Por si acaso quedan motores rotos encendidos, los apagamos también
        if (_engines != null)
        {
            foreach (var engine in _engines)
            {
                if (engine != null) engine.SetActive(false);
            }
        }
    }

    // Método público para que PlayerController u otros le ordenen iniciar la reaparición
    public void StartRespawnSequence()
    {
        StartCoroutine(StartGameRoutine());
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        while (_disabled)
        {
            transform.position = new Vector3(0, -10.0f, 0);

            yield return new WaitForSeconds(4.0f);

            _disabled = false;
            
            // Reactivamos todos los SpriteRenderer al terminar la reaparición
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in allRenderers)
            {
                if (renderer != null) renderer.enabled = true;
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        // Obtenemos el de la nave y los de todos sus hijos (como el propulsor)
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        bool state = true;

        while (_disabled)
        {
            state = !state;
            foreach (var renderer in allRenderers)
            {
                if (renderer != null) renderer.enabled = state;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_disabled)
        {
            if (other.CompareTag("Shots")) 
            {
                if (other.TryGetComponent<Shots>(out var shot))
                {
                    // Obtenemos el daño real calculado (con mínimo, máximo y crítico)
                    int finalDamage = shot.GetCalculatedDamage(out bool isCrit);

                    if (isCrit)
                        Debug.Log($"<color=red>¡CRÍTICOS CONTRA TI! -{finalDamage}</color>");
                    else
                        Debug.Log($"La nave recibió {finalDamage} de daño. Vida restante: {_health}");

                    GetDamage(finalDamage);
                }

                if (ProjectilePool.Instance != null)
                    ProjectilePool.Instance.ReturnToPool(other.gameObject);
                else
                    Destroy(other.gameObject);
            }
        }
    }
}