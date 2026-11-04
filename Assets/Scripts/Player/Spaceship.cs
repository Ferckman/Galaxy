using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class Spaceship : MonoBehaviour
{
    [Header("Spaceship Stats")]
    [SerializeField] protected int _health = 200;
    [SerializeField] protected int _speed = 8;
    [SerializeField] protected bool _disabled = false;
    [SerializeField] private bool _shieldActivated;

    // Propiedad para que PlayerController pueda encenderlo/apagarlo
    public bool ShieldActivated 
    { 
        get => _shieldActivated; 
        set => _shieldActivated = value; 
    }

    [Header("Visuals & Effects")]
    [SerializeField] private GameObject[] _engines;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private SpriteRenderer[] _thrusterSprites;
    [SerializeField] private PlayerAnimation _playerAnimation;

    private SpriteRenderer _spriteRenderer;

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

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Al arrancar el juego por primera vez, la nave inicia su secuencia de aparición
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
        Vector3 pos = transform.position;

        float verticalLimit = Camera.main.orthographicSize;
        float horizontalLimit = verticalLimit * Camera.main.aspect;

        float marginX = 0.6f; 
        float bottomMargin = 1.2f;
        float topMargin = 1.0f;

        pos.y = Mathf.Clamp(pos.y, -verticalLimit + bottomMargin, verticalLimit - topMargin);
        pos.x = Mathf.Clamp(pos.x, -horizontalLimit + marginX, horizontalLimit - marginX);

        transform.position = pos;
    }

    public virtual void GetDamage(int damage)
    {
        if (_disabled) return;

        // ¡RECUPERAMOS LA LÓGICA DEL ESCUDO!
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
        else if (_health <= 40)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.25f);

            if (_engines.Length >= 2)
            {
                _engines[0].SetActive(true);
                _engines[1].SetActive(true);

                if (_thrusterSprites != null)
                {
                    if (_thrusterSprites.Length > 0 && _thrusterSprites[0] != null) 
                        _thrusterSprites[0].enabled = false;
                    if (_thrusterSprites.Length > 1 && _thrusterSprites[1] != null) 
                        _thrusterSprites[1].enabled = false;
                }
            }
        }
        else if (_health <= 60)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.15f);

            if (_engines.Length > 0)
            {
                int randomIndex = Random.Range(0, _engines.Length);
                
                _engines[randomIndex].SetActive(true);
                
                if (_thrusterSprites != null && randomIndex < _thrusterSprites.Length && _thrusterSprites[randomIndex] != null) 
                    _thrusterSprites[randomIndex].enabled = false;
            }
        }
    }

    public void ResetSpaceshipState()
    {
        _health = 200;
        _speed = 8;
        _disabled = true;
        _shieldActivated = false;

        if (_engines.Length >= 2)
        {
            _engines[0].SetActive(false);
            _engines[1].SetActive(false);
        }

        if (_thrusterSprites != null)
        {
            foreach (var thrusterSprite in _thrusterSprites)
            {
                if (thrusterSprite != null) thrusterSprite.enabled = true;
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
            transform.position = new Vector3(0, -4.0f, 0);

            yield return new WaitForSeconds(4.0f);

            _disabled = false;
            if (_spriteRenderer != null) _spriteRenderer.enabled = true;
            
            if (_thrusterSprites != null)
            {
                foreach (var thruster in _thrusterSprites)
                {
                    if (thruster != null) thruster.enabled = true;
                }
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (_disabled)
        {
            if (_spriteRenderer != null) _spriteRenderer.enabled = !_spriteRenderer.enabled;
            
            if (_thrusterSprites != null)
            {
                foreach (var thruster in _thrusterSprites)
                {
                    if (thruster != null) thruster.enabled = !thruster.enabled;
                }
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
                    Debug.Log($"La nave recibió {shot.Damage} de daño. Vida restante: {_health}");
                    GetDamage(shot.Damage);
                }

                Destroy(other.gameObject);
            }
        }
    }
}