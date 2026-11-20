using UnityEngine;

public class Asteroid : Entity
{
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _crystalPrefab;
    private Vector3 _moveDirection;

    public void Initialize(float scale)
    {
        SetAsteroidProperties(scale);
        SetupRandomSpawnAndTrajectory();
    }

    /*public void SetAsteroidProperties(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        _speed = _speed * (1f / scale);
        _health = Mathf.RoundToInt(scale * 60f);
        _damage = Mathf.RoundToInt(scale * 120f);
        _score = Mathf.RoundToInt(scale * 20f);
    }*/

    public void SetAsteroidProperties(float scale)
    {
        float safeScale = Mathf.Max(0.2f, scale);
        transform.localScale = new Vector3(safeScale, safeScale, safeScale);

        // Velocidad base más viva
        // Los pequeños van más rápidos y los grandes más pesados, sin perder ritmo
        float speedFactor = Mathf.Lerp(1.35f, 0.65f, Mathf.Clamp01((safeScale - 0.5f) / 2.5f));
        _speed = _speed * speedFactor;

        // Vida: que los pequeños aguanten 1-2 láseres y los grandes exijan ráfagas o un misil
        _health = Mathf.RoundToInt(safeScale * 260f);

        // Daño por choque contra 4000 de vida: molesta y obliga a esquivar sin ser instant-kill
        _damage = Mathf.RoundToInt(safeScale * 350f);

        // Puntuación proporcional al riesgo y dureza
        _score = Mathf.RoundToInt(safeScale * 50f);
    }

    private void SetupRandomSpawnAndTrajectory()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float screenHeight = cam.orthographicSize;
        float screenWidth = cam.orthographicSize * cam.aspect;
        float spawnOffset = 3f; // Distancia extra fuera de pantalla para que aparezcan de la nada

        int posIni = Random.Range(1, 5); // Elige aleatoriamente del 1 al 4
        Vector3 spawnPos = Vector3.zero;
        Vector3 targetPos = Vector3.zero;

        switch (posIni)
        {
            case 1: // Arriba -> cruza hacia abajo
                spawnPos = new Vector3(Random.Range(-screenWidth, screenWidth), screenHeight + spawnOffset, 0f);
                targetPos = new Vector3(Random.Range(-screenWidth * 0.8f, screenWidth * 0.8f), -screenHeight - spawnOffset, 0f);
                break;
            case 2: // Abajo -> cruza hacia arriba
                spawnPos = new Vector3(Random.Range(-screenWidth, screenWidth), -screenHeight - spawnOffset, 0f);
                targetPos = new Vector3(Random.Range(-screenWidth * 0.8f, screenWidth * 0.8f), screenHeight + spawnOffset, 0f);
                break;
            case 3: // Izquierda -> cruza hacia la derecha
                spawnPos = new Vector3(-screenWidth - spawnOffset, Random.Range(-screenHeight, screenHeight), 0f);
                targetPos = new Vector3(screenWidth + spawnOffset, Random.Range(-screenHeight * 0.8f, screenHeight * 0.8f), 0f);
                break;
            case 4: // Derecha -> cruza hacia la izquierda
                spawnPos = new Vector3(screenWidth + spawnOffset, Random.Range(-screenHeight, screenHeight), 0f);
                targetPos = new Vector3(-screenWidth - spawnOffset, Random.Range(-screenHeight * 0.8f, screenHeight * 0.8f), 0f);
                break;
        }

        // Posicionamos el asteroide y calculamos su dirección normalizada hacia el punto de destino
        transform.position = spawnPos;
        _moveDirection = (targetPos - spawnPos).normalized;
    }

    // Sobrescribimos el Movement exigido por la clase base Entity
    protected override void Movement()
    {
        transform.Translate(_moveDirection * _speed * Time.deltaTime, Space.World);
    }

    protected override void DropLoot()
    {
        if (_crystalPrefab != null) Instantiate(_crystalPrefab, transform.position, Quaternion.identity);
    }
}