using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

public class Shield : MonoBehaviour
{
    [SerializeField] private float _timeLife;

    public Shield(float timeLife)
    {
        _timeLife = timeLife;
    }

    private void Awake()
    {
        StartCoroutine(Blink());
    }

    public void SetTimeLife(float timeLife)
    {
        _timeLife = timeLife;
    }


    private void CheckTimeLife()
    {
        _timeLife -= Time.deltaTime;
        if(_timeLife <= 0f)
        {
            DisableShield();
            Destroy(this.gameObject);
        }
    }

    private void DisableShield()
    {
        Player player = transform.parent.gameObject.GetComponent<Player>();
        player.SetShieldActivated(false);
    }

    public void WeakenShield()
    {
        _timeLife -= 3.0f;
    }

    private IEnumerator Blink()
    {
        while(true)
        {
            if(_timeLife <= 10.0f && _timeLife > 0f)
            {
                SpriteRenderer _sprite = this.gameObject.GetComponent<SpriteRenderer>();
                _sprite.enabled = !_sprite.enabled;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    
    // Update is called once per frame
    private void Update()
    {
        CheckTimeLife();
    }
    
}
