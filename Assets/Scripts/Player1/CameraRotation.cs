using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {
    private bool gyroEnabled;

    private void Start() {
        // Keeps screen from turning off
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Enables gyro controls, if possible
        gyroEnabled = EnableGyro();
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

        // Apply the initial rotation
        transform.localRotation = phoneQuat;

        // Get the global rotation of the camera and clamps the angle
        Vector3 camEuler = transform.rotation.eulerAngles;

        // Clamp the rotation
        if (Mathf.Abs(camEuler.z - 180) < 45) {
            // Calculate the difference between the angle and the up and down angles
            float angle1 = Mathf.Abs(camEuler.x - 90);
            float angle2 = Mathf.Abs(camEuler.x - 270);

            // Depending on which angle it's closer to, set the angle
            if (angle1 < angle2) {
                camEuler.x = 90;
            } else {
                camEuler.x = 270;
            }

            // Set the z rotation to keep camera from tilting
            camEuler.z = 180;
        } else {
            camEuler.z = 0;
        }

        // Applies the final rotation
        transform.rotation = Quaternion.Euler(camEuler);
    }
}