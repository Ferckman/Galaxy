/*using UnityEngine;

public class Kamikaze : Enemy
{
    [Header("Kamikaze Explosion Settings")]
    [SerializeField] private float _explosionScaleMultiplier;

    protected override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    protected override void SpawnExplosion()
    {
        if (_explosionPrefab != null) 
        {
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= _explosionScaleMultiplier;
        }
    }

    protected override bool ShouldDestroyOnPlayerHit
    {
        get
        {
            Player player = Object.FindAnyObjectByType<Player>();
            return player != null && player.ShieldActivated;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Obstacles>(out var obstacle))
        {
            if (obstacle.gameObject != this.gameObject)
            {
                return;
            }
        }

        base.OnTriggerEnter2D(other);
    }
    
}
*/

using UnityEngine;

public class Kamikaze : Enemy
{
    [Header("Kamikaze Settings")]
    [SerializeField] private float _explosionScaleMultiplier = 1.5f;
    [SerializeField] private float _diveSpeed = 8f; // Velocidad de inmersión en picado hacia el jugador

    private bool _isDiving = true; // Nace y desciende directamente al terreno de juego

    private void Update()
    {
        // En lugar de quedarse esperando en una formación, el kamikaze se lanza en picado vertical
        if (_isDiving)
        {
            transform.Translate(Vector3.down * _diveSpeed * Time.deltaTime);
            CheckBounds(); // Se destruye automáticamente si sale de la pantalla por abajo
            return;
        }
    }

    /*
    // Sobrescribimos esto para que no haga nada de la lógica de filas estáticas
    public override void SetupFormation(Vector3 targetPos)
    {
        // El kamikaze ignora la posición de la fila; su posición inicial la decide el Spawner al instanciarlo arriba.
    }

    // Sobrescribimos esto para que la señal de combate global no le afecte o dispare cosas extrañas
    public override void ActivateCombatBehavior()
    {
        // Ya está buceando hacia abajo por sí mismo
    }
    */

    protected override void SpawnExplosion()
    {
        if (_explosionPrefab != null) 
        {
            // Mantenemos el desfase en Z para evitar z-fighting y aplicamos el multiplicador de escala
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, Random.Range(-0.1f, -0.5f));
            GameObject explosion = Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);
            
            explosion.transform.localScale *= _explosionScaleMultiplier;
        }
    }

    protected override bool ShouldDestroyOnPlayerHit
    {
        get
        {
            Player player = Object.FindAnyObjectByType<Player>();
            return player != null && player.ShieldActivated;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Regla específica para ignorar colisiones con otros obstáculos que no sea él mismo
        if (other.TryGetComponent<Obstacles>(out var obstacle))
        {
            if (obstacle.gameObject != this.gameObject)
            {
                return;
            }
        }

        base.OnTriggerEnter2D(other);
    }
}