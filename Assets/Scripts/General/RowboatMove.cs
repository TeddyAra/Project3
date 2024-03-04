using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowboatMove : Obstacle {
    [SerializeField] private float rowDistance;
    [SerializeField] private float rowSpeed;
    [SerializeField] private float rotationSpeed;

    public override void Spawn() {
        gameObject.SetActive(true);
    }

    private void Start() {
        StartCoroutine(moveBoat());
    }

    IEnumerator moveBoat() {
        float distance = 0f;

        while (distance < rowDistance) {
            if (paused) yield return null;
            transform.Translate(Vector3.forward * rowSpeed * Time.deltaTime);
            distance += rowSpeed * Time.deltaTime;
            yield return null;

        }

        float rotated = 0f;
        while (rotated < 90) {
            if (paused) yield return null;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            transform.Translate(Vector3.forward * rowSpeed * Time.deltaTime);
            rotated += rotationSpeed * Time.deltaTime;
            yield return null;
        }

        StartCoroutine(moveBoat());
    }
}
