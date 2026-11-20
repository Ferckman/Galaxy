using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int _numLives = 3;
    [SerializeField] private bool _canTripleShoot;

    [Header("Power-up Cooldowns")]
    [SerializeField] private float _cooldownTripleShoot;
    [SerializeField] private float _cooldownMoreSpeed;

    [Header("Visuals & Prefabs")]
    [SerializeField] private GameObject _shieldPrefab;

    [Header("References")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameManager _gameManager;

    private Spaceship _spaceship;
    private MissileSpawner _missileSpawner;
    private LaserSpawner _laserSpawner;
    private HyperBeamController _hyperBeamController;
    private SpriteRenderer _spriteRenderer;

    public bool CanTripleShoot 
    { 
        get => _canTripleShoot; 
        set => _canTripleShoot = value; 
    }

    private void Awake()
    {
        // Obtenemos las referencias de los componentes que están en el mismo GameObject
        _spaceship = GetComponent<Spaceship>();
        _missileSpawner = GetComponent<MissileSpawner>();
        _laserSpawner = GetComponent<LaserSpawner>();
        _hyperBeamController = GetComponent<HyperBeamController>();

        // Buscamos los gestores globales si no están asignados en el Inspector
        if (_uiManager == null) _uiManager = Object.FindFirstObjectByType<UIManager>();
        if (_gameManager == null) _gameManager = Object.FindFirstObjectByType<GameManager>();

        if (_missileSpawner == null) Debug.LogError("Falta el componente MissileSpawner en el Player.", this);
        if (_spaceship == null) Debug.LogError("Falta el componente Spaceship en el Player.", this);

        // Nos suscribimos al evento de destrucción que emite la nave física (Spaceship)
        if (_spaceship != null)
        {
            _spaceship.OnSpaceshipDestroyed += HandleSpaceshipDestroyed;
        }
    }

    private void OnDestroy()
    {
        // Buena práctica: desuscribirse del evento al destruir el objeto
        if (_spaceship != null)
        {
            _spaceship.OnSpaceshipDestroyed -= HandleSpaceshipDestroyed;
        }
    }

    private void Update()
    {
        ShootLaser();
        CheckMeters();
    }

    private void HandleSpaceshipDestroyed()
    {
        _numLives--;
        _spaceship.ResetSpaceshipState();
        ResetPowerUps(); // <-- Limpia cooldowns y estados activos

        if (_uiManager != null) _uiManager.UpdateLives(_numLives);

        if (_numLives > 0)
        {
            _spaceship.StartRespawnSequence();
        }
        else
        {
            if (_spaceship != null)
            {
                _spaceship.HideSpaceshipVisuals();
            }
            StartCoroutine(EndGameRoutine());
        }
    }

    private void ResetPowerUps()
    {
        _cooldownMoreSpeed = 0f;
        _cooldownTripleShoot = 0f;
        _canTripleShoot = false;

        if (_laserSpawner != null)
        {
            _laserSpawner.SetTripleShoot(false);
        }
    }

    private IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(4.0f);

        if (_uiManager != null) _uiManager.ShowMainMenu();
        if (_gameManager != null)
        {
            _gameManager.SetGameStarted(false);
        }

        Destroy(gameObject);
    }

    private void ShootLaser()
    {
        if (_spaceship != null && _spaceship.IsDisabled) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) _laserSpawner?.FireLaser();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1))
        {
            if (_missileSpawner != null) _missileSpawner.TryFireSalvo();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _hyperBeamController?.TryFireHyperBeam();
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
                if (_laserSpawner != null)
                {
                    _laserSpawner.SetTripleShoot(false);
                }
            }
        }

        if (_cooldownMoreSpeed > 0f)
        {
            _cooldownMoreSpeed -= Time.deltaTime;
            if (_cooldownMoreSpeed <= 0f)
            {
                _cooldownMoreSpeed = 0f;
                if (_spaceship != null) 
                {
                    _spaceship.Speed -= 10; // Restaura exactamente los 10 que sumamos al principio
                    Debug.Log("Velocidad normalizada");
                }
            }
        }

        _numLives = Mathf.Clamp(_numLives, 0, 3);
    }

    private void _cooldownTripleShieldTimer() { }

    public void TripleShotPowerupOn()
    {
        if (_laserSpawner != null) _laserSpawner.SetTripleShoot(true);
    }

    public void SpeedBoostPowerupOn(int speedBoostDuration)
    {
        if (_spaceship == null) return;

        // Si el power-up YA estaba activo, NO sumamos velocidad de nuevo, 
        // simplemente extendemos/reiniciamos el tiempo.
        if (_cooldownMoreSpeed <= 0f)
        {
            _spaceship.Speed += 10; // Solo sumamos si partíamos de velocidad normal
        }

        // Reiniciamos el temporizador al máximo valor (sea nuevo o ya estuviera activo)
        _cooldownMoreSpeed = speedBoostDuration;
    }

    public void ShieldPowerupOn()
    {
        _spaceship.ShieldActivated = true;
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

    public void Invincibility(bool value)
    {
        _spaceship.InvincibilityActivated = value;
    }
}