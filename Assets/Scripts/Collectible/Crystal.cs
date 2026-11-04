using UnityEngine;

public class Crystal : Collectible
{
    private int _energyAmount = 5; // Cuánta carga otorga este cristal a la barra
    [SerializeField] private float _initialSpeed = 0.5f;  // Velocidad inicial lenta (siente el imán)
    [SerializeField] private float _acceleration = 5.0f; // Lo rápido que aumenta la velocidad de atracción
    private float _currentSpeed; // Velocidad actual que irá subiendo

    private Transform _playerTransform;
    private HyperBeamController _beamController;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Spaceship");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _beamController = playerObj.GetComponent<HyperBeamController>();
        }

        // Empezamos con la velocidad base
        _currentSpeed = _initialSpeed;
    }

    protected override void Update()
    {
        // Si tenemos al jugador y la barra NO está llena, volamos hacia él
        if (_playerTransform != null && _beamController != null && !_beamController.IsFull)
        {
            // 1. Incrementamos la velocidad cada frame (aceleración magnética)
            _currentSpeed += _acceleration * Time.deltaTime;

            // 2. Calculamos la dirección hacia la nave
            Vector2 direction = (_playerTransform.position - transform.position).normalized;

            // 3. Movemos el cristal usando esa velocidad acelerada
            transform.position = Vector2.MoveTowards(
                transform.position, 
                _playerTransform.position, 
                _currentSpeed * Time.deltaTime
            );
        }
        else
        {
            // Si la barra está llena (o no hay jugador), ejecutamos el comportamiento del padre (caer hacia abajo)
            base.Update(); 
        }
    }

    protected override bool ApplyEffect(PlayerController playerController)
    {
        // 1. Buscamos el gestor del hiperrayo en el jugador o en la escena
        // (Asumiendo que tienes un script que controla la barra, ej: HyperBeamController)
        HyperBeamController beamController = playerController.GetComponent<HyperBeamController>();

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