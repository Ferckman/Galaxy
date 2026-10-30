using UnityEngine;

public interface IMissileGuidance
{
    float CalculateTargetRotation(Transform currentTransform, Transform target, Rigidbody2D rb, float maxRotateSpeed, float deltaTime);
}