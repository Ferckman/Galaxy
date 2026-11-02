using UnityEngine;
using System.Collections.Generic;

public class MissileSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _missilePrefab;

    [Header("Missile Economy & Cooldown")]
    [SerializeField] private int _maxMissileAmmo;      // Munición máxima
    [SerializeField] private int _currentMissileAmmo;     // Munición actual
    [SerializeField] private float _bigFireRate;     // Enfriamiento de 2 segundos
    private float _canBigFire = 0f;                       // Temporizador de control

    private List<Transform> _leftSpawnPoints = new List<Transform>();
    private List<Transform> _rightSpawnPoints = new List<Transform>();

    private void Awake()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;

            if (child.name.Contains("Spawn"))
            {
                if (child.localPosition.x < 0f)
                {
                    _leftSpawnPoints.Add(child);
                }
                else
                {
                    _rightSpawnPoints.Add(child);
                }
            }
        }
    }

    // NUEVO: Método público que intenta disparar controlando cooldown y munición
    public bool TryFireSalvo()
    {
        if (Time.time < _canBigFire) return false;

        if (_currentMissileAmmo <= 0)
        {
            Debug.Log("¡Sin munición de misiles!");
            return false;
        }

        List<Transform> activeTargets = FindActiveTargets();
        if (activeTargets.Count == 0) return false;

        // Ejecutamos la lógica y capturamos cuántos misiles se han disparado en total
        int missilesFired = FireMissileSalvoLogic(activeTargets);

        // Restamos exactamente la munición gastada y aplicamos el cooldown de 2 segundos
        _currentMissileAmmo -= missilesFired;
        _canBigFire = Time.time + _bigFireRate;

        Debug.Log($"¡Salva lanzada! Misiles gastados: {missilesFired}. Munición restante: {_currentMissileAmmo}");
        return true;
    }

    // Método para recargar munición (lo usaremos luego con los Power-Ups)
    public void AddAmmo(int amount)
    {
        _currentMissileAmmo += amount;
        _currentMissileAmmo = Mathf.Clamp(_currentMissileAmmo, 0, _maxMissileAmmo);
        Debug.Log("¡Munición recargada! Total: " + _currentMissileAmmo);
    }

    public bool TryAddAmmo(int amount)
    {
        // Si ya estamos al máximo, no recogemos nada y devolvemos false
        if (_currentMissileAmmo >= _maxMissileAmmo) 
        {
            return false; 
        }

        // Si no está al máximo, sumamos (sin pasarnos del límite) y devolvemos true
        _currentMissileAmmo = Mathf.Min(_currentMissileAmmo + amount, _maxMissileAmmo);
        
        // Aquí actualizas tu UI de munición si la tienes...
        //UpdateAmmoUI(); 

        return true;
    }

    private int FireMissileSalvoLogic(List<Transform> activeTargets)
    {
        if (_missilePrefab == null) return 0;

        // Separamos los objetivos por la izquierda y derecha respecto a la posición del spawner
        List<Transform> leftSideTargets = activeTargets.FindAll(t => t.position.x < transform.position.x);
        List<Transform> rightSideTargets = activeTargets.FindAll(t => t.position.x >= transform.position.x);

        // Ordenamos cada bando por DISTANCIA al lanzador (el más cercano primero)
        leftSideTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
        rightSideTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));

        Transform leftTarget = null;
        Transform rightTarget = null;

        if (leftSideTargets.Count > 0)
        {
            leftTarget = leftSideTargets[0];
        }
        else
        {
            activeTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
            leftTarget = activeTargets[0];
        }

        if (rightSideTargets.Count > 0)
        {
            rightTarget = rightSideTargets[0];
        }
        else
        {
            activeTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
            rightTarget = activeTargets[0];
        }

        if (activeTargets.Count == 1)
        {
            leftTarget = activeTargets[0];
            rightTarget = activeTargets[0];
        }

        // DISPARAMOS Y CONTAMOS: Sumamos los misiles que salgan del bando izquierdo y derecho
        int leftFired = SpawnPair(_leftSpawnPoints, leftTarget);
        int rightFired = SpawnPair(_rightSpawnPoints, rightTarget);

        return leftFired + rightFired;
    }

    private int SpawnPair(List<Transform> spawnPoints, Transform target)
    {
        if (spawnPoints == null || target == null) return 0;

        Transform containerTransform = MissilesContainer.Instance != null ? MissilesContainer.Instance.transform : null;
        int count = 0;

        foreach (Transform sp in spawnPoints)
        {
            if (sp == null) continue;

            GameObject missileInstance = Instantiate(_missilePrefab, sp.position, sp.rotation, containerTransform);
            
            if (missileInstance.TryGetComponent<Missile>(out Missile missileScript))
            {
                IMissileGuidance arcStrategy = new SmoothArcGuidance();
                missileScript.Initialize(target, arcStrategy);
            }

            count++;
        }

        return count;
    }

    private List<Transform> FindActiveTargets()
    {
        List<Transform> validTargets = new List<Transform>();
        Entity[] foundEntities = FindObjectsByType<Entity>(FindObjectsSortMode.None);

        foreach (Entity entity in foundEntities)
        {
            if (entity != null && entity.gameObject.activeInHierarchy)
            {
                validTargets.Add(entity.transform);
            }
        }

        return validTargets;
    }
}