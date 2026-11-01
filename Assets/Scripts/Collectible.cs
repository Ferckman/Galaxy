using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    [SerializeField] private float _speed = 4f;
    [SerializeField] private AudioClip _pickupSound;

    private void Update()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (!player.IsDisabled)
                {
                    // Aplicamos el efecto específico de cada objeto hijo
                    ApplyEffect(player);

                    // Reproducimos sonido si existe
                    if (_pickupSound != null && Camera.main != null)
                    {
                        AudioSource.PlayClipAtPoint(_pickupSound, Camera.main.transform.position, 1f);
                    }

                    // Destrucción del objeto
                    Destroy(gameObject);
                }
            }
        }
    }

    // Método abstracto que cada tipo de objeto implementará a su manera
    protected abstract void ApplyEffect(Player player);
}