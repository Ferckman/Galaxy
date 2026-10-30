using UnityEngine;

public class SmoothArcGuidance : IMissileGuidance
{
    private float _currentVelocity; // Utilizado para el suavizado inercial

    public float CalculateTargetRotation(Transform currentTransform, Transform target, Rigidbody2D rb, float maxRotateSpeed, float deltaTime)
    {
        if (target == null) return rb.rotation;

        Vector2 direction = (Vector2)target.position - rb.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // Usamos SmoothDampAngle en lugar de MoveTowards lineal para dar una inercia realista de misil
        // Esto previene los tirones bruscos y dibuja el arco fluido deseado (máximo dos fases de corrección)
        return Mathf.SmoothDampAngle(rb.rotation, targetAngle, ref _currentVelocity, 1f / (maxRotateSpeed * 0.1f), maxRotateSpeed, deltaTime);
    }
}