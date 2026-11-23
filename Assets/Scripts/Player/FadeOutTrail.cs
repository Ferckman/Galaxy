/*using System.Collections;
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
*/



/*
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class FadeOutTrail : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 5.0f;

    private TrailRenderer _trail;
    private Transform _originalParent;
    private Vector3 _originalLocalPosition;
    private float _initialWidth;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        _originalParent = transform.parent;
        _originalLocalPosition = transform.localPosition;
        _initialWidth = _trail.widthMultiplier;
    }

    public void ResetTrail()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        if (_originalParent != null)
        {
            transform.SetParent(_originalParent);
            transform.localPosition = _originalLocalPosition;
            transform.localRotation = Quaternion.identity;
        }

        _trail.Clear();
        _trail.widthMultiplier = _initialWidth;
        _trail.emitting = true;
        gameObject.SetActive(true);
    }

    public void BeginFade()
    {
        if (!gameObject.activeInHierarchy) return;

        transform.SetParent(null);
        _trail.emitting = false;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float elapsedTime = 0f;
        float startWidth = _trail.widthMultiplier;

        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeTime);
            _trail.widthMultiplier = Mathf.Lerp(startWidth, 0f, t);
            yield return null;
        }

        _trail.widthMultiplier = 0f;
        _fadeCoroutine = null;
        gameObject.SetActive(false);
    }
}
*/

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class FadeOutTrail : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 5.0f;

    private TrailRenderer _trail;
    private Transform _missileParent;
    private Vector3 _localPos;
    private Quaternion _localRot;
    private float _initialWidth;
    private Coroutine _fadeCoroutine;
    private bool _isInitialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        _trail = GetComponent<TrailRenderer>();
        _missileParent = transform.parent;
        _localPos = transform.localPosition;
        _localRot = transform.localRotation;
        if (_trail != null)
        {
            _initialWidth = _trail.widthMultiplier;
        }
        _isInitialized = true;
    }

    public void ResetTrail()
    {
        EnsureInitialized();
        if (_trail == null) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (_missileParent != null && transform.parent != _missileParent)
        {
            transform.SetParent(_missileParent, false);
            transform.localPosition = _localPos;
            transform.localRotation = _localRot;
        }

        _trail.emitting = false;
        _trail.Clear();
        _trail.widthMultiplier = _initialWidth;
        _trail.emitting = true;
    }

    public void BeginFade()
    {
        EnsureInitialized();
        if (_trail == null) return;

        transform.SetParent(null, true);
        _trail.emitting = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float elapsedTime = 0f;
        float startWidth = _trail.widthMultiplier;

        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeTime);
            _trail.widthMultiplier = Mathf.Lerp(startWidth, 0f, t);
            yield return null;
        }

        _trail.widthMultiplier = 0f;
        _fadeCoroutine = null;

        // Devolvemos la estela a su misil y la dejamos lista antes de ocultarla
        if (_missileParent != null)
        {
            transform.SetParent(_missileParent, false);
            transform.localPosition = _localPos;
            transform.localRotation = _localRot;
        }

        _trail.Clear();
        _trail.widthMultiplier = _initialWidth;
        gameObject.SetActive(false);
    }
}