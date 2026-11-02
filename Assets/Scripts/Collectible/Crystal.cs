using UnityEngine;

public class Crystal : Collectible
{
    [SerializeField] private int _energyAmount; // Cuánta carga otorga este cristal a la barra

    protected override bool ApplyEffect(Player player)
    {
        // 1. Buscamos el gestor del hiperrayo en el jugador o en la escena
        // (Asumiendo que tienes un script que controla la barra, ej: HyperBeamController)
        HyperBeamController beamController = player.GetComponent<HyperBeamController>();

        if (beamController != null)
        {
            // Comprobamos si la barra YA está a tope
            if (beamController.IsFull)
            {
                // Si está llena, devolvemos false para que el coleccionable NO se destruya ni reproduzca sonido
                return false; 
            }

            // Si no está llena, añadimos energía
            beamController.AddEnergy(_energyAmount);
            return true; // Devuelve true -> El cristal se destruye y suena el audio
        }

        return false;
    }
}