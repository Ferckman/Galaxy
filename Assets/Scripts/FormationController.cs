using UnityEngine;
using System.Collections.Generic;

public class FormationController : MonoBehaviour
{
    private float _moveSpeed = 3f;      // Velocidad de desplazamiento lateral del bloque
    private float _maxXLimit = 7f;      // Límite derecho antes de rebotar (ajústalo según tu cámara)
    private int _movementDirection = 1; // 1 = Derecha, -1 = Izquierda
    
    private bool _isBlockedActive = false;
    private List<Enemy> _myEnemies = new List<Enemy>();

    public void Initialize(List<Enemy> enemies, float speed, float limit)
    {
        _myEnemies = enemies;
        _moveSpeed = speed;
        _maxXLimit = limit;
        _isBlockedActive = true;
    }

    private void Update()
    {
        if (!_isBlockedActive) return;

        // Limpiamos de la lista los enemigos que ya hayan muerto
        _myEnemies.RemoveAll(e => e == null);

        // Si ya no quedan enemigos vivos, destruimos el controlador de formación
        if (_myEnemies.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // Movemos todo el objeto contenedor (la formación entera) en bloque
        float moveDelta = _movementDirection * _moveSpeed * Time.deltaTime;
        transform.Translate(Vector3.right * moveDelta);

        // Comprobamos si el bloque toca los márgenes laterales de la pantalla para rebotar
        Camera cam = Camera.main;
        if (cam != null)
        {
            float screenWidth = cam.orthographicSize * cam.aspect;
            float margin = 1.0f; // Margen para que no toquen el borde exacto

            // Si algún hijo se pasa del límite derecho o izquierdo, cambiamos la dirección del bloque entero
            foreach (Enemy enemy in _myEnemies)
            {
                if (enemy == null) continue;

                if (enemy.transform.position.x > screenWidth - margin && _movementDirection > 0)
                {
                    _movementDirection = -1;
                    DropFormation(); // Opcional: bajan un poco al rebotar (estilo Space Invaders)
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

    // Opcional: Hace que todo el bloque baje una pizca al tocar los bordes para meter presión al jugador
    private void DropFormation()
    {
        transform.position += Vector3.down * 0.3f;
    }
}