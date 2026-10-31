using UnityEngine;

public class Enemy : Obstacles
{
    private Vector3 _targetFormationPos;
    private bool _isMovingToFormation = false;
    private bool _isCombatActive = false;
    [SerializeField] private float _formationSpeed = 5f;

    // Método que llama el spawner al instanciarlo para indicarle dónde debe detenerse en la fila
    public void SetupFormation(Vector3 targetPos)
    {
        _targetFormationPos = targetPos;
        _isMovingToFormation = true;
    }

    // Método que llama el spawner cuando toda la fila está lista
    public void ActivateCombatBehavior()
    {
        _isCombatActive = true;
        Debug.Log($"{gameObject.name}: ¡Señal recibida! Entrando en combate.");
    }

    private void Update()
    {
        // FASE 1: Descenso inicial hasta su posición en la fila
        if (_isMovingToFormation)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetFormationPos, _formationSpeed * Time.deltaTime);

            // Si ya ha llegado a su sitio exacto, se detiene y pasa a modo espera en formación
            if (Vector3.Distance(transform.position, _targetFormationPos) < 0.01f)
            {
                transform.position = _targetFormationPos;
                _isMovingToFormation = false;
            }
            return; // No ejecuta combate mientras se está colocando
        }

        // FASE 2: Espera en formación (quieto o flotando suavemente hasta que llegue la señal)
        if (!_isCombatActive)
        {
            return; 
        }

        // FASE 3: LÓGICA DE COMBATE NORMAL
        // (Aquí irá el movimiento hacia el jugador, disparos, etc.)
    }
}