using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int _health;
    [SerializeField] private int _speed;
    [SerializeField] private int _numLives;
    [SerializeField]  private bool _shieldActivated;
    [SerializeField] private bool _canTripleShoot;
    [SerializeField] private bool _disabled;
    [SerializeField] private float _cooldownTripleShoot;
    [SerializeField] private float _cooldownMoreSpeed;
    [SerializeField] private GameObject[] _engines;

    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _shieldPrefab;

    [SerializeField] private AudioSource _audioLaser;
    [SerializeField] private AudioSource _audioMissile;

    private float _canFire;
    private float _canBigFire;

    public Player(int health, int speed, int numLives, bool shieldActivated, bool canTripleShoot, bool disabled, float cooldownTripleShoot, float cooldownMoreSpeed)
    {
        _health = health;
        _speed = speed;
        _numLives = numLives;
        _shieldActivated = shieldActivated;
        _canTripleShoot = canTripleShoot;
        _disabled = disabled;
        _cooldownTripleShoot = cooldownTripleShoot;
        _cooldownMoreSpeed = cooldownMoreSpeed;
    }

    private void ResetValues()
    {
        _health = 100;
        _speed = 3;
        _shieldActivated = false;
        _canTripleShoot = false;
        _disabled = true;
        _cooldownTripleShoot = 0f;
        _cooldownMoreSpeed = 0f;
        _engines[0].SetActive(false);
        _engines[1].SetActive(false);
    }

    private void Awake()
    {
        StartCoroutine(StartGame());
        StartCoroutine(Blink());
    }

    public void SetSpeed(int speed)
    {
        _speed = speed;
    }

    public bool GetShieldActivated()
    {
        return _shieldActivated;
    }

    public void SetShieldActivated(bool shieldActivated)
    {
        _shieldActivated = shieldActivated;
    }

    public void SetCanTripleShoot(bool canTripleShoot)
    {
        _canTripleShoot = canTripleShoot;
    }

    public bool GetDisabled()
    {
        return _disabled;
    }
    
    private IEnumerator StartGame()
    {
        while(_disabled)
        {
            //Start position
            transform.position = new Vector3(0, -4.0f, 0);

            yield return new WaitForSeconds(4.0f);

            //When the immune seconds of the start end
            _disabled = false;
            SpriteRenderer _sprite = this.gameObject.GetComponent<SpriteRenderer>();
            _sprite.enabled = true;

            SpriteRenderer _spriteThrusters = GameObject.Find("Thruster").GetComponent<SpriteRenderer>();
            _spriteThrusters.enabled = true;
        }
    }

    private IEnumerator Blink()
    {
        while(_disabled)
        {
            SpriteRenderer _sprite = this.gameObject.GetComponent<SpriteRenderer>();
            _sprite.enabled = !_sprite.enabled;

            SpriteRenderer _spriteThrusters = GameObject.Find("Thruster").GetComponent<SpriteRenderer>();
            _spriteThrusters.enabled = !_spriteThrusters.enabled;

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator EndGame()
    {
        while(true)
        {
            yield return new WaitForSeconds(4.0f);

            UIManager uIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
            uIManager.ShowMainMenu();
            GameManager gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
            gameManager.SetGameStart(false);
            gameManager.DestroyElements();
            Destroy(this.gameObject);
            break;
        }
    }
    

    private void Movement()
    {
        float _horizontalInput = Input.GetAxis("Horizontal");
        float _verticalInput = Input.GetAxis("Vertical");

        transform.Translate(_speed * _horizontalInput * Time.deltaTime * Vector3.right);
        transform.Translate(_speed * _verticalInput * Time.deltaTime * Vector3.up);
    }

    private void Limit()
    {
        //Límite vertical
        if(transform.position.y > 4.2f)
            transform.position = new Vector3(transform.position.x, 4.2f, 0);
        else if(transform.position.y < -4.2f)
            transform.position = new Vector3(transform.position.x, -4.2f, 0);

        //Límite horizontal
        if(transform.position.x > 8.2f)
            transform.position = new Vector3(8.2f, transform.position.y, 0);
        else if(transform.position.x < -8.2f)
            transform.position = new Vector3(-8.2f, transform.position.y, 0);
    }

    private void ShootLaser()
    {
        if(!_disabled)
        {
            float _fireRate = 0.25f, _bigFireRate = 10.0f;

            //Space or Left Click
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if(Time.time > _canFire)
                {
                    if(_canTripleShoot)
                    {
                        //Left laser
                        Instantiate(_laserPrefab, transform.position + new Vector3(-0.56f, 0, 0), Quaternion.identity);
                        //Right laser
                        Instantiate(_laserPrefab, transform.position + new Vector3(0.56f, 0, 0), Quaternion.identity);
                    }

                    //Center laser
                    Instantiate(_laserPrefab, transform.position + new Vector3(0, 0.86f, 0), Quaternion.identity);

                    //Audio laser
                    _audioLaser.Play();

                    _canFire = Time.time + _fireRate;
                }
            }
            //Enter or Right Click
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1))
            {
                if(Time.time > _canBigFire)
                {
                    Instantiate(_missilePrefab, transform.position + new Vector3(-0.56f, -1.2f, 0), Quaternion.identity);
                    Instantiate(_missilePrefab, transform.position + new Vector3(0.14f, 0, 0), Quaternion.identity);
                    Instantiate(_missilePrefab, transform.position + new Vector3(-0.14f, 0, 0), Quaternion.identity);
                    Instantiate(_missilePrefab, transform.position + new Vector3(0.56f, -1.2f, 0), Quaternion.identity);

                    //Audio missile
                    _audioMissile.Play();

                    _canBigFire = Time.time + _bigFireRate;
                }
            }
        }
    }

    public void CheckLife()
    {
        if(_health <= 0)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            _numLives--;
            //Reset values
            ResetValues();

            if(GameObject.Find("Canvas").TryGetComponent<UIManager>(out var uIManager))
                uIManager.UpdateLives(_numLives);
            
            if(_numLives > 0)
            {
                StartCoroutine(StartGame());
                StartCoroutine(Blink());
            }
            else
            {
                //Ship and thrusters not visible
                SpriteRenderer _spriteShip = gameObject.GetComponent<SpriteRenderer>();
                _spriteShip.enabled = false;
                
                SpriteRenderer _spriteThrusters = GameObject.Find("Thruster").GetComponent<SpriteRenderer>();
                _spriteThrusters.enabled = false;

                StartCoroutine(EndGame());
            }
        }
        else if(_health <= 40)
        {
            //Visible failure of both engines
            _engines[0].SetActive(true);
            _engines[1].SetActive(true);
        }
        else if(_health <= 60)
        {
            //Engine failure
            _engines[Random.Range(0, 2)].SetActive(true);
        }
    }

    private void CheckMeters()
    {
        _cooldownTripleShoot -= Time.deltaTime;
        _cooldownTripleShoot = Mathf.Clamp(_cooldownTripleShoot, 0f, 10.0f);
        _cooldownMoreSpeed -= Time.deltaTime;
        _cooldownMoreSpeed = Mathf.Clamp(_cooldownMoreSpeed, 0f, 10.0f);

        _numLives = Mathf.Clamp(_numLives, 0, 3);

        if(_cooldownTripleShoot == 0f)
            _canTripleShoot = false;

        if(_cooldownMoreSpeed == 0f)
            _speed = 6;
    }

    public void TripleShotPowerupOn()
    {
        _cooldownTripleShoot = 10.0f;
    }

    public void SpeedBoostPowerupOn()
    {
        _cooldownMoreSpeed = 10.0f;
    }

    public void ShieldPowerupOn()
    {
        Transform shieldTransform = transform.Find("Shield");

        //If shield doesn't exist
        if(shieldTransform == null)
        {
            GameObject shield = Instantiate(_shieldPrefab, transform.position, Quaternion.identity);
            shield.name = "Shield";
            shield.transform.parent = this.gameObject.transform;
        }
        else
        {
            //Reset shield life time
            Shield shield = GameObject.Find("Shield").GetComponent<Shield>();
            shield.SetTimeLife(20.0f);
            SpriteRenderer spriteShield = GameObject.Find("Shield").GetComponent<SpriteRenderer>();
            spriteShield.enabled = true;
        }
    }

    public void GetDamage(int damage)
    {
        _health -= damage;
    }

    private void Update()
    {
        Movement();
        Limit();
        ShootLaser();
        CheckMeters();
    }
}
