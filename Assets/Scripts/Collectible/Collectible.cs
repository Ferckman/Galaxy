using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    [SerializeField] private protected float _speed = 4f;
    [SerializeField] private AudioClip _pickupSound;

    protected virtual void Update()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    // En tu Collectible.cs base
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spaceship"))
        {
            if (other.TryGetComponent<Spaceship>(out var spaceship))
            {
                if (!spaceship.IsDisabled)
                {
                    var playerController = other.GetComponent<PlayerController>();

                    if (ApplyEffect(playerController))
                    {
                        if (_pickupSound != null && Camera.main != null)
                            AudioSource.PlayClipAtPoint(_pickupSound, Camera.main.transform.position, 1f);

                        if (LootPool.Instance != null)
                            LootPool.Instance.ReturnToPool(gameObject);
                        else
                            Destroy(gameObject);
                    }
                }
            }
        }
    }
    

    // Cambiamos el tipo de retorno a bool en el base (por defecto devuelve true para otros coleccionables como PowerUps)
    protected virtual bool ApplyEffect(PlayerController playerController)
    {
        return true;
    }
}