using System.Collections;
using UnityEngine;

public class TestGrunt : MonoBehaviour
{
    [SerializeField] private GameObject _gruntPrefab;
    [SerializeField] private int _count = 20;
    [SerializeField] private float _spawnInterval = 0.15f;
    [SerializeField] private float _spawnY = 23.5f;

    private IEnumerator Start()
    {
        if (_gruntPrefab == null || EnemySlotManager.Instance == null) yield break;

        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        for (int i = 0; i < _count; i++)
        {
            if (!EnemySlotManager.Instance.TryReserveSlot(out Vector3 targetSlot))
            {
                break; // No hay más huecos libres
            }

            // Entra desde arriba alineado a la X de su hueco (o variado si prefieres)
            Vector3 spawnPos = new Vector3(targetSlot.x, _spawnY, 0f);
            GameObject obj = Instantiate(_gruntPrefab, spawnPos, Quaternion.identity, container);

            if (obj.TryGetComponent<GruntEnemy>(out GruntEnemy grunt))
            {
                grunt.SetAnchorPoint(targetSlot);
            }

            yield return new WaitForSeconds(_spawnInterval);
        }
    }
}