using UnityEngine;

public class PowerUp : Collectible
{
    [SerializeField] private string _type; // "TripleShoot", "SpeedBoost", "Shield"

    protected override bool ApplyEffect(PlayerController playerController)
    {
        switch (_type)
        {
            case "TripleShoot":
                // Opcional: Si ya tiene el triple tiro activo, podrías evitar que lo coja devolviendo false, 
                // pero si quieres que se renueve o consuma siempre, lo dejamos que devuelva true:
                playerController.CanTripleShoot = true;
                playerController.TripleShotPowerupOn();
                break;
                
            case "SpeedBoost":
                playerController.SpeedBoostPowerupOn();
                break;
                
            case "Shield":
                //playerController.ShieldActivated = true;
                playerController.ShieldPowerupOn();
                break;
        }

        // Devolvemos true para indicar que el power-up se ha aplicado y debe destruirse
        return true; 
    }
}