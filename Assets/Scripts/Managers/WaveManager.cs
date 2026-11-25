/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Serializable]
    public struct SlotCoord
    {
        [Min(0)] public int row;
        [Min(0)] public int col;
    }

    [Serializable]
    public class WaveEntry
    {
        public Entity enemyPrefab;
        public int count = 8;
        public float spawnInterval = 0.2f;
        public float delayBeforeEntry = 0f;

        [Header("Figura / Posiciones Concretas (Opcional)")]
        [Tooltip("Si tiene elementos, cada enemigo intentará ocupar esa coordenada (row, col). Si está vacío, elige hueco libre.")]
        public List<SlotCoord> customSlots = new List<SlotCoord>();
        public bool fallbackToRandomFreeSlot = true;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Oleada 1";
        public float spawnY = 23.5f;
        public float delayAfterWave = 2.0f;
        public List<WaveEntry> entries = new List<WaveEntry>();
    }

    [Header("Configuración de Oleadas")]
    [SerializeField] private List<WaveDefinition> _waves = new List<WaveDefinition>();
    [SerializeField] private bool _autoStart = true;

    public int CurrentWaveIndex { get; private set; } = -1;
    public bool IsWaveActive { get; private set; }

    private int _aliveEnemiesCount;

    private void Start()
    {
        if (_autoStart)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        StopAllCoroutines();
        StartCoroutine(RunWavesRoutine());
    }

    private IEnumerator RunWavesRoutine()
    {
        for (int i = 0; i < _waves.Count; i++)
        {
            CurrentWaveIndex = i;
            IsWaveActive = true;
            WaveDefinition wave = _waves[i];

            yield return StartCoroutine(SpawnWaveRoutine(wave));

            while (_aliveEnemiesCount > 0)
            {
                yield return null;
            }

            IsWaveActive = false;

            if (wave.delayAfterWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayAfterWave);
            }
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        foreach (WaveEntry entry in wave.entries)
        {
            if (entry.enemyPrefab == null || EntityPool.Instance == null || EnemySlotManager.Instance == null)
                continue;

            if (entry.delayBeforeEntry > 0f)
            {
                yield return new WaitForSeconds(entry.delayBeforeEntry);
            }

            bool hasCustomPattern = entry.customSlots != null && entry.customSlots.Count > 0;

            for (int i = 0; i < entry.count; i++)
            {
                Vector3 targetSlot = Vector3.zero;
                bool reserved = false;

                if (hasCustomPattern && i < entry.customSlots.Count)
                {
                    SlotCoord coord = entry.customSlots[i];
                    reserved = EnemySlotManager.Instance.TryReserveSlotAt(coord.row, coord.col, out targetSlot);

                    if (!reserved && entry.fallbackToRandomFreeSlot)
                    {
                        reserved = EnemySlotManager.Instance.TryReserveSlot(out targetSlot);
                    }
                }
                else
                {
                    reserved = EnemySlotManager.Instance.TryReserveSlot(out targetSlot);
                }

                if (!reserved)
                {
                    yield return new WaitForSeconds(0.25f);
                    i--;
                    continue;
                }

                Vector3 spawnPos = new Vector3(targetSlot.x, wave.spawnY, 0f);
                Entity entity = EntityPool.Instance.Get(entry.enemyPrefab, spawnPos, Quaternion.identity);

                if (entity != null)
                {
                    _aliveEnemiesCount++;
                    entity.OnDespawned += HandleEntityDespawned;

                    if (entity.TryGetComponent<GruntEnemy>(out GruntEnemy grunt))
                    {
                        grunt.SetAnchorPoint(targetSlot);
                    }
                }

                if (entry.spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }
        }
    }

    private void HandleEntityDespawned(Entity entity)
    {
        if (entity != null)
        {
            entity.OnDespawned -= HandleEntityDespawned;
        }

        _aliveEnemiesCount = Mathf.Max(0, _aliveEnemiesCount - 1);
    }
}
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Serializable]
    public struct SlotCoord
    {
        [Min(0)] public int row;
        [Min(0)] public int col;
    }

    [Serializable]
    public class WaveEntry
    {
        public Entity enemyPrefab;
        public int count = 8;
        public float spawnInterval = 0.2f;
        public float delayBeforeEntry = 0f;

        [Header("Figura / Posiciones Concretas (Opcional)")]
        [Tooltip("Si tiene elementos, cada enemigo intentará ocupar esa coordenada (row, col). Si está vacío, elige hueco libre.")]
        public List<SlotCoord> customSlots = new List<SlotCoord>();
        public bool fallbackToRandomFreeSlot = true;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Oleada 1";
        public float spawnY = 23.5f;
        public float delayAfterWave = 2.0f;
        public List<WaveEntry> entries = new List<WaveEntry>();
    }

    [Header("Configuración de Oleadas")]
    [SerializeField] private List<WaveDefinition> _waves = new List<WaveDefinition>();
    [SerializeField] private bool _autoStart = true;

    [Header("Spawn Relativo a Cámara")]
    [SerializeField] private bool _useCameraRelativeSpawnY = true;
    [SerializeField] private float _spawnMarginTop = 3.5f;

    public int CurrentWaveIndex { get; private set; } = -1;
    public bool IsWaveActive { get; private set; }

    private int _aliveEnemiesCount;

    private void Start()
    {
        if (_autoStart)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        StopAllCoroutines();
        StartCoroutine(RunWavesRoutine());
    }

    private float GetResolvedSpawnY(WaveDefinition wave)
    {
        if (!_useCameraRelativeSpawnY)
            return wave.spawnY;

        Camera cam = Camera.main;
        if (cam == null)
            return wave.spawnY;

        return cam.orthographicSize + _spawnMarginTop;
    }

    private IEnumerator RunWavesRoutine()
    {
        for (int i = 0; i < _waves.Count; i++)
        {
            CurrentWaveIndex = i;
            IsWaveActive = true;
            WaveDefinition wave = _waves[i];

            yield return StartCoroutine(SpawnWaveRoutine(wave));

            while (_aliveEnemiesCount > 0)
            {
                yield return null;
            }

            IsWaveActive = false;

            if (wave.delayAfterWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayAfterWave);
            }
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        float resolvedSpawnY = GetResolvedSpawnY(wave);

        foreach (WaveEntry entry in wave.entries)
        {
            if (entry.enemyPrefab == null || EntityPool.Instance == null || EnemySlotManager.Instance == null)
                continue;

            if (entry.delayBeforeEntry > 0f)
            {
                yield return new WaitForSeconds(entry.delayBeforeEntry);
            }

            bool hasCustomPattern = entry.customSlots != null && entry.customSlots.Count > 0;

            for (int i = 0; i < entry.count; i++)
            {
                Vector3 targetSlot = Vector3.zero;
                bool reserved = false;

                if (hasCustomPattern && i < entry.customSlots.Count)
                {
                    SlotCoord coord = entry.customSlots[i];
                    reserved = EnemySlotManager.Instance.TryReserveSlotAt(coord.row, coord.col, out targetSlot);

                    if (!reserved && entry.fallbackToRandomFreeSlot)
                    {
                        reserved = EnemySlotManager.Instance.TryReserveSlot(out targetSlot);
                    }
                }
                else
                {
                    reserved = EnemySlotManager.Instance.TryReserveSlot(out targetSlot);
                }

                if (!reserved)
                {
                    yield return new WaitForSeconds(0.25f);
                    i--;
                    continue;
                }

                Vector3 spawnPos = new Vector3(targetSlot.x, resolvedSpawnY, 0f);
                Entity entity = EntityPool.Instance.Get(entry.enemyPrefab, spawnPos, Quaternion.identity);

                if (entity != null)
                {
                    _aliveEnemiesCount++;
                    entity.OnDespawned += HandleEntityDespawned;

                    if (entity.TryGetComponent<GruntEnemy>(out GruntEnemy grunt))
                    {
                        grunt.SetAnchorPoint(targetSlot);
                    }
                }

                if (entry.spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }
        }
    }

    private void HandleEntityDespawned(Entity entity)
    {
        if (entity != null)
        {
            entity.OnDespawned -= HandleEntityDespawned;
        }

        _aliveEnemiesCount = Mathf.Max(0, _aliveEnemiesCount - 1);
    }
}