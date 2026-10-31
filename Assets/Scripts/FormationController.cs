using UnityEngine;
using System.Collections.Generic;

public class FormationController : MonoBehaviour
{
    [Header("Movement Settings")]
    private float _moveSpeed = 3f;      
    private float _maxXLimit = 7f;      
    private int _movementDirection = 1; // 1 = Derecha, -1 = Izquierda

    [Header("Advanced Patterns (Opcional)")]
    private bool _useSineWave = false;      // Movimiento oscilante vertical en onda
    private float _sineFrequency = 2f;      // Velocidad de la onda
    private float _sineAmplitude = 0.5f;    // Altura de la onda
    private float _initialYPosition;        // Posición Y de referencia para la onda
    private int _totalInitialEnemies = 0;   // Para calcular el efecto breathing

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
    }

    private void Update()
    {
        if (!_isBlockedActive) return;

        // Limpiamos enemigos muertos
        _myEnemies.RemoveAll(e => e == null);

        if (_myEnemies.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // --- EFECTO "BREATHING" (Aceleración progresiva) ---
        // Cuantos menos enemigos quedan vivos, más rápido se mueve el bloque (clásico de arcade)
        float currentSpeedMultiplier = 3f;
        if (_totalInitialEnemies > 0)
        {
            float percentageAlive = (float)_myEnemies.Count / _totalInitialEnemies;
            // Si queda la mitad o menos, el bloque empieza a acelerar progresivamente hasta duplicar velocidad
            currentSpeedMultiplier = Mathf.Lerp(1.8f, 1.0f, percentageAlive);
        }

        // --- MOVIMIENTO LATERAL ---
        float moveDelta = _movementDirection * (_moveSpeed * currentSpeedMultiplier) * Time.deltaTime;
        transform.Translate(Vector3.right * moveDelta, Space.World);

        // --- MOVIMIENTO SINOIDAL (Onda opcional) ---
        if (_useSineWave)
        {
            // Modificamos el eje Y suavemente con una función seno mientras se desplaza
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
                    _movementDirection = -1;
                    DropFormation();
                    break;
                }
                else if (enemy.transform.position.x < -screenWidth + margin && _movementDirection < 0)
                {
                    _movementDirection = 1;
                    DropFormation();
                    break;
                }
            }
        }
    }

    private void DropFormation()
    {
        transform.position += Vector3.down * 0.3f;
        // Actualizamos la posición base de la onda si está activa para que no pegue saltos raros
        _initialYPosition = transform.position.y;
    }
}