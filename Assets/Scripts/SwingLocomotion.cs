using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(OVRCameraRig))]
public class SwingLocomotion : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float deadzone = 0.15f;
    [SerializeField] private float speedSmoothing = 8f;
    [SerializeField] private float maxSwingVelocity = 2.5f;

    [Header("Anti-phase")]
    [SerializeField, Range(0f, 1f)] private float antiphaseWeight = 0.6f;

    [Header("Gravity")]
    [SerializeField] private float gravityMultiplier = 2.0f;
    [SerializeField] private float stickToGroundForce = 10f;

    private CharacterController _cc;
    private Transform _leftHand, _rightHand, _centerEye;
    private Vector3 _leftPrevPos, _rightPrevPos;
    private float _currentSpeed, _verticalVelocity;

    // right controller Button A (Button.One) toggles locomotion on/off
    private bool _locomotionEnabled = true;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        var rig = GetComponent<OVRCameraRig>();
        _leftHand  = rig.leftHandAnchor;
        _rightHand = rig.rightHandAnchor;
        _centerEye = rig.centerEyeAnchor;
    }

    private void Start()
    {
        _leftPrevPos  = _leftHand.position;
        _rightPrevPos = _rightHand.position;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            _locomotionEnabled = !_locomotionEnabled;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // always update prev positions — prevents velocity spike when re-enabling locomotion
        Vector3 leftVel  = (_leftHand.position  - _leftPrevPos)  / dt;
        Vector3 rightVel = (_rightHand.position - _rightPrevPos) / dt;
        _leftPrevPos  = _leftHand.position;
        _rightPrevPos = _rightHand.position;

        if (_locomotionEnabled)
        {
            Vector3 headFwd = _centerEye.forward;
            headFwd.y = 0f;
            if (headFwd.sqrMagnitude < 0.001f) headFwd = transform.forward;
            headFwd.Normalize();

            float leftZ  = Vector3.Dot(leftVel,  headFwd);
            float rightZ = Vector3.Dot(rightVel, headFwd);
            float leftAbs  = Mathf.Abs(leftZ);
            float rightAbs = Mathf.Abs(rightZ);

            // both hands must move — single-hand jitter or reach-out won't trigger locomotion
            bool bothHandsActive = leftAbs > deadzone * 0.5f && rightAbs > deadzone * 0.5f;
            float rawCombined = bothHandsActive ? leftAbs + rightAbs : 0f;

            float antiphaseScore = 0f;
            if (rawCombined > deadzone * 2f)
                antiphaseScore = Mathf.Clamp01(
                    -(leftZ * rightZ) / (leftAbs * rightAbs + 0.0001f));

            float effectiveCombined = rawCombined *
                Mathf.Lerp(1f, antiphaseScore, antiphaseWeight);

            float swingFraction = 0f;
            if (effectiveCombined > deadzone)
                swingFraction = Mathf.Clamp01(
                    (effectiveCombined - deadzone) / (maxSwingVelocity - deadzone));

            _currentSpeed = Mathf.Lerp(_currentSpeed, swingFraction * maxSpeed,
                                       speedSmoothing * dt);
        }
        else
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, speedSmoothing * dt);
        }

        // recalculate heading outside the branch — needed for CC.Move regardless of toggle state
        Vector3 headFwdFinal = _centerEye.forward;
        headFwdFinal.y = 0f;
        if (headFwdFinal.sqrMagnitude < 0.001f) headFwdFinal = transform.forward;
        headFwdFinal.Normalize();

        _verticalVelocity = _cc.isGrounded
            ? -stickToGroundForce
            : _verticalVelocity + Physics.gravity.y * gravityMultiplier * dt;

        _cc.Move(new Vector3(
            headFwdFinal.x * _currentSpeed,
            _verticalVelocity,
            headFwdFinal.z * _currentSpeed) * dt);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_leftHand == null || _rightHand == null) return;
        Gizmos.color = _locomotionEnabled ? Color.green : Color.gray;
        Gizmos.DrawLine(_leftHand.position,
                        _leftHand.position + Vector3.up * _currentSpeed * 0.3f);
        Gizmos.color = _locomotionEnabled ? Color.red : Color.gray;
        Gizmos.DrawLine(_rightHand.position,
                        _rightHand.position + Vector3.up * _currentSpeed * 0.3f);
    }
#endif
}
