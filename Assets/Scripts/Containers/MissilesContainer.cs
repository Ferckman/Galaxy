using UnityEngine;

public class MissilesContainer : MonoBehaviour
{
    // Instancia estática accesible globalmente
    public static MissilesContainer Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}