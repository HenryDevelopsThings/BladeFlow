using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform camHolder;
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private float RotationX;
    [SerializeField] private float RotationY;
    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        Vector2 mouseVector = gameInput.GetMouseDelta();
        float mouseX = mouseVector.x * Time.deltaTime * sensX;
        float mouseY = mouseVector.y * Time.deltaTime * sensY;

        RotationY += mouseX;
        RotationX -= mouseY;

        RotationX = Mathf.Clamp(RotationX, -90f, 90f);
        camHolder.rotation = Quaternion.Euler(RotationX, RotationY, 0);
        orientation.rotation = Quaternion.Euler(0, RotationY, 0);
    }

    public void DoFov(float endValue) {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt) {
        transform.DOLocalRotate(new Vector3(0,0,zTilt), 0.25f);
    }
}