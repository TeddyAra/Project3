using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {
    [SerializeField] private float lowAngle = 70f;
    [SerializeField] private float highAngle = 10f;
    [SerializeField] private float zoomStrength = 0.1f;
    [SerializeField] private float minFov = 20;
    [SerializeField] private float maxFov = 60;

    private Camera cam;
    private bool gyroEnabled;
    private float fingerDistance;
    private float lastDistance;

    private void Start() {
        // Keeps screen from turning off
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Enables gyro controls, if possible
        gyroEnabled = EnableGyro();

        // Gets the camera
        cam = GetComponent<Camera>();
    }

    private bool EnableGyro() {
        // Checks if the system has gyroscope functionality
        if (SystemInfo.supportsGyroscope) {
            Input.gyro.enabled = true;
            return true;   
        }
        return false;
    }

    private void Update() {
        // Don't do anything if gyro isn't available
        if (!gyroEnabled) {
            return;
        }

        ApplyRotation();
        ApplyZoom();
    }

    private void ApplyRotation() {
        // Get phone rotation 
        Quaternion phoneQuat = Input.gyro.attitude * new Quaternion(0, 0, 1, 0);

        // Apply the initial rotation
        transform.localRotation = phoneQuat;

        // Get the global rotation of the camera and clamps the angle
        Vector3 camEuler = transform.rotation.eulerAngles;

        // Clamp the rotation (Normal Mathf.Clamp results in snappy movement)
        camEuler.z = 0;
        if (camEuler.x > 180 && camEuler.x < 360 - highAngle) camEuler.x = 360 - highAngle;
        if (camEuler.x > lowAngle && camEuler.x < 180) camEuler.x = lowAngle;

        // Applies the final rotation
        transform.rotation = Quaternion.Euler(camEuler);
    }

    private void ApplyZoom() {
        if (Input.touchCount >= 2) {
            lastDistance = lastDistance == 0 ? (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude : fingerDistance;
            fingerDistance = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + (lastDistance - fingerDistance) * zoomStrength, minFov, maxFov);
        } else {
            lastDistance = 0;
        }
    }
}