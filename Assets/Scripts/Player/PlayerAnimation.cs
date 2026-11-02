using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    // Caché para recordar la última dirección enviada y evitar llamadas redundantes en el Update
    private int _lastSentDirection = -99;

    private void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
    }

    public void UpdateAnimation(float horizontalInput)
    {
        if (_anim == null) return;

        // Convertimos el input flotante de GetAxisRaw (-1, 0 o 1) en un entero limpio
        int currentDirection = 0;
        if (horizontalInput < 0f) currentDirection = -1;
        else if (horizontalInput > 0f) currentDirection = 1;

        // OPTIMIZACIÓN SENIOR: Solo enviamos la orden al Animator si la dirección ha cambiado
        if (currentDirection != _lastSentDirection)
        {
            _lastSentDirection = currentDirection;
            _anim.SetInteger("TurnDir", currentDirection);
        }
    }
}