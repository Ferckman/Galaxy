using System.Collections.Generic;
using UnityEngine;

public class EnemySlotManager : MonoBehaviour
{
    public static EnemySlotManager Instance { get; private set; }

    [Header("Distribución de Huecos (Cámara Ortho = 20)")]
    [SerializeField] private int _columns = 8;
    [SerializeField] private int _rows = 3;
    [SerializeField] private float _horizontalSpacing = 4.2f;
    [SerializeField] private float _topY = 14.5f;
    [SerializeField] private float _rowSpacingY = 4.2f;

    [Header("Límite de Acciones Puntuales Simultáneas")]
    [SerializeField] private int _maxConcurrentActingGrunts = 3;

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

    public void GenerateSlots()
    {
        _slots.Clear();

        float totalWidth = (_columns - 1) * _horizontalSpacing;
        float startX = -totalWidth / 2f;

        for (int r = 0; r < _rows; r++)
        {
            float y = _topY - (r * _rowSpacingY);
            for (int c = 0; c < _columns; c++)
            {
                float x = startX + (c * _horizontalSpacing);
                _slots.Add(new Slot
                {
                    Position = new Vector3(x, y, 0f),
                    IsOccupied = false
                });
            }
        }
    }

    /// <summary>
    /// Devuelve un hueco libre y lo marca como ocupado. Retorna false si no hay hueco.
    /// </summary>
    public bool TryReserveSlot(out Vector3 slotPosition)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsOccupied)
            {
                _slots[i].IsOccupied = true;
                slotPosition = _slots[i].Position;
                return true;
            }
        }

        slotPosition = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Libera el hueco más cercano a la posición indicada cuando un enemigo muere.
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
        float totalWidth = (_columns - 1) * _horizontalSpacing;
        float startX = -totalWidth / 2f;

        Gizmos.color = Color.cyan;
        for (int r = 0; r < _rows; r++)
        {
            float y = _topY - (r * _rowSpacingY);
            for (int c = 0; c < _columns; c++)
            {
                float x = startX + (c * _horizontalSpacing);
                Gizmos.DrawWireSphere(new Vector3(x, y, 0f), 0.35f);
            }
        }
    }
}