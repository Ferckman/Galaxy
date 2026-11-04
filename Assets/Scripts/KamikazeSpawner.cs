using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeSpawner : MonoBehaviour
{
    private const int MaxEnemies = 60;

    [Header("Prefabs")]
    [SerializeField] private GameObject _kamikazePrefab;         
    [SerializeField] private GameObject _warningIndicatorPrefab;

    /// <summary>
    /// Gestiona la aparición de kamikazes en lotes temporales con sus respectivas alertas visuales.
    /// </summary>
    public IEnumerator SpawnKamikazesOverTimeRoutine(WaveManager.Wave wave, List<GameObject> blockEnemiesList, List<GameObject> kamikazeEnemiesList)
    {
        if (_kamikazePrefab == null) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        float spawnY = cam.orthographicSize + 1.0f;
        Vector3 leftLimit = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        Vector3 rightLimit = cam.ViewportToWorldPoint(new Vector3(1f, 0f, cam.nearClipPlane));
        float margin = 1.5f;

        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        yield return new WaitForSeconds(2.0f); 

        int totalCount = wave.kamikazeCount;
        int spawnedCount = 0;

        while (spawnedCount < totalCount)
        {
            if (blockEnemiesList.Count + kamikazeEnemiesList.Count >= MaxEnemies) break;

            // Decidimos cuántos kamikazes saldrán en este lote (respetando el rango y los que faltan)
            int batchSize = Random.Range(wave.minKamikazesPerSpawn, wave.maxKamikazesPerSpawn + 1);
            batchSize = Mathf.Min(batchSize, totalCount - spawnedCount);

            // Lista para almacenar las posiciones X de este lote y asegurar la separación
            List<float> batchXPositions = new List<float>();
            float minSeparation = 4f; // Distancia mínima obligatoria entre kamikazes simultáneos

            for (int b = 0; b < batchSize; b++)
            {
                float randomX = 0f;
                bool validPosition = false;
                int attempts = 0;

                // Buscamos una posición X que no esté demasiado cerca de otro kamikaze de este mismo lote
                while (!validPosition && attempts < 15)
                {
                    randomX = Random.Range(leftLimit.x + margin, rightLimit.x - margin);
                    validPosition = true;

                    foreach (float existingX in batchXPositions)
                    {
                        if (Mathf.Abs(randomX - existingX) < minSeparation)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    attempts++;
                }

                batchXPositions.Add(randomX);

                // Mostramos la advertencia visual para cada uno en su posición separada
                if (_warningIndicatorPrefab != null)
                {
                    Vector3 warningPos = new Vector3(randomX, spawnY - 1.5f, 0f);
                    GameObject warning = Instantiate(_warningIndicatorPrefab, warningPos, Quaternion.identity);
                    
                    WarningIndicator indicatorScript = warning.GetComponent<WarningIndicator>();
                    if (indicatorScript != null) indicatorScript.Initialize(1.0f);
                }
            }

            // Pausa dramática para que el jugador procese la alerta de todo el lote
            yield return new WaitForSeconds(1.0f); 

            // Instanciamos físicamente todos los kamikazes de este lote
            foreach (float posX in batchXPositions)
            {
                Vector3 spawnPos = new Vector3(posX, spawnY, 0f);
                GameObject kamikazeObj = Instantiate(_kamikazePrefab, spawnPos, Quaternion.identity, container);
                
                if (kamikazeObj != null)
                {
                    kamikazeEnemiesList.Add(kamikazeObj);
                }

                spawnedCount++;
            }

            // Si aún quedan kamikazes por salir en la oleada, esperamos el intervalo configurado
            if (spawnedCount < totalCount)
            {
                yield return new WaitForSeconds(wave.kamikazeSpawnInterval);
            }
        }
    }
}