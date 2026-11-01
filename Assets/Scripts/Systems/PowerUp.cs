using UnityEngine;

public class PowerUp : Collectible
{
    [SerializeField] private string _type; // "TripleShoot", "SpeedBoost", "Shield"

    protected override void ApplyEffect(Player player)
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
}