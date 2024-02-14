using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {
    [Range(180f, 269f)]
    [SerializeField] private float minAngle = 210;
    [Range(270, 359f)]
    [SerializeField] private float maxAngle = 330;

    private bool gyroEnabled;
    Transform parent;

    private void Start() {
        // Keeps screen from turning off
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Enables gyro controls, if possible
        gyroEnabled = EnableGyro();

        // Gets parent
        parent = transform.parent;
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

        // Get phone rotation 
        Quaternion phoneQuat = Input.gyro.attitude * new Quaternion(0, 0, 1, 0);

        // Turn phone rotation into x angle for camera
        float xAngle = Mathf.Clamp(phoneQuat.eulerAngles.y, minAngle, maxAngle);
        Vector3 camEuler = new Vector3(xAngle, 0, 0);

        // Apply phone rotation to camera
        transform.localEulerAngles = camEuler;

        parent.Rotate(Vector3.back, -Input.gyro.rotationRateUnbiased.y);

        // Get phone accelerometer
        Vector3 phoneAcc = Input.gyro.rotationRateUnbiased;
        for (int i = 0; i < 3; i++) {
            if (phoneAcc[i] > 1 || phoneAcc[i] < -1) {
                Debug.Log(i + ": " + phoneAcc[i]);
            }
        }
    }
}