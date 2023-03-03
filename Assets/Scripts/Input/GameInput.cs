using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] private PlayerInputActions playerInputActions;
    private bool sprinting = false;
    private bool crouching = false;
    private bool sliding = false;
    private bool swinging = false;
    
    private void OnEnable() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Sprint.performed += x => SprintPressed();
        playerInputActions.Player.Sprint.canceled += x => SprintReleased();
        playerInputActions.Player.Crouching.performed += x => CrouchingPressed();
        playerInputActions.Player.Crouching.canceled += x => CrouchingReleased();
        playerInputActions.Player.Sliding.performed += x => SlidingPressed();
        playerInputActions.Player.Sliding.canceled += x => SlidingReleased();
        playerInputActions.Player.Swinging.performed += x => SwingingPressed();
        playerInputActions.Player.Swinging.canceled += x => SwingingReleased();
    }

    private void OnDisable() {
        playerInputActions.Player.Disable();
    }

    // MOVEMENT

    public Vector2 GetMovementVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    // JUMPING

    public bool Jump() {
        return playerInputActions.Player.Jump.triggered;
    }

    // SPRINTING

    public void SprintPressed() {
        sprinting = true;
    }

    public void SprintReleased() {
        sprinting = false;
    }

    public bool IsSprinting() {
        return sprinting;
    }

    // CROUCHING

    public void CrouchingPressed() {
        crouching = true;
    }

    public void CrouchingReleased() {
        crouching = false;
    }

    public bool IsCrouching() {
        return crouching;
    }

    // SLIDING

    public void SlidingPressed() {
        sliding = true;
    }

    public void SlidingReleased() {
        sliding = false;
    }

    public bool IsSliding() {
        return sliding;
    }

    // SWINGING

    public void SwingingPressed() {
        swinging = true;
    }

    public void SwingingReleased() {
        swinging = false;
    }

    public bool IsSwinging() {
        return swinging;
    }

    // MOUSE MOVEMENT
    
    public Vector2 GetMouseDelta() {
        return playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    // LEFT CLICKING

    public bool LeftClick() {
        return playerInputActions.Player.LeftClick.triggered;
    }
}
