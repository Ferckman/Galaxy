using UnityEngine;
using System.Collections.Generic;

public class FormationController : MonoBehaviour
{
    [Header("Movement Settings")]
    private float _moveSpeed = 3f;      
    private float _maxXLimit = 7f;      
    private int _movementDirection = 1; // 1 = Derecha, -1 = Izquierda

    [Header("Advanced Patterns (Opcional)")]
    private bool _useSineWave = false;      
    private float _sineFrequency = 2f;      
    private float _sineAmplitude = 0.5f;    
    private float _initialYPosition;        
    private int _totalInitialEnemies = 0;   

    private bool _isBlockedActive = false;
    private List<Enemy> _myEnemies = new List<Enemy>();

    public void Initialize(List<Enemy> enemies, float speed, float limit, bool useSineWave = false)
    {
        _myEnemies = enemies;
        _moveSpeed = speed;
        _maxXLimit = limit;
        _useSineWave = useSineWave;
        _initialYPosition = transform.position.y;
        _totalInitialEnemies = enemies.Count;
        _isBlockedActive = true;

        UpdateEnemiesDirection(_movementDirection);
    }

    private void Update()
    {
        if (!_isBlockedActive) return;

        _myEnemies.RemoveAll(e => e == null);

        if (_myEnemies.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // --- EFECTO "BREATHING" ---
        float currentSpeedMultiplier = 3f;
        if (_totalInitialEnemies > 0)
        {
            float percentageAlive = (float)_myEnemies.Count / _totalInitialEnemies;
            currentSpeedMultiplier = Mathf.Lerp(1.8f, 1.0f, percentageAlive);
        }

        // --- MOVIMIENTO LATERAL ---
        float moveDelta = _movementDirection * (_moveSpeed * currentSpeedMultiplier) * Time.deltaTime;
        transform.Translate(Vector3.right * moveDelta, Space.World);

        // --- MOVIMIENTO SINOIDAL ---
        if (_useSineWave)
        {
            float newY = _initialYPosition + Mathf.Sin(Time.time * _sineFrequency) * _sineAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // --- COMPROBACIÓN DE BORDES Y REBOTE ---
        Camera cam = Camera.main;
        if (cam != null)
        {
            float screenWidth = cam.orthographicSize * cam.aspect;
            float margin = 1.0f;

            foreach (Enemy enemy in _myEnemies)
            {
                if (enemy == null) continue;

                if (enemy.transform.position.x > screenWidth - margin && _movementDirection > 0)
                {
                    ChangeDirection(-1);
                    break;
                }
                else if (enemy.transform.position.x < -screenWidth + margin && _movementDirection < 0)
                {
                    ChangeDirection(1);
                    break;
                }
            }
        }
    }

    private void ChangeDirection(int newDirection)
    {
        _movementDirection = newDirection;
        UpdateEnemiesDirection(_movementDirection);
        DropFormation();
    }

    private void DropFormation()
    {
        transform.position += Vector3.down * 0.3f;
        _initialYPosition = transform.position.y;
    }

    private void UpdateEnemiesDirection(int direction)
    {
        foreach (Enemy enemy in _myEnemies)
        {
            if (enemy != null)
            {
                enemy.SetFacingDirection(direction);
            }
        }
    }
}