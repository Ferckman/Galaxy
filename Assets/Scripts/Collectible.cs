using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    [SerializeField] private float _speed = 4f;
    [SerializeField] private AudioClip _pickupSound;

    private void Update()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    // En tu Collectible.cs base
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (!player.IsDisabled)
                {
                    // Intentamos aplicar el efecto. Solo si devuelve true se destruye.
                    if (ApplyEffect(player))
                    {
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
    }

    // Cambiamos el tipo de retorno a bool en el base (por defecto devuelve true para otros coleccionables como PowerUps)
    protected virtual bool ApplyEffect(Player player)
    {
        return true;
    }
}