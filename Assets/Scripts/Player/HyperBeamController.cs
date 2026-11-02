using UnityEngine;
using UnityEngine.UI; // Necesario si usas UI de Unity (Sliders)

public class HyperBeamController : MonoBehaviour
{
    [Header("Configuración de Energía")]
    private int _maxEnergy = 50; // Cuántos cristales hacen falta
    private int _currentEnergy = 0;

    [Header("Referencias Visuales (UI)")]
    [SerializeField] private Slider _beamSlider; // Opcional: tu barra de progreso en pantalla

    [SerializeField] private GameObject _beamObject;

    public bool IsFull => _currentEnergy >= _maxEnergy;

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
            
            // ¡Aquí lo dejamos a cero y seco!
            _currentEnergy = 0; 
            UpdateUI(); // Actualizamos la UI para que la barra baje vacía al instante
        }
        else
        {
            Debug.Log("¡El hiperrayo aún no está cargado!");
        }
    }

    private void ActivateBeam()
    {
        // Aquí activas tu hiperrayo hijo:
        _beamObject.SetActive(true);
        Debug.Log("¡HIPERRAYO ACTIVADO Y BATERÍA GASTADA!");
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