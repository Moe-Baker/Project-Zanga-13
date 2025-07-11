using System;

using UnityEngine;
using UnityEngine.InputSystem;

public class BasePlayer : MonoBehaviour
{
    [SerializeField]
    CameraRig CameraRig;

    [SerializeField]
    InputActionAsset InputAsset;

    InputAction JumpAction;
    InputAction GrabAction;

    protected CharacterController CharacterController;

    void Awake()
    {
        CharacterController = GetComponent<CharacterController>();

        MoveAction = InputAsset["Player/Move"];
        JumpAction = InputAsset["Player/Jump"];
        GrabAction = InputAsset["Player/Grab"];

        CameraRig.SetTarget(this);
    }

    void OnEnable()
    {
        InputAsset.Enable();
    }
    void OnDisable()
    {
        InputAsset.Disable();
    }

    void Update()
    {
        CalculateGravity();

        ApplyMovement();

        ApplyLook();
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
}