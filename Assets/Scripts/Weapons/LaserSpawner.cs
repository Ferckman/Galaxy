using UnityEngine;
using System.Collections.Generic;

public class LaserSpawner : MonoBehaviour
{
    [Header("Prefabs & Audio")]
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private AudioSource _audioLaser;

    [Header("Power-Up Settings")]
    [SerializeField] private bool _canTripleShoot = false;
    [SerializeField] private float _tripleShootDuration = 7f; // Cuánto dura el power-up en segundos
    private float _tripleShootTimer; // Temporizador interno de cuenta atrás

    private Transform _centerSpawnPoint;
    private List<Transform> _sideSpawnPoints = new List<Transform>();

    public bool CanTripleShoot => _canTripleShoot;

    private void Awake()
    {
        // Auto-descubrimiento de los puntos de láser
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;

            if (child.name.Contains("Laser"))
            {
                if (child.name.Contains("Center") || child.name.Contains("Middle") || Mathf.Abs(child.localPosition.x) < 0.1f)
                {
                    _centerSpawnPoint = child;
                }
                else
                {
                    _sideSpawnPoints.Add(child);
                }
            }
        }
    }

    private void Update()
    {
        // Si el power-up está activo, restamos tiempo en cada frame
        if (_canTripleShoot)
        {
            _tripleShootTimer -= Time.deltaTime;

            if (_tripleShootTimer <= 0f)
            {
                _canTripleShoot = false; // Se acabó el tiempo, volvemos al láser normal
            }
        }
    }

    public void FireLaser()
    {
        if (_laserPrefab == null) return;

        Collider2D playerCollider = GetComponentInChildren<Collider2D>();

        // 1. Disparar centro
        if (_centerSpawnPoint != null)
        {
            SpawnSingleLaser(_centerSpawnPoint, playerCollider);
        }

        // 2. Disparar laterales si el power-up sigue activo
        if (_canTripleShoot)
        {
            foreach (Transform sp in _sideSpawnPoints)
            {
                if (sp != null)
                {
                    SpawnSingleLaser(sp, playerCollider);
                }
            }
        }

        if (_audioLaser != null && !_audioLaser.isPlaying)
        {
            _audioLaser.Play();
        }
    }

    private void SpawnSingleLaser(Transform spawnPoint, Collider2D playerCollider)
    {
        GameObject laserInstance = Instantiate(_laserPrefab, spawnPoint.position, spawnPoint.rotation);
        
        if (laserInstance.TryGetComponent<Laser>(out Laser laserScript))
        {
            laserScript.Initialize(playerCollider);
        }
    }

    // Método que llama tu Power-Up (a través del Player)
    public void SetTripleShoot(bool active)
    {
        _canTripleShoot = active;

        if (active)
        {
            // Cada vez que coges el power-up, reiniciamos el contador al máximo
            _tripleShootTimer = _tripleShootDuration;
        }
    }
}