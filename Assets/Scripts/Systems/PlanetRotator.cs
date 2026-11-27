using UnityEngine;

public class PlanetRotator : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de giro en grados por segundo. Usa valores bajos (ej. 2 a 8) para planetas grandes.")]
    [SerializeField] private float _rotationSpeed = 4f;
    
    [Tooltip("Eje sobre el que gira el planeta (por defecto el eje Y vertical).")]
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;

    [Header("Inclinación Axial (Opcional)")]
    [Tooltip("Inclina el planeta unos grados al iniciar para que el giro se vea más natural y cinematográfico.")]
    [SerializeField] private Vector3 _initialTilt = new Vector3(0f, 0f, 15f);

    private void Start()
    {
        // Aplica una ligera inclinación inicial al planeta (como la Tierra)
        transform.localRotation = Quaternion.Euler(_initialTilt);
    }

    private void Update()
    {
        // Gira el planeta suavemente sobre su propio eje inclinado
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime, Space.Self);
    }
}