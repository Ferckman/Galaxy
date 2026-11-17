using UnityEngine;
using UnityEngine.UI; // Necesario si usas UI de Unity (Sliders)
using System.Collections;

public class HyperBeamController : MonoBehaviour
{
    [Header("Configuración de Energía")]
    private int _maxEnergy = 50; // Cuántos cristales hacen falta
    private int _currentEnergy = 0;

    [Header("Referencias Visuales (UI)")]
    [SerializeField] private Slider _beamSlider; // Opcional: tu barra de progreso en pantalla

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _laserSoundClip;

    //[Header("Referencias")]
    [SerializeField] private Animator beamAnimator;
    public bool IsFull => _currentEnergy >= _maxEnergy;
    [SerializeField] private GameObject _hyperBeam;     // El contenedor que escala de 0 a 1
    private bool _isFiring = false; // Seguridad para evitar que se solape si se pulsa muchas veces

    [Header("Configuración")]
    private float _beamDuration = 12.5f;   // El tiempo que se queda el rayo disparando

    private PlayerController _playerController;
    private Spaceship spaceship;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        spaceship = GetComponent<Spaceship>();
        if (_playerController == null) Debug.LogError("Player component not found in the scene!", this);

        if (_audioSource == null)
        {
            Debug.LogWarning("No se encontró ningún AudioSource en el Player. Asegúrate de añadir uno.");
        }
    }

    public void AddEnergy(int amount)
    {
        _currentEnergy = Mathf.Min(_currentEnergy + amount, _maxEnergy);
        UpdateUI();
        
        Debug.Log("Energía del Hiperrayo: " + _currentEnergy + " / " + _maxEnergy);
    }

    // Método que se llama cuando el jugador pulsa la tecla para disparar
    public void TryFireHyperBeam()
    {
        if (IsFull)
        {
            ActivateBeam();
        }
        else
        {
            Debug.Log("¡El hiperrayo aún no está cargado!");
        }
    }

    public void ActivateBeam()
    {
        // Si ya está disparando, ignoramos la nueva pulsación
        if (_isFiring) return;

        StartCoroutine(BeamSequenceRoutine());
    }

    private IEnumerator BeamSequenceRoutine()
    {
        _isFiring = true;

        // 1. El audio suena al instante
        if (_audioSource != null && _laserSoundClip != null)
        {
            _audioSource.PlayOneShot(_laserSoundClip);
        }

        // 2. ACTIVAMOS EL CAMERA SHAKE AL INSTANTE DE DISPARAR
        //StartCoroutine(CameraShakeRoutine(_shakeDuration, _shakeMagnitude));
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(12.5f, 0.2f);
        }

        // 2. Activamos el contenedor del rayo (si lo tenías con SetActive)
        if (_hyperBeam != null)
        {
            _hyperBeam.SetActive(true);
        }

        if (_playerController != null)
        {
            _playerController.Invincibility(true);
            _playerController.SpeedBoostPowerupOn(8);
            spaceship.ShieldActivated = true;
            _playerController.ShieldPowerupOn();
        }

        // Añade esto justo antes del SetTrigger para que Unity te diga la verdad a la cara:
        Debug.Log("Estoy intentando usar el Animator del objeto: " + beamAnimator.gameObject.name + " que tiene el controlador: " + beamAnimator.runtimeAnimatorController.name);

        // 4. DECIMOS AL ANIMATOR QUE ENTRE EN MODO DISPARO (Se queda abierto)
        if (beamAnimator != null)
        {
            beamAnimator.SetBool("IsFiring", true); 
        }

        // 5. Mantenemos el rayo activo durante los segundos que dura el disparo
        yield return new WaitForSeconds(_beamDuration);

        // 6. DECIMOS AL ANIMATOR QUE TERMINE (Vuelve al Idle)
        if (beamAnimator != null)
        {
            beamAnimator.SetBool("IsFiring", false); 
        }

        // 6. Apagamos el contenedor
        if (_hyperBeam != null)
        {
            _hyperBeam.SetActive(false);

            _currentEnergy = 0; 
            UpdateUI(); 
        }

        // 7. Liberamos el cerrojo
        _isFiring = false;

        _playerController.Invincibility(false);
    }

    private void UpdateUI()
    {
        if (_beamSlider != null)
        {
            // Actualiza la barra de forma proporcional (de 0 a 1)
            _beamSlider.value = (float)_currentEnergy / _maxEnergy;
        }
    }
}