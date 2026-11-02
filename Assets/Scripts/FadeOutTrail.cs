using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class FadeOutTrail : MonoBehaviour
{
    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
    }

    // Este método se llamará cuando el misil muera
    public void BeginFade()
    {
        // 1. Desvinculamos la estela para que se quede flotando en el mundo
        transform.SetParent(null);

        // 2. Detenemos la emisión de nuevos puntos
        _trail.emitting = false;

        // 3. Iniciamos la corrutina para desvanecerla toda a la vez
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float fadeTime = 5.0f; // Lo que tardará en desaparecer toda la estela por completo
        float elapsedTime = 0f;
        
        float initialWidth = _trail.widthMultiplier;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;

            // Hacemos que la estela se vaya estrechando uniformemente en todas partes a la vez
            _trail.widthMultiplier = Mathf.Lerp(initialWidth, 0f, t);

            yield return null;
        }

        // Una vez desvanecida por completo, la destruimos de la escena
        Destroy(gameObject);
    }
}