using SlingWheel.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    #region Serialized Variables

    [SerializeField, Tooltip("Rotating amount in degrees per sec")]
    private float _driftingSpeedAngular = 36f;
    [SerializeField, Tooltip("Rotating limit when drifting")]
    private float _driftAngleLimit = 25f;
    [SerializeField, TagSelector]
    private string GROUND_TAG = "StraightRoad";

    #endregion

    #region Private Variables

    private float _speed;
    private float _driftAccumulation = 0f;

    private RaycastHit _hitInfo;
    private Transform _lastRoad;
    private bool _isOnStraightRoad = true;

    private TrailRenderer _trailRenderer;

    // Hook Related things
    private HookPointDetector _hookPointDetector;
    private bool CanHook => _hookPointDetector.HookPoint.HasValue;
    public Vector3 HookPoint => _hookPointDetector.HookPoint.Value;
    private CornerOrientation HookPointOrientation => _hookPointDetector.Orientation;
    public bool IsHooked => _isHookStarted;
    private bool _isHookStarted = false;

    // Level Finish Animation variables
    private float _FinishAnimationTime = 2f;
    private bool _isLevelUpAnimationActive = false;


#if UNITY_EDITOR || UNITY_STANDALONE
    private bool IsTouching => Input.GetMouseButton(0);
    private bool IsTouchUp => Input.GetMouseButtonUp(0);
#else
    private bool IsTouching => Input.touchCount > 0;
    private bool IsTouchUp => Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended;
#endif

    #endregion

    public void Init()
    {
        _speed = GlobalConfigs.CarStartSpeed;
        _driftAngleLimit = GlobalConfigs.DriftLimitAngle;
        _driftingSpeedAngular = GlobalConfigs.DriftSpeedAngular;
        _FinishAnimationTime = GlobalConfigs.LevelFinishAnimationTimeInSec;

        _hookPointDetector = GetComponentInChildren<HookPointDetector>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
        GameManager.Instance.LevelFinishedEvent += OnLevelFinished;
    }

    private float _rotatingTimer = 0f;
    private float _driftStartAngle = 0f;
    private float _driftTargetAngle = 0f;

    private void Update()
    {
        if (IsTouchUp)
        {
            OnTouchUp();
        }

        if (IsTouching && CanHook)
        {
            Hook();
            return;
        }

        if (_isOnStraightRoad)
        {
            RecoverFromDrift();

            if (!_isLevelUpAnimationActive)
            {
                transform.position += transform.forward * _speed * Time.deltaTime;
            }
        }
        else
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

    }

    private void FixedUpdate()
    {
        _isOnStraightRoad = false;
        // Detect if on the straigt road or not
        if (Physics.Raycast(transform.position + transform.forward * 2f, -transform.up, out _hitInfo)) 
        {
            //Debug.Log("AAAA"+_hitInfo.collider.tag);
            if (_hitInfo.transform.CompareTag(GROUND_TAG))
            {
                _lastRoad = _hitInfo.transform;
                _isOnStraightRoad = true;
            }
        }
    }

    #region Private Functions

    private void RecoverFromDrift()
    {
        // while not drifting movement direction should be parallel to the road the car is on
        // we should recover the car direction to the road direction
        if (Mathf.Abs(_driftAccumulation) > 0f)
        {
            // Drift End
            _driftAccumulation = 0f;
            _driftTargetAngle = _lastRoad.eulerAngles.y;
            //Debug.Log($"Drift target angle: {_driftTargetAngle}, Drift start angle: {_driftStartAngle}");
            if (_driftStartAngle > 180f && _driftTargetAngle < 180f)
            {
                _driftTargetAngle = _driftTargetAngle + 360f;
            }

            _rotatingTimer = 0f;
        }

        _rotatingTimer += Time.deltaTime * 0.25f;
        if (_rotatingTimer > 1.0f)
        {
            // Recover End
            _rotatingTimer = 1.0f;
            _driftAccumulation = 0f;
        }

        if (_rotatingTimer <= 1.0f)
        {
            transform.rotation = Quaternion.Euler(0f, UtilityMethods.EaseOutElastic(_driftStartAngle, _driftTargetAngle, _rotatingTimer), 0f);
        }

    }

    private void OnTouchUp()
    {
        if (_isHookStarted)
        {
            _isHookStarted = false;
            OnHookEnd();
        }
    }

    private void Hook()
    {
        if (!_isHookStarted)
        {
            _isHookStarted = true;
            OnHookStart();
        }

        // if sign is positive it means corner is turn left else right
        float sign = (float)HookPointOrientation;
        // Rotate around the hook point
        float radius = Vector3.Distance(HookPoint, transform.position);
        if(radius < 15f)
        {
            transform.RotateAround(HookPoint, Vector3.up, 1.2f*_speed * Time.deltaTime * Mathf.PI * sign);
        }
        else
        {
            transform.RotateAround(HookPoint, Vector3.up, 1.1f * _speed * Time.deltaTime * Mathf.PI * sign);
        }

        // Rotate around self for drifting until it reaches limit angle
        float nextAcc = _driftAccumulation + _driftingSpeedAngular * sign * Time.deltaTime;
        if(Mathf.Abs(nextAcc) < _driftAngleLimit)
        {
            transform.Rotate(Vector3.up, _driftingSpeedAngular * sign * Time.deltaTime);
            _driftAccumulation = nextAcc;
        }
    }

    private void OnHookStart()
    {
        _trailRenderer.emitting = true;
    }

    private void OnHookEnd()
    {
        _trailRenderer.emitting = false;
        _driftStartAngle = transform.eulerAngles.y;
        //Debug.Log("Drift Accumulation: " + _driftAccumulation);
    }

    private void OnLevelFinished(LevelFinishedEventArgs args)
    {
        _speed += GlobalConfigs.CarSpeedIncreasePerLevel;
        StartCoroutine(LevelFinishAnimation());
    }

    private IEnumerator LevelFinishAnimation()
    {
        Vector3 animationStartPos = transform.position;
        Vector3 animationFinishPos = _lastRoad.parent.Find("EndPoint").position - (_lastRoad.forward*40f);
        animationFinishPos.y = transform.position.y;
        float t = 0;
        _isLevelUpAnimationActive = true;
        while (t <= 1)
        {
            transform.position = Vector3.Lerp(animationStartPos, animationFinishPos, t );
            t = t + (Time.deltaTime / _FinishAnimationTime);
            yield return new WaitForEndOfFrame();
        }
        _isLevelUpAnimationActive = false;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Car Forward
        Vector3 startDir = Quaternion.Euler(0f, _driftStartAngle, 0f) * Vector3.up;
        GizmosFs.DrawArrowDir(transform.position + transform.up*5f, startDir, 3f);

        Gizmos.color = Color.red;
        Vector3 targetDir = Quaternion.Euler(0f, _driftTargetAngle, 0f) * Vector3.up;
        GizmosFs.DrawArrowDir(transform.position + transform.up * 5f, targetDir, 3f);

        Gizmos.color = Color.red;
        if(_lastRoad)
            GizmosFs.DrawArrow(transform.position, _lastRoad.position + (_lastRoad.forward * _lastRoad.localScale.z * 10f)/2f);

        Gizmos.DrawSphere(transform.position + transform.forward * 2, .5f);
    }

}
