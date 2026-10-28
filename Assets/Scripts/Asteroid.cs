using UnityEngine;

public class Asteroid : Obstacles
{
    private Vector3 _customDirection;

    protected override void Awake()
    {
        base.Awake();

        Vector3 targetCenter = new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, 2f), 0f);
        _customDirection = (targetCenter - transform.position).normalized;
    }

    public void SetAsteroidProperties(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);

        _speed = _speed * (1f / scale);

        _health = Mathf.RoundToInt(scale * 125f);

        _damage = Mathf.RoundToInt(scale * 15f);

        _score = Mathf.RoundToInt(scale * 20f);
    }

    protected override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * _customDirection);
    }
}