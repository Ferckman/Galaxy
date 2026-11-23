using UnityEngine;

public class ParallaxUVScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float _scrollSpeed = 0.08f;
    [SerializeField] private bool _useSharedMaterial = false;

    private Material _material;
    private Vector2 _currentOffset;

    private void Awake()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            _material = _useSharedMaterial ? renderer.sharedMaterial : renderer.material;
        }
    }

    private void Update()
    {
        if (_material == null) return;

        // Desplazamos
        _currentOffset.y += _scrollSpeed * Time.deltaTime;
        _currentOffset.y %= 1f;

        _material.mainTextureOffset = _currentOffset;
    }

    private void OnDestroy()
    {
        if (!_useSharedMaterial && _material != null)
        {
            Destroy(_material);
        }
    }
}