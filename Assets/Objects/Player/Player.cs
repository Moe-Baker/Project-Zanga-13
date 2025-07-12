using KinematicCharacterController;

using NUnit.Framework.Constraints;

using System;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.GraphicsBuffer;

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
    bool IsGrounded => CharacterMotor.GroundingStatus.IsStableOnGround;

    CapsuleCollider Collider;

    TimeStream TimeStream;

    Level Level => Level.Instance;

    void Awake()
    {
        InputAsset.Enable();

        CharacterMotor = GetComponent<KinematicCharacterMotor>();
        CharacterMotor.CharacterController = this;

        Collider = GetComponent<CapsuleCollider>();

        MoveAction = InputAsset["Player/Move"];
        SwapTimeAction = InputAsset["Player/Swap Time"];
        InteractAction = InputAsset["Player/Interact"];
        JumpAction = InputAsset["Player/Jump"];

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
        ProcessJump();
        CalculateMovement();

        CalculateLook();

        ProcessTimeSwapAction();

        ProcessInteraction();
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
        if (IsGrounded || JumpActive)
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
    [SerializeField, Range(0f, 1f)]
    float MoveAccelerationAirModifier = 0.2f;

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

            var acceleration = MoveAcceleration;

            if (IsGrounded is false)
                acceleration *= MoveAccelerationAirModifier;

            HorizontalVelocity = Vector3.MoveTowards(HorizontalVelocity, direction, acceleration * Time.deltaTime);
        }

        //Calculate Vertical Velocity
        {
            VerticalVelocity = (Vector3.down * GravityVelocity) + (Vector3.up * JumpVelocity);
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

    #region Jump
    [Header("Jump")]

    [SerializeField]
    float JumpMaxForce;

    [SerializeField]
    float JumpDrag;

    [SerializeField]
    float JumpCoyoteTimeDuration = 0.15f;

    float JumpCoyoteTimeTimer;

    [SerializeField]
    float JumpInputBufferDuration = 0.2f;
    float JumpInputBufferTimer;

    InputAction JumpAction;
    bool JumpActive;
    float JumpVelocity;

    void ProcessJump()
    {
        //Coyote Time
        {
            if (IsGrounded)
                JumpCoyoteTimeTimer = JumpCoyoteTimeDuration;
            else
                JumpCoyoteTimeTimer -= Time.deltaTime;
        }

        //Input Buffer
        {
            if (JumpAction.WasPressedThisFrame())
                JumpInputBufferTimer = JumpInputBufferDuration;
            else
                JumpInputBufferTimer -= Time.deltaTime;
        }

        if (JumpActive)
        {
            JumpVelocity = Mathf.MoveTowards(JumpVelocity, 0f, JumpDrag * Time.deltaTime);

            if (Mathf.Approximately(JumpVelocity, 0f))
            {
                JumpActive = false;
                JumpVelocity = 0f;
            }
        }
        else if (HasJumpInput() && CanJump())
        {
            JumpActive = true;
            JumpVelocity = JumpMaxForce;

            JumpCoyoteTimeTimer = 0;
            JumpInputBufferTimer = 0;

            CharacterMotor.ForceUnground();
        }
    }

    bool CanJump()
    {
        if (JumpActive)
            return false;

        if (IsGroundedForJump() is false)
            return false;

        return true;
    }

    bool HasJumpInput()
    {
        return JumpInputBufferTimer > 0;
    }

    bool IsGroundedForJump()
    {
        if (IsGrounded)
            return true;

        if (JumpCoyoteTimeTimer > 0f)
            return true;

        return false;
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

    #region Interact
    [Header("Interact")]

    [SerializeField]
    float InteractionRadius = 5f;

    InputAction InteractAction;

    void ProcessInteraction()
    {
        if (IsGrabbing)
        {
            ProcessGrab();
            return;
        }

        if (CanInteract())
        {
            var hits = CheckInteractOverlap();

            if (TryFindInteractable(hits, out var item))
            {
                Level.InteractionIndicator.Activate(item);

                if (InteractAction.WasPressedThisFrame())
                    PerformInteract(item);
            }
            else
            {
                Level.InteractionIndicator.Disable();
            }
        }
        else
        {
            Level.InteractionIndicator.Disable();
        }
    }

    Collider[] InteractCache = new Collider[10];
    ArraySegment<Collider> CheckInteractOverlap()
    {
        var top = CharacterMotor.transform.position + CharacterMotor.CharacterTransformToCapsuleTop;
        var bottom = CharacterMotor.transform.position + CharacterMotor.CharacterTransformToCapsuleBottom;

        var count = Physics.OverlapCapsuleNonAlloc(top, bottom, InteractionRadius, InteractCache, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        return new ArraySegment<Collider>(InteractCache, 0, count);
    }
    bool TryFindInteractable(ArraySegment<Collider> colliders, out InteractionItem item)
    {
        var marker = (Item: default(InteractionItem), Score: float.MinValue);

        foreach (var collider in colliders)
        {
            var root = collider.GetRoot();

            if (root.TryGetComponent(out InteractionItem context) is false)
                continue;

            var score = CalculateScore(context);

            if (score > marker.Score)
                marker = (context, score);
        }

        item = marker.Item;
        return item != null;

        float CalculateScore(InteractionItem item)
        {
            var distance = Vector3.Distance(transform.position, item.transform.position);
            var angle = Vector3.Angle(transform.forward, (item.transform.position - transform.position));

            return (-distance) + (-angle);
        }
    }

    bool CanInteract()
    {
        if (IsGrabbing) return false;

        return true;
    }

    void PerformInteract(InteractionItem item)
    {
        Level.InteractionIndicator.Disable();

        item.Interact(this);

        if (item is GrabItem grabbable)
            Grab(grabbable);
    }
    #endregion

    #region Grab
    [Header("Grab")]

    [SerializeField]
    Transform GrabHeightMarker;

    [SerializeField]
    float GrabSpacing;

    GrabItem GrabItem;
    public bool IsGrabbing => GrabItem != null;

    void ProcessGrab()
    {
        if (InteractAction.WasPressedThisFrame())
        {
            Drop();
        }
    }

    void Grab(GrabItem target)
    {
        GrabItem = target;

        GrabItem.transform.parent = GrabHeightMarker.transform;
        GrabItem.transform.localPosition = Vector3.forward * (GrabItem.Radius + GrabSpacing);
        GrabItem.transform.rotation = Quaternion.identity;

        GrabItem.Pickup(this);
    }
    void Drop()
    {
        GrabItem.transform.parent = null;

        var cache = GrabItem;
        GrabItem = default;
        cache.Drop();
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
