using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacles : MonoBehaviour
{
    [SerializeField] private protected int _health;
    [SerializeField] private protected int _damage;
    [SerializeField] private int _speed;
    [SerializeField] private int _score;

    [SerializeField] private GameObject _explosionPrefab;

    public Obstacles(int health, int damage, int speed)
    {
        _health = health;
        _damage = damage;
        _speed = speed;
    }

    private void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    public virtual void IncreaseScore()
    {
        //Increase score
        UIManager UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        UIManager.UpdateScore(_score);
    }

    public virtual void DestroyMyself()
    {
        IncreaseScore();

        //Explosion animation in my position
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        //Delete from generator list
        GameManager gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        gameManager.DestroyConcreteElement(this.gameObject);

        Destroy(this.gameObject);
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
                    //If player's shield is not activated
                    if(!player.GetShieldActivated())
                    {
                        player.GetDamage(_damage);
                        player.CheckLife();
                    }
                    else
                    {
                        Transform shieldTransform = other.transform.Find("Shield");
                        if(shieldTransform != null)
                        {
                            //Weaken shield
                            GameObject shieldPlayer = other.transform.Find("Shield").gameObject;
                            Shield shield = shieldPlayer.GetComponent<Shield>();
                            shield.WeakenShield();
                        }
                    }

                    //Destroy myself
                    DestroyMyself();
                }
            }
        }
        else if(other.CompareTag("Shots"))
        {
            //Access the shot
            if(other.TryGetComponent<Shots>(out var shot))
            {
                _health -= shot.GetDamage();
                if(_health <= 0)
                    DestroyMyself();
            }

            Destroy(other.gameObject);
        }
    }
}
