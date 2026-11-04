using UnityEngine;

public class AmmoPickup : Collectible
{
    [SerializeField] private int _ammoAmount = 2; // Cantidad de misiles que otorga

    // En tu clase de Collectible de Munición (ej. AmmoCollectible.cs)
    protected override bool ApplyEffect(PlayerController playerController)
    {
        MissileSpawner missileSpawner = Object.FindFirstObjectByType<MissileSpawner>();
        if (missileSpawner != null)
        {
            // Intentamos añadir munición y devolvemos el resultado (true si sumó, false si estaba lleno)
            return missileSpawner.TryAddAmmo(_ammoAmount);
        }
        
        return false;
    }
}