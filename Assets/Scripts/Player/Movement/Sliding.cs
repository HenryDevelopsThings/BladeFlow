using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Player pm;
    [SerializeField] private GameInput gameInput;
    [Header("Sliding")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    private float slideTimer;
    [SerializeField] private float slideYScale;
    private float startYScale;
    [Header("Input")]
    [SerializeField] private Vector2 inputVector;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<Player>();

        startYScale = playerObj.localScale.y;
    }

    private void Update() {
        Vector2 inputVector = gameInput.GetMovementVector();
        if (gameInput.IsSliding() && (inputVector.x != 0 || inputVector.y !=0)) {
            StartSlide();
        }

        if (!gameInput.IsSliding() && pm.sliding) {
            StopSlide();
        }
    }

    private void FixedUpdate() {
        if (pm.sliding) {
            SlidingMovement();
        }
    }

    private void StartSlide() {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void StopSlide() {
        pm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    private void SlidingMovement() {
        Vector3 moveDir = orientation.forward * inputVector.y + orientation.right * inputVector.x;

        if (!pm.OnSlope() || rb.velocity.y > -0.1f) {
            rb.AddForce(moveDir.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }

        else {
            rb.AddForce(pm.GetSlopeMoveDirection(moveDir) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0) {
            StopSlide();
        }
    }
}
