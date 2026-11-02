using UnityEngine;

public class Kamikaze : Enemy
{
    [Header("Kamikaze Settings")]
    [SerializeField] private float _explosionScaleMultiplier = 1.5f;
    [SerializeField] private float _diveSpeed = 8f; // Velocidad de inmersión en picado hacia el jugador

    private bool _isDiving = true; // Nace y desciende directamente al terreno de juego

    protected override void Update()
    {
        // En lugar de quedarse esperando en una formación, el kamikaze se lanza en picado vertical
        if (_isDiving)
        {
            transform.Translate(Vector3.down * _diveSpeed * Time.deltaTime);
            CheckBounds(); // Se destruye automáticamente si sale de la pantalla por abajo
            return;
        }
    }

    protected override void SpawnExplosion()
    {
        if (_explosionPrefab != null) 
        {
            // Mantenemos el desfase en Z para evitar z-fighting y aplicamos el multiplicador de escala
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, Random.Range(-0.1f, -0.5f));
            GameObject explosion = Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);
            
            explosion.transform.localScale *= _explosionScaleMultiplier;
        }
        DropLoot();
    }

    protected override bool ShouldDestroyOnPlayerHit
    {
        get
        {
            Player player = Object.FindAnyObjectByType<Player>();
            return player != null && player.ShieldActivated;
        }
    }

    protected override void DropLoot()
    {
        // Tiramos los dados de 0 a 100, pero adaptado para el Kamikaze (100% de premio seguro)
        float roll = Random.Range(0f, 100f);

        // 50% de probabilidad para la munición de misiles
        if (roll < 50f)
        {
            if (_missileAmmoPrefab != null)
            {
                Instantiate(_missileAmmoPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            // 50% restante para un power-up general (Triple, Speed, Shield)
            SpawnRandomGeneralPowerUp();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        /*
        // Regla específica para ignorar colisiones con otros obstáculos que no sea él mismo
        if (other.TryGetComponent<Obstacles>(out var obstacle))
        {
            if (obstacle.gameObject != this.gameObject)
            {
                return;
            }
        }
        */



        base.OnTriggerEnter2D(other);
    }
}