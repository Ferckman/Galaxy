using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private Transform _bg1;
    [SerializeField] private Transform _bg2;
    [SerializeField] private float _scrollSpeed;

    private float _spriteHeight;

    private void Start()
    {
        if (_bg1.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) _spriteHeight = spriteRenderer.bounds.size.y;

        _bg1.position = new Vector3(0f, 0f, 0f);
        _bg2.position = new Vector3(0f, _spriteHeight, 0f);
    }

    private void Update()
    {
        float movement = _scrollSpeed * Time.deltaTime;

        _bg1.Translate(Vector3.down * movement, Space.World);
        _bg2.Translate(Vector3.down * movement, Space.World);

        if (_bg1.position.y <= -_spriteHeight) _bg1.position = new Vector3(_bg1.position.x, _bg2.position.y + _spriteHeight, _bg1.position.z);

        if (_bg2.position.y <= -_spriteHeight) _bg2.position = new Vector3(_bg2.position.x, _bg1.position.y + _spriteHeight, _bg2.position.z);
    }
}