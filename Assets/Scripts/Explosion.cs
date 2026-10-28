using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 5.0f;

    private void Awake()
    {
        Destroy(gameObject, _lifeTime);
    }
}