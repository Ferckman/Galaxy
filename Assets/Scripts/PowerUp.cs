using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private string _type;
    [SerializeField] private int _speed;
    [SerializeField] private AudioClip _powerupSound;

    public PowerUp(string type, int speed)
    {
        _type = type;
        _speed = speed;
    }

    private void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    private void TypePowerUp(Player player)
    {
        switch(_type)
        {
            case "TripleShoot":
                //Enable triple shot
                player.SetCanTripleShoot(true);
                player.TripleShotPowerupOn();
            break;
            case "SpeedBoost":
                //Speed boost
                player.SetSpeed(10);
                player.SpeedBoostPowerupOn();
            break;
            case "Shield":
                //Activate shield
                player.SetShieldActivated(true);
                player.ShieldPowerupOn();
            break;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        Movement();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            //Access the player
            if(other.TryGetComponent<Player>(out var player))
            {
                //If player is not desactivated
                if(!player.GetDisabled())
                {
                    TypePowerUp(player);

                    AudioSource.PlayClipAtPoint(_powerupSound, Camera.main.transform.position, 1f);

                    //Delete from generator list
                    GameManager gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
                    gameManager.DestroyConcreteElement(this.gameObject);

                    //Destroy ourself
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
