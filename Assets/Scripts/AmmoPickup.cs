using UnityEngine;

public class AmmoPickup : Collectible
{
    [SerializeField] private int _ammoAmount = 2; // Cantidad de misiles que otorga

    protected override void ApplyEffect(Player player)
    {
        // Buscamos el MissileSpawner del jugador y le sumamos munición
        MissileSpawner missileSpawner = Object.FindFirstObjectByType<MissileSpawner>();
        if (missileSpawner != null)
        {
            missileSpawner.AddAmmo(_ammoAmount);
        }
    }
}