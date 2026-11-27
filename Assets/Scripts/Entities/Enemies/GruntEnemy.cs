using UnityEngine;

public class GruntEnemy : Enemy
{
    public enum GruntState
    {
        Entering,
        Settled,
        Acting
    }

    [Header("Grunt Settling & Action")]
    [SerializeField] private float _arriveThreshold = 0.2f;
    [SerializeField] private float _minActionInterval = 4f;
    [SerializeField] private float _maxActionInterval = 8f;
    private float _swoopDistanceY = 20f;
    private float _swoopSpeed = 20f;

    [Header("Idle Hover (En su sitio)")]
    [SerializeField] private float _hoverAmplitudeX = 0.45f;
    [SerializeField] private float _hoverAmplitudeY = 0.3f;
    [SerializeField] private float _hoverFrequencyX = 2.0f;
    [SerializeField] private float _hoverFrequencyY = 2.6f;

    private GruntState _state = GruntState.Entering;
    private Vector3 _anchorPosition;
    private bool _hasAnchor = false;

    private float _actionTimer = 0f;
    private bool _descendingInAction = true;
    private bool _hasActingPermission = false;

    private float _hoverTimer = 0f;
    private float _hoverPhaseOffset = 0f;

    public override void OnSpawnedFromPool()
    {
        base.OnSpawnedFromPool();
        _state = GruntState.Entering;
        _hasAnchor = false;
        _descendingInAction = true;
        _hasActingPermission = false;
        _actionTimer = 0f;
        _hoverTimer = 0f;
        _hoverPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public override void OnReturnedToPool()
    {
        ReleaseSlotAndPermission();
        base.OnReturnedToPool();
    }

    public void SetAnchorPoint(Vector3 anchorPos)
    {
        _anchorPosition = anchorPos;
        _hasAnchor = true;
        _state = GruntState.Entering;
        _descendingInAction = true;
        _actionTimer = Random.Range(_minActionInterval, _maxActionInterval);
    }

    protected override void BehaviorLogic()
    {
        if (!_hasAnchor) return;

        switch (_state)
        {
            case GruntState.Entering:
                MoveTowardPoint(_anchorPosition, _moveSpeed, () =>
                {
                    _hoverTimer = 0f;
                    _state = GruntState.Settled;
                    StartShooting();
                });
                break;

            case GruntState.Settled:
                UpdateIdleHover();

                _actionTimer -= Time.deltaTime;
                if (_actionTimer <= 0f)
                {
                    if (EnemySlotManager.Instance == null || EnemySlotManager.Instance.TryRequestActingPermission())
                    {
                        _hasActingPermission = true;
                        _descendingInAction = true;
                        _state = GruntState.Acting;
                    }
                    else
                    {
                        _actionTimer = 0.5f;
                    }
                }
                break;

            case GruntState.Acting:
                PerformSwoopAction();
                break;
        }
    }

    private void UpdateIdleHover()
    {
        _hoverTimer += Time.deltaTime;

        float offsetX = Mathf.Sin((_hoverTimer * _hoverFrequencyX) + _hoverPhaseOffset) * _hoverAmplitudeX;
        float offsetY = Mathf.Cos((_hoverTimer * _hoverFrequencyY) + _hoverPhaseOffset) * _hoverAmplitudeY;

        Vector3 targetPos = _anchorPosition + new Vector3(offsetX, offsetY, 0f);
        transform.position = Vector3.Lerp(transform.position, targetPos, 8f * Time.deltaTime);
    }

    private void PerformSwoopAction()
    {
        Vector3 swoopTarget = _anchorPosition + Vector3.down * _swoopDistanceY;

        if (_descendingInAction)
        {
            MoveTowardPoint(swoopTarget, _swoopSpeed, () => _descendingInAction = false);
        }
        else
        {
            MoveTowardPoint(_anchorPosition, _moveSpeed, () =>
            {
                if (_hasActingPermission)
                {
                    EnemySlotManager.Instance?.ReleaseActingPermission();
                    _hasActingPermission = false;
                }

                _hoverTimer = 0f;
                _state = GruntState.Settled;
                _actionTimer = Random.Range(_minActionInterval, _maxActionInterval);
            });
        }
    }

    private void MoveTowardPoint(Vector3 target, float speed, System.Action onArrive)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) <= _arriveThreshold)
        {
            transform.position = target;
            onArrive?.Invoke();
        }
    }

    private void ReleaseSlotAndPermission()
    {
        if (_hasActingPermission)
        {
            EnemySlotManager.Instance?.ReleaseActingPermission();
            _hasActingPermission = false;
        }
        if (_hasAnchor)
        {
            EnemySlotManager.Instance?.ReleaseSlotNear(_anchorPosition);
            _hasAnchor = false;
        }
    }

    public override void DestroyMyself()
    {
        ReleaseSlotAndPermission();
        base.DestroyMyself();
    }
}