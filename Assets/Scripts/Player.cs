using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int _health = 100;
    [SerializeField] private int _speed = 8;
    [SerializeField] private int _numLives = 3;
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

    [Header("Power-up Cooldowns")]
    [SerializeField] private float _cooldownTripleShoot;
    [SerializeField] private float _cooldownMoreSpeed;

    [Header("Visuals & Prefabs")]
    [SerializeField] private GameObject[] _engines;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _shieldPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioLaser;
    [SerializeField] private AudioSource _audioMissile;

    [Header("References")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SpriteRenderer _thrusterSprite;
    [SerializeField] private PlayerAnimation _playerAnimation;

    private float _canFire;
    private float _canBigFire;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_uiManager == null) _uiManager = Object.FindFirstObjectByType<UIManager>();
        if (_gameManager == null) _gameManager = Object.FindFirstObjectByType<GameManager>();

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
    }

    private IEnumerator StartGame()
    {
        while (_disabled)
        {
            transform.position = new Vector3(0, -4.0f, 0);

            yield return new WaitForSeconds(4.0f);

            _disabled = false;
            if (_spriteRenderer != null) _spriteRenderer.enabled = true;
            if (_thrusterSprite != null) _thrusterSprite.enabled = true;
        }
    }

    private IEnumerator Blink()
    {
        while (_disabled)
        {
            if (_spriteRenderer != null) _spriteRenderer.enabled = !_spriteRenderer.enabled;
            if (_thrusterSprite != null) _thrusterSprite.enabled = !_thrusterSprite.enabled;

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
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);
        transform.Translate(_speed * Time.deltaTime * direction);

        if (_playerAnimation != null) _playerAnimation.UpdateAnimation(horizontalInput);
    }

    private void Limit()
    {
        Vector3 pos = transform.position;

        if (pos.y > 4.2f) pos.y = 4.2f;
        else if (pos.y < -4.2f) pos.y = -4.2f;

        if (pos.x > 8.2f) pos.x = 8.2f;
        else if (pos.x < -8.2f) pos.x = -8.2f;

        transform.position = pos;
    }

    private void ShootLaser()
    {
        if (_disabled) return;

        float fireRate = 0.25f;
        float bigFireRate = 10.0f;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (Time.time > _canFire)
            {
                if (_canTripleShoot)
                {
                    Instantiate(_laserPrefab, transform.position + new Vector3(-0.56f, 0, 0), Quaternion.identity);
                    Instantiate(_laserPrefab, transform.position + new Vector3(0.56f, 0, 0), Quaternion.identity);
                }

                Instantiate(_laserPrefab, transform.position + new Vector3(0, 0.86f, 0), Quaternion.identity);

                if (_audioLaser != null) _audioLaser.Play();

                _canFire = Time.time + fireRate;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1))
        {
            if (Time.time > _canBigFire)
            {
                Instantiate(_missilePrefab, transform.position + new Vector3(-0.56f, -1.2f, 0), Quaternion.identity);
                Instantiate(_missilePrefab, transform.position + new Vector3(0.14f, 0, 0), Quaternion.identity);
                Instantiate(_missilePrefab, transform.position + new Vector3(-0.14f, 0, 0), Quaternion.identity);
                Instantiate(_missilePrefab, transform.position + new Vector3(0.56f, -1.2f, 0), Quaternion.identity);

                if (_audioMissile != null) _audioMissile.Play();

                _canBigFire = Time.time + bigFireRate;
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
                if (_thrusterSprite != null) _thrusterSprite.enabled = false;

                StartCoroutine(EndGame());
            }
        }
        else if (_health <= 40)
        {
            if (_engines.Length >= 2)
            {
                _engines[0].SetActive(true);
                _engines[1].SetActive(true);
            }
        }
        else if (_health <= 60)
        {
            if (_engines.Length > 0) _engines[Random.Range(0, _engines.Length)].SetActive(true);
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
        _cooldownTripleShoot = 10.0f;
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
        _health -= damage;
        CheckLife();
    }
}