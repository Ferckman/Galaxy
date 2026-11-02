using UnityEngine;

public class PowerUp : Collectible
{
    [SerializeField] private string _type; // "TripleShoot", "SpeedBoost", "Shield"

    protected override bool ApplyEffect(Player player)
    {
        switch (_type)
        {
            case "TripleShoot":
                // Opcional: Si ya tiene el triple tiro activo, podrías evitar que lo coja devolviendo false, 
                // pero si quieres que se renueve o consuma siempre, lo dejamos que devuelva true:
                player.CanTripleShoot = true;
                player.TripleShotPowerupOn();
                break;
                
            case "SpeedBoost":
                player.Speed = 16;
                player.SpeedBoostPowerupOn();
                break;
                
            case "Shield":
                // Si ya tiene el escudo activo, quizás no quieras que gaste el power-up:
                /*
                if (player.ShieldActivated)
                {
                    return false; // El power-up se queda en el suelo porque ya tiene escudo
                }
                */
                player.ShieldActivated = true;
                player.ShieldPowerupOn();
                break;
        }

        // Devolvemos true para indicar que el power-up se ha aplicado y debe destruirse
        return true; 
    }
}