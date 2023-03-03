using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Dashing : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCam;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Player pm;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private JuiceController juiceController;
    [Header("Dashing")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpwardForce;
    [SerializeField] private float maxDashYSpeed;
    [SerializeField] private float dashDuration;
    [Header("CameraEffects")]
    [SerializeField] private PlayerCam cam;
    [SerializeField] private float dashFov;
    [Header("Settings")]
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private bool allowAllDirections = true;
    [SerializeField] private bool disableGravity = false;
    [SerializeField] private bool resetVel = true;
    [Header("Cooldown")]
    [SerializeField] private float dashCd;
    [SerializeField] VisualEffect smokePoof;
    private float dashCdTimer;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<Player>();
    }

    private void Update() {
        if (gameInput.LeftClick() && juiceController.GetJuice() > 0) {
            Dash();
        }

        if (dashCdTimer > 0) dashCdTimer -= Time.deltaTime;
    }

    private void Dash() {
        
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        juiceController.TakeJuice(20);

        cam.DoFov(dashFov);
        SpawnSmokePoof();
        
        Transform forwardT;

        if (useCameraForward) forwardT = playerCam;
        else forwardT = orientation;

        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity) rb.useGravity = false;

        delayedForceToApply = forceToApply;

        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;

    private void DelayedDashForce() {

        if (resetVel) rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash() {
        pm.dashing = false;
        pm.maxYSpeed = 0;

        cam.DoFov(85f);

        if (disableGravity) rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT) {
        Vector2 vector = gameInput.GetMovementVector();

        Vector3 direction = new Vector3();
        if (allowAllDirections) direction = forwardT.forward * vector.y + forwardT.right * vector.x;
        else direction = forwardT.forward;
        if (vector.y == 0 && vector.x == 0) direction = forwardT.forward;
        return direction.normalized;
    }

    void SpawnSmokePoof() {
        VisualEffect newSmokePoof = Instantiate(smokePoof, transform.position, transform.rotation);
        newSmokePoof.Play();
        Destroy(newSmokePoof.gameObject, 1.5f);
    }
}
