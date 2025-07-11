using KinematicCharacterController;

using System;

using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, ICharacterController
{
    [SerializeField]
    PlayerType Type;

    [SerializeField]
    CameraRig CameraRig;

    [SerializeField]
    Animator Animator;

    [SerializeField]
    InputActionAsset InputAsset;

    KinematicCharacterMotor CharacterMotor;

    TimeStream TimeStream;

    Level Level => Level.Instance;

    void Awake()
    {
        InputAsset.Enable();

        CharacterMotor = GetComponent<KinematicCharacterMotor>();
        CharacterMotor.CharacterController = this;

        MoveAction = InputAsset["Player/Move"];
        SwapTimeAction = InputAsset["Player/Swap Time"];

        CameraRig.SetTarget(this);
    }
    void Start()
    {
        //Hook Time System
        {
            var period = PlayerTypeToTimePeriod(Type);
            TimeStream = Level.TimeSystem.Streams.Evaluate(period);

            TimeStream.OnEnter += TimeStreamEnterCallback;
            TimeStream.OnExit += TimeStreamExitCallback;

            if (TimeStream.InUse)
                TimeStreamEnterCallback();
            else
                TimeStreamExitCallback();
        }
    }
    void OnDestroy()
    {
        if (TimeStream)
        {
            TimeStream.OnEnter -= TimeStreamEnterCallback;
            TimeStream.OnExit -= TimeStreamExitCallback;
        }
    }

    void OnEnable()
    {
        CameraRig.SetActive(true);
    }
    void OnDisable()
    {
        if (CameraRig) CameraRig.SetActive(false);
    }

    void Update()
    {
        CalculateGravity();

        CalculateMovement();

        CalculateLook();

        ProcessTimeSwapAction();
    }

    void TimeStreamEnterCallback()
    {
        gameObject.SetActive(true);
    }
    void TimeStreamExitCallback()
    {
        gameObject.SetActive(false);
    }

    #region Gravity
    [Header("Gravity")]

    [SerializeField]
    float GravityTerminalVelocity = 20f;
    [SerializeField]
    float GravityAcceleration = 9.8f;

    float GravityVelocity;

    void CalculateGravity()
    {
        if (CharacterMotor.GroundingStatus.IsStableOnGround)
        {
            GravityVelocity = 0f;
        }
        else
        {
            GravityVelocity = Mathf.MoveTowards(GravityVelocity, GravityTerminalVelocity, GravityAcceleration * Time.deltaTime);
        }
    }
    #endregion

    #region Move
    [Header("Move")]

    [SerializeField]
    float MoveSpeed = 5;
    [SerializeField]
    float MoveAcceleration = 20;

    InputAction MoveAction;

    Vector3 HorizontalVelocity;
    Vector3 VerticalVelocity;

    void CalculateMovement()
    {
        //Calculate Horizontal
        {
            var input = MoveAction.ReadValue<Vector2>();

            var direction = (Vector3.right * input.x) + (Vector3.forward * input.y);
            direction = Vector3.ClampMagnitude(direction * MoveSpeed, MoveSpeed);

            HorizontalVelocity = Vector3.MoveTowards(HorizontalVelocity, direction, MoveAcceleration * Time.deltaTime);
        }

        //Calculate Vertical Velocity
        {
            VerticalVelocity = Vector3.down * GravityVelocity;
        }

        ApplyMovementAnimation();
    }

    void ApplyMovementAnimation()
    {
        var speed = transform.InverseTransformDirection(CharacterMotor.BaseVelocity) / MoveSpeed;

        var move = Mathf.Abs(speed.z);

        Animator.SetFloat("Move", move);
    }

    public void UpdateVelocity(ref Vector3 current, float deltaTime)
    {
        current = HorizontalVelocity + VerticalVelocity;
    }
    #endregion

    #region Look
    [Header("Look")]

    [SerializeField]
    float LookSpeed = 360f;

    void CalculateLook() { }

    public void UpdateRotation(ref Quaternion current, float deltaTime)
    {
        var velocity = CharacterMotor.BaseVelocity;
        velocity.y = 0f;

        if (velocity.magnitude < 0.25f)
            return;

        var target = Quaternion.LookRotation(velocity);
        current = Quaternion.RotateTowards(current, target, LookSpeed * deltaTime);
    }
    #endregion

    #region Time Swap
    InputAction SwapTimeAction;

    void ProcessTimeSwapAction()
    {
        if (SwapTimeAction.WasPressedThisFrame() is false)
            return;

        Level.TimeSystem.TogglePeriod();
    }
    #endregion

    TimePeriod PlayerTypeToTimePeriod(PlayerType type) => type switch
    {
        PlayerType.Big => TimePeriod.Past,
        PlayerType.Small => TimePeriod.Future,

        _ => throw new NotImplementedException(),
    };

    #region Kinematic Character Controller Usages
    public void BeforeCharacterUpdate(float deltaTime) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void AfterCharacterUpdate(float deltaTime) { }

    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    #endregion
}

public enum PlayerType
{
    Small, Big
}