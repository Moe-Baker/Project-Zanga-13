using System;

using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    PlayerType Type;

    [SerializeField]
    CameraRig CameraRig;

    [SerializeField]
    InputActionAsset InputAsset;

    protected CharacterController CharacterController;

    TimeStream TimeStream;

    Level Level => Level.Instance;

    void Awake()
    {
        InputAsset.Enable();

        CharacterController = GetComponent<CharacterController>();

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

        ApplyMovement();

        ApplyLook();

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
    [SerializeField]
    float GravityTerminalVelocity = 20f;
    [SerializeField]
    float GravityAcceleration = 9.8f;

    float GravityVelocity;

    void CalculateGravity()
    {
        if (CharacterController.isGrounded)
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
    [SerializeField]
    float MoveSpeed = 5;
    [SerializeField]
    float MoveAcceleration = 20;

    InputAction MoveAction;

    void ApplyMovement()
    {
        var input = MoveAction.ReadValue<Vector2>();

        var velocity = (Vector3.right * input.x) + (Vector3.forward * input.y);
        velocity = Vector3.ClampMagnitude(velocity * MoveSpeed, MoveSpeed);

        velocity += Vector3.down * GravityVelocity;

        CharacterController.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region Look
    [SerializeField]
    float LookSpeed = 360f;

    void ApplyLook()
    {
        var velocity = CharacterController.velocity;
        velocity.y = 0f;

        if (velocity.magnitude < 0.5f)
            return;

        var rotation = Quaternion.LookRotation(velocity);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, LookSpeed * Time.deltaTime);
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
}

public enum PlayerType
{
    Small, Big
}