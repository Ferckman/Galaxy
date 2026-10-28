using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private string _type;
    [SerializeField] private int _speed = 4;
    [SerializeField] private AudioClip _powerupSound;

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    private void TypePowerUp(Player player)
    {
        switch (_type)
        {
            case "TripleShoot":
                player.CanTripleShoot = true;
                player.TripleShotPowerupOn();
                break;
                
            case "SpeedBoost":
                player.Speed = 16;
                player.SpeedBoostPowerupOn();
                break;
                
            case "Shield":
                player.ShieldActivated = true;
                player.ShieldPowerupOn();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (!player.IsDisabled)
                {
                    TypePowerUp(player);

                    if (_powerupSound != null && Camera.main != null) AudioSource.PlayClipAtPoint(_powerupSound, Camera.main.transform.position, 1f);

                    GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
                    if (gameManager != null) gameManager.DestroyConcreteElement(gameObject);

                    Destroy(gameObject);
                }
            }
        }
    }
}