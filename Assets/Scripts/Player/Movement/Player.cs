using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Player : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Vector3 moveDir;
    [Header("Movement")]
    public MovementState state;
    public float wallrunSpeed;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;
    [SerializeField] private float dashSpeed;
    public float maxYSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    [SerializeField] private float dashSpeedChangeFactor;
    private float speedChangeFactor;
    private bool desiredMoveSpeedHasChanged;
    private MovementState lastState;
    private bool keepMomentum;
    [SerializeField] private float slideSpeed;
    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private bool readyToJump;
    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    private float startYScale;
    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask ground;
    public bool grounded;
    [SerializeField] private Transform orientation;
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private RaycastHit slopeHit;
    [SerializeField] private bool exitingSlope;
    
    public enum MovementState {
        walking,
        sprinting,
        sliding,
        wallrunning,
        crouching,
        dashing,
        air
    }

    public bool dashing;
    public bool sliding;
    public bool crouching;
    public bool wallrunning;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update() {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);

        HandleInput();
        SpeedControl();
        StateHandler();

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    // MOVEMENT

    private void FixedUpdate() {
        MovePlayer();
    }

    private void StateHandler() {
        // DASHING
        if (dashing) {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        // WALLRUNNING
        else if (wallrunning) {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        // SLIDING
        else if (gameInput.IsSliding()) {
            state = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f) desiredMoveSpeed = slideSpeed;
            else desiredMoveSpeed = sprintSpeed;
        }
        // CROUCHING
        else if (gameInput.IsCrouching()) {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // SPRINTING
        else if (grounded && gameInput.IsSprinting()) {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // WALKING
        else if (grounded) {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // AIR
        else {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed) desiredMoveSpeed = walkSpeed;
            else desiredMoveSpeed = sprintSpeed;

        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged) {
            if (keepMomentum) {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeedOnDash());
            }

            else {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0) {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeedOnSlope());
        } else {
            moveSpeed = desiredMoveSpeed;
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private IEnumerator SmoothlyLerpMoveSpeedOnSlope() {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference) {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope()) {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            } 
            else time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeedOnDash() {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference) {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void MovePlayer() {

        if (state == MovementState.dashing) return;

        // MOVEMENT
        
        Vector2 inputVector = gameInput.GetMovementVector();
        moveDir = orientation.forward * inputVector.y + orientation.right * inputVector.x;
        

        // ON SLOPE

        if (OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);
            if (rb.velocity.y > 0) rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded) rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded) rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        // LIMIT SPEED ON SLOPE

        if (OnSlope()) {
            if (rb.velocity.magnitude > moveSpeed) rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // LIMIT SPEED ON GROUND OR AIR
        else {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            // LIMIT VELOCITY IF NEEDED
            if (flatVel.magnitude > moveSpeed) {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    // JUMPING

    private void Jump() {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;

        exitingSlope = false;
    }

    private void HandleInput() { 
        Vector2 inputVector = gameInput.GetMovementVector();

        // JUMP

        if (gameInput.Jump() && readyToJump && grounded) {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // CROUCHING

        if (gameInput.IsCrouching() && inputVector.x == 0 && inputVector.y ==0) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
        }

        if (!gameInput.IsCrouching()) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouching = false;
        }
    }

    // SLOPE HANDLING

    public bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 moveDir) {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }
}
