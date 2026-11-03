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

    [Header("Configuración de Turbinas (Color Azul)")]
    [SerializeField] private SpriteRenderer[] _turbines; // Arrastra aquí tus 3 turbinas
    [SerializeField] private Color _activeBlueColor; // Elige el tono azul en el Inspector
    private Color[] _originalColors; // Array interno para guardar el color original (FFFFFF)

    private Player player;

    private void Awake()
    {
        player = Object.FindFirstObjectByType<Player>();
        if (player == null) Debug.LogError("Player component not found in the scene!", this);

        if (_audioSource == null)
        {
            Debug.LogWarning("No se encontró ningún AudioSource en el Player. Asegúrate de añadir uno.");
        }

        // Guardamos los colores originales de las turbinas al arrancar el juego
        if (_turbines != null && _turbines.Length > 0)
        {
            _originalColors = new Color[_turbines.Length];
            for (int i = 0; i < _turbines.Length; i++)
            {
                if (_turbines[i] != null)
                {
                    _originalColors[i] = _turbines[i].color;
                }
            }
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

        // 2. Activamos el contenedor del rayo (si lo tenías con SetActive)
        if (_hyperBeam != null)
        {
            _hyperBeam.SetActive(true);
        }

        // 3. CAMBIAMOS LAS TURBINAS AL COLOR AZUL (lo que añadimos antes)
        if (_turbines != null)
        {
            for (int i = 0; i < _turbines.Length; i++)
            {
                if (_turbines[i] != null)
                {
                    _turbines[i].color = _activeBlueColor;
                }
            }
        }

        if (player != null)
        {
            player.Speed = 16;
            player.SpeedBoostPowerupOn();
            player.ShieldActivated = true;
            player.ShieldPowerupOn();
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

        // 6. Apagamos el contenedor y devolvemos las turbinas a su color original
        if (_hyperBeam != null)
        {
            _hyperBeam.SetActive(false);

            if (_turbines != null && _originalColors != null)
            {
                for (int i = 0; i < _turbines.Length; i++)
                {
                    if (_turbines[i] != null)
                    {
                        _turbines[i].color = _originalColors[i];
                    }
                }
            }

            _currentEnergy = 0; 
            UpdateUI(); 
        }

        // 7. Liberamos el cerrojo
        _isFiring = false;
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