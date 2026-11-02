using UnityEngine;

public class Asteroid : Entity
{
    [SerializeField] private float _speed = 2f;

    public void Initialize(float scale)
    {
        SetAsteroidProperties(scale);
    }

    public void SetAsteroidProperties(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        _speed = _speed * (1f / scale);
        _health = Mathf.RoundToInt(scale * 60f);
        _damage = Mathf.RoundToInt(scale * 60f);
        _score = Mathf.RoundToInt(scale * 20f);
    }

    // Implementamos obligatoriamente el movimiento exigido por la clase base
    protected override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down, Space.World);
    }

    protected override void DropLoot()
    {
        
    }
}