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

    private GruntState _state = GruntState.Entering;
    private Vector3 _anchorPosition;
    private bool _hasAnchor = false;

    private float _actionTimer = 0f;
    private bool _descendingInAction = true;

    private bool _hasActingPermission = false;

    public void SetAnchorPoint(Vector3 anchorPos)
    {
        _anchorPosition = anchorPos;
        _hasAnchor = true;
        _state = GruntState.Entering;
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
                    _state = GruntState.Settled;
                    StartShooting();
                });
                break;

            case GruntState.Settled:
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
                        // Si ya hay demasiados haciendo pasada a la vez, reintenta en breve
                        _actionTimer = 0.5f;
                    }
                }
                break;

            case GruntState.Acting:
                PerformSwoopAction();
                break;
        }
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

    public override void DestroyMyself()
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
        base.DestroyMyself();
    }
}