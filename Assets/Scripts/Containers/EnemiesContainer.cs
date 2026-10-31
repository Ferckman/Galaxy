using UnityEngine;

public class EnemiesContainer : MonoBehaviour
{
    // Instancia estática accesible desde cualquier parte del código
    public static EnemiesContainer Instance { get; private set; }

    private void Awake()
    {
        // Configuramos el Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evitamos duplicados por si acaso
        }
    }
}