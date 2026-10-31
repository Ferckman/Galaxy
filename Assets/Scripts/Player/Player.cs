using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int _health;
    [SerializeField] private int _speed;
    [SerializeField] private int _numLives;
    [SerializeField] private bool _shieldActivated;
    [SerializeField] private bool _canTripleShoot;
    [SerializeField] private bool _disabled;

    public int Speed 
    { 
        get => _speed; 
        set => _speed = value; 
    }

    public bool ShieldActivated 
    { 
        get => _shieldActivated; 
        set => _shieldActivated = value; 
    }

    public bool CanTripleShoot 
    { 
        get => _canTripleShoot; 
        set => _canTripleShoot = value; 
    }

    public bool IsDisabled 
    { 
        get => _disabled; 
        set => _disabled = value; 
    }

    [Header("Missile Settings")]
    [SerializeField] private float _bigFireRate;
    [SerializeField] private float _canBigFire;
    
    [Header("Power-up Cooldowns")]
    [SerializeField] private float _cooldownTripleShoot;
    [SerializeField] private float _cooldownMoreSpeed;

    [Header("Visuals")]
    [SerializeField] private GameObject[] _engines;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _shieldPrefab;

    [Header("References")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SpriteRenderer[] _thrusterSprites;
    [SerializeField] private PlayerAnimation _playerAnimation;

    private SpriteRenderer _spriteRenderer;
    private MissileSpawner _missileSpawner;
    private LaserSpawner _laserSpawner;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_uiManager == null) _uiManager = Object.FindFirstObjectByType<UIManager>();
        if (_gameManager == null) _gameManager = Object.FindFirstObjectByType<GameManager>();

        _missileSpawner = GetComponent<MissileSpawner>();
        _laserSpawner = GetComponent<LaserSpawner>();
        
        if (_missileSpawner == null) Debug.LogError("Falta el componente MissileSpawner en el Player.", this);

        StartCoroutine(StartGame());
        StartCoroutine(Blink());
    }

    private void Update()
    {
        Movement();
        Limit();
        ShootLaser();
        CheckMeters();
    }

    private void ResetValues()
    {
        _health = 100;
        _speed = 8;
        _shieldActivated = false;
        _canTripleShoot = false;
        _disabled = true;
        _cooldownTripleShoot = 0f;
        _cooldownMoreSpeed = 0f;

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

    private IEnumerator StartGame()
    {
        while (_disabled)
        {
            transform.position = new Vector3(0, -4.0f, 0);

            yield return new WaitForSeconds(4.0f);

            _disabled = false;
            if (_spriteRenderer != null) _spriteRenderer.enabled = true;
            
            foreach (var thruster in _thrusterSprites)
            {
                if (thruster != null) thruster.enabled = true;
            }
        }
    }

    private IEnumerator Blink()
    {
        while (_disabled)
        {
            if (_spriteRenderer != null) _spriteRenderer.enabled = !_spriteRenderer.enabled;
            
            foreach (var thruster in _thrusterSprites)
            {
                if (thruster != null) thruster.enabled = !thruster.enabled;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(4.0f);

        if (_uiManager != null) _uiManager.ShowMainMenu();
        if (_gameManager != null)
        {
            _gameManager.SetGameStarted(false);
            _gameManager.DestroyElements();
        }

        Destroy(gameObject);
    }

    private void Movement()
    {
        if(!_disabled)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);
            transform.Translate(_speed * Time.deltaTime * direction);

            if (_playerAnimation != null) _playerAnimation.UpdateAnimation(horizontalInput);
        }
    }

    private void Limit()
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
    
    private void ShootLaser()
    {
        if (_disabled) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) _laserSpawner?.FireLaser();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1))
        {
            if (Time.time > _canBigFire)
            {
                if (_missileSpawner != null) _missileSpawner.FireMissileSalvo();

                _canBigFire = Time.time + _bigFireRate;
            }
        }
    }

    public void CheckLife()
    {
        if (_health <= 0)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            _numLives--;
            ResetValues();

            if (_uiManager != null) _uiManager.UpdateLives(_numLives);

            if (_numLives > 0)
            {
                StartCoroutine(StartGame());
                StartCoroutine(Blink());
            }
            else
            {
                if (_spriteRenderer != null) _spriteRenderer.enabled = false;
                
                foreach (var thruster in _thrusterSprites)
                {
                    if (thruster != null) thruster.enabled = false;
                }

                StartCoroutine(EndGame());
            }
        }
        else if (_health <= 40)
        {
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
            if (_engines.Length > 0)
            {
                int randomIndex = Random.Range(0, _engines.Length);
                
                _engines[randomIndex].SetActive(true);
                
                if (_thrusterSprites != null && randomIndex < _thrusterSprites.Length && _thrusterSprites[randomIndex] != null) _thrusterSprites[randomIndex].enabled = false;
            }
        }
    }

    private void CheckMeters()
    {
        if (_cooldownTripleShoot <= 0f && _cooldownMoreSpeed <= 0f) return;

        if (_cooldownTripleShoot > 0f)
        {
            _cooldownTripleShoot -= Time.deltaTime;
            if (_cooldownTripleShoot <= 0f)
            {
                _cooldownTripleShoot = 0f;
                _canTripleShoot = false;
            }
        }

        if (_cooldownMoreSpeed > 0f)
        {
            _cooldownMoreSpeed -= Time.deltaTime;
            if (_cooldownMoreSpeed <= 0f)
            {
                _cooldownMoreSpeed = 0f;
                _speed = 8;
            }
        }

        _numLives = Mathf.Clamp(_numLives, 0, 3);
    }

    public void TripleShotPowerupOn()
    {
        if (_laserSpawner != null) _laserSpawner.SetTripleShoot(true);
    }

    public void SpeedBoostPowerupOn()
    {
        _cooldownMoreSpeed = 10.0f;
    }

    public void ShieldPowerupOn()
    {
        Transform shieldTransform = transform.Find("Shield");

        if (shieldTransform == null)
        {
            GameObject shield = Instantiate(_shieldPrefab, transform.position, Quaternion.identity);
            shield.name = "Shield";
            shield.transform.parent = transform;
        }
        else
        {
            Shield shieldComponent = shieldTransform.GetComponent<Shield>();
            if (shieldComponent != null) shieldComponent.SetTimeLife(20.0f);

            SpriteRenderer shieldSprite = shieldTransform.GetComponent<SpriteRenderer>();
            if (shieldSprite != null) shieldSprite.enabled = true;
        }
    }

    public void GetDamage(int damage)
    {
        if (_disabled) return;

        if (_shieldActivated)
        {
            Debug.Log("¡El escudo ha absorbido el impacto!");
            
            Transform shieldTransform = transform.Find("Shield");
            if (shieldTransform != null)
            {
                if (shieldTransform.TryGetComponent<Shield>(out var shield)) shield.WeakenShield();
            }
        }
        else
        {
            _health -= damage;
            CheckLife();
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
                    Debug.Log($"El jugador recibió {shot.Damage} de daño. Vida restante: {_health}");
                    GetDamage(shot.Damage);
                }

                Destroy(other.gameObject);
            }
        }
    }
}