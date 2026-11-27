using System.Collections.Generic;
using UnityEngine;

public class EnemySlotManager : MonoBehaviour
{
    public static EnemySlotManager Instance { get; private set; }

    [Header("Distribución de Huecos (Cámara Ortho = 20)")]
    [SerializeField] private int _columns = 14;
    [SerializeField] private int _rows = 6;
    [SerializeField] private float _horizontalSpacing = 4.2f;
    [SerializeField] private float _topY = 14.5f;
    [SerializeField] private float _rowSpacingY = 4.2f;

    [Header("Ajuste Automático a Cámara")]
    [SerializeField] private bool _clampToCameraWidth = true;
    [SerializeField] private float _sideMargin = 3.0f;

    [Header("Límite de Acciones Puntuales Simultáneas")]
    [SerializeField] private int _maxConcurrentActingGrunts = 5;

    private readonly List<Slot> _slots = new List<Slot>();
    private int _currentActingCount = 0;

    private class Slot
    {
        public Vector3 Position;
        public bool IsOccupied;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GenerateSlots();
    }

    private float GetEffectiveHorizontalSpacing()
    {
        if (!_clampToCameraWidth || _columns <= 1)
            return _horizontalSpacing;

        Camera cam = Camera.main;
        if (cam == null)
            return _horizontalSpacing;

        float halfScreenWidth = cam.orthographicSize * cam.aspect;
        float maxUsableWidth = Mathf.Max(0f, (halfScreenWidth * 2f) - (_sideMargin * 2f));
        float desiredWidth = (_columns - 1) * _horizontalSpacing;

        if (desiredWidth <= maxUsableWidth)
            return _horizontalSpacing;

        return maxUsableWidth / (_columns - 1);
    }

    public void GenerateSlots()
    {
        _slots.Clear();

        float spacingX = GetEffectiveHorizontalSpacing();
        float totalWidth = (_columns - 1) * spacingX;
        float startX = -totalWidth / 2f;

        for (int r = 0; r < _rows; r++)
        {
            float y = _topY - (r * _rowSpacingY);
            for (int c = 0; c < _columns; c++)
            {
                float x = startX + (c * spacingX);
                _slots.Add(new Slot
                {
                    Position = new Vector3(x, y, 0f),
                    IsOccupied = false
                });
            }
        }
    }

    public bool TryReserveSlotAt(int row, int col, out Vector3 slotPosition)
    {
        slotPosition = Vector3.zero;

        // Convertir de 1-indexado (Inspector) a 0-indexado (array/lista interna)
        int r = row - 1;
        int c = col - 1;

        if (r < 0 || r >= _rows || c < 0 || c >= _columns)
            return false;

        int index = (r * _columns) + c;
        if (index < 0 || index >= _slots.Count)
            return false;

        Slot target = _slots[index];
        if (target.IsOccupied)
            return false;

        target.IsOccupied = true;
        slotPosition = target.Position;
        return true;
    }

    /// <summary>
    /// Devuelve un hueco libre al azar y lo marca como ocupado. Retorna false si no hay hueco.
    /// </summary>
    public bool TryReserveSlot(out Vector3 slotPosition)
    {
        List<Slot> freeSlots = new List<Slot>();

        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsOccupied)
            {
                freeSlots.Add(_slots[i]);
            }
        }

        if (freeSlots.Count == 0)
        {
            slotPosition = Vector3.zero;
            return false;
        }

        Slot chosen = freeSlots[Random.Range(0, freeSlots.Count)];
        chosen.IsOccupied = true;
        slotPosition = chosen.Position;
        return true;
    }

    /// <summary>
    /// Libera el hueco más cercano a la posición indicada cuando un enemigo muere o vuelve al pool.
    /// </summary>
    public void ReleaseSlotNear(Vector3 position)
    {
        Slot closest = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsOccupied) continue;
            float dist = Vector3.Distance(_slots[i].Position, position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = _slots[i];
            }
        }

        if (closest != null)
        {
            closest.IsOccupied = false;
        }
    }

    public bool TryRequestActingPermission()
    {
        if (_currentActingCount >= _maxConcurrentActingGrunts) return false;
        _currentActingCount++;
        return true;
    }

    public void ReleaseActingPermission()
    {
        _currentActingCount = Mathf.Max(0, _currentActingCount - 1);
    }

    private void OnDrawGizmosSelected()
    {
        float spacingX = GetEffectiveHorizontalSpacing();
        float totalWidth = (_columns - 1) * spacingX;
        float startX = -totalWidth / 2f;

        Gizmos.color = Color.cyan;
        for (int r = 0; r < _rows; r++)
        {
            float y = _topY - (r * _rowSpacingY);
            for (int c = 0; c < _columns; c++)
            {
                float x = startX + (c * spacingX);
                Gizmos.DrawWireSphere(new Vector3(x, y, 0f), 0.35f);
            }
        }
    }
}