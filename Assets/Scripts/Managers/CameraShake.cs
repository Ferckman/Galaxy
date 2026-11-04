using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Patrón Singleton para acceder desde cualquier script fácilmente sin buscarlo en la escena
    public static CameraShake Instance { get; private set; }

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

    // Método público al que llamaremos desde cualquier clase (Player, HyperBeam, etc.)
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Transform camTransform = transform; // Como está en la cámara, 'transform' es su propia posición
        Vector3 originalPosition = camTransform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPosition;
    }
}