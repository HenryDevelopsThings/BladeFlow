using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    [SerializeField] private LayerMask wall;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    [SerializeField] private float maxWallRunTime;
    [SerializeField] private float wallClimbSpeed;
    private float wallRunTimer;
    [Header("Detection")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    [SerializeField] private bool wallLeft;
    [SerializeField] private bool wallRight;
    [Header("Exiting")]
    private bool exitingWall;
    [SerializeField] private float exitWallTime;
    private float exitWallTimer;
    [Header("Gravity")]
    [SerializeField] private bool useGravity;
    [SerializeField] private float gravityCounterForce;
    [Header("Refrences")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerCam cam;
    [SerializeField] private Player pm;
    [SerializeField] private Rigidbody rb;
    [Header("Input")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private bool upwardsRunning;
    [SerializeField] private bool downwardsRunning;
    private Vector2 inputVector;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<Player>();
    }

    private void Update() {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate() {
        if (pm.wallrunning) WallRunningMovement();
    }

    private void CheckForWall() {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wall);
    }

    private bool AboveGround() {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, ground);
    }

    private void StateMachine() {
        inputVector = gameInput.GetMovementVector();

        upwardsRunning = gameInput.IsSliding();
        downwardsRunning = gameInput.IsCrouching();

        if ((wallLeft || wallRight) && inputVector.y > 0 && AboveGround() && !exitingWall) {
            if (!pm.wallrunning) StartWallRun();

            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && pm.wallrunning) {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            // WALL JUMP
            if (gameInput.Jump()) WallJump();
        }

        // EXITING
        else if (exitingWall) {
            if (pm.wallrunning) StopWallRun();
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0) exitingWall = false;
        }
        else {
            if (pm.wallrunning) StopWallRun();
        }
    }

    private void StartWallRun() {
        pm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Apply camera effects
        cam.DoFov(90f);
        if (wallLeft) cam.DoTilt(-5f);
        if (wallRight) cam.DoTilt(5f);
    }

    private void WallRunningMovement() {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (upwardsRunning) rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning) rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        if (!(wallLeft && inputVector.x > 0) && !(wallRight && inputVector.x < 0)) rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity) rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun() {
        pm.wallrunning = false;
        // Reset camera effects
        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    private void WallJump() {

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // Reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
