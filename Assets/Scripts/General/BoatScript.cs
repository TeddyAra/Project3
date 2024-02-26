using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatScript : MonoBehaviour {
    [SerializeField] private float turnSpeed;
    [SerializeField] private float turnTime;
    [SerializeField] private float delay;
    private bool turning;

    // Only turn if not already turning
    public void MoveBoat(bool left) {
        if (!turning) StartCoroutine(Turn(left));
    }

    IEnumerator Turn(bool left) {
        // Turn one way
        turning = true;
        float timer = 0;
        Quaternion snapRotation = transform.rotation;
        while (timer <= turnTime) {
            timer += Time.deltaTime;
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime * (left ? -1 : 1));
            yield return null;
        }

        // Wait
        while (timer <= turnTime + delay) {
            timer += Time.deltaTime;
            yield return null;
        }

        // Turn the other way
        while (timer <= turnTime * 2 + delay) { 
            timer += Time.deltaTime;
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime * (left ? 1 : -1));
            yield return null;
        }

        // Snap back to original rotation
        turning = false;
        transform.rotation = snapRotation;
        yield return null;
    }

    // Checks for collisions with obstacles
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Obstacle")) {
            gameObject.SetActive(false);
            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);
        }
    }
}
