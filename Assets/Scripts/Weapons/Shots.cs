/*using System.Collections;
using UnityEngine;

public abstract class Shots : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private protected int _speed;

    public int Damage => _damage;

    private void Update()
    {
        Movement();
    }

    public virtual void Movement()
    {

    }
}
*/

using UnityEngine;

public abstract class Shots : MonoBehaviour
{
    [Header("Base Stats del Láser")]
    [SerializeField] private int _baseMinDamage = 180;
    [SerializeField] private int _baseMaxDamage = 250;
    [SerializeField, Range(0f, 100f)] private float _baseCritChance = 15f;
    [SerializeField] private float _critMultiplier = 2f;

    [SerializeField] private protected int _speed;

    // Factores de escala que vendrán del jugador o de sus mejoras
    private float _damageMultiplier = 1f;
    private float _bonusCritChance = 0f;

    /// <summary>
    /// Método para inicializar el disparo con los stats actuales del jugador/nave.
    /// </summary>
    public void Initialize(float damageMultiplier, float bonusCritChance)
    {
        _damageMultiplier = damageMultiplier;
        _bonusCritChance = bonusCritChance;
    }

    public int GetCalculatedDamage(out bool isCrit)
    {
        // Aplicamos el multiplicador de la nave/mejoras al rango base
        int scaledMin = Mathf.RoundToInt(_baseMinDamage * _damageMultiplier);
        int scaledMax = Mathf.RoundToInt(_baseMaxDamage * _damageMultiplier);

        int baseDamage = Random.Range(scaledMin, scaledMax + 1);

        // Probabilidad de crítico sumando el base + bonus de la nave/mejoras
        float totalCritChance = _baseCritChance + _bonusCritChance;
        float roll = Random.Range(0f, 100f);

        if (roll <= totalCritChance)
        {
            isCrit = true;
            return Mathf.RoundToInt(baseDamage * _critMultiplier);
        }

        isCrit = false;
        return baseDamage;
    }

    private void Update()
    {
        Movement();
    }

    public virtual void Movement() { }
}