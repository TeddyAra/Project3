using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BoatScript : MonoBehaviour {
    [SerializeField] private float turnSpeed;
    [SerializeField] private float turnTime;
    [SerializeField] private float moveSpeed;

    // Move forward
    private void Update() {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    // Only turn if not already turning
    public void MoveBoat(bool left) {
        StopAllCoroutines();
        StartCoroutine(Turn(left));
    }

    // Turn the boat
    IEnumerator Turn(bool left) {
        Debug.Log("Turn");
        float timer = 0;

        while (timer <= turnTime) {
            timer += Time.deltaTime;
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime * (left ? -1 : 1));
            yield return null;
        }

        yield return null;
    }

    // Checks for collisions with obstacles
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Obstacle")) {
            gameObject.SetActive(false);
            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);

            MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
            GameObject icon = mapScript.boatIcons[transform].gameObject;
            mapScript.icons.Remove(mapScript.boatIcons[transform].gameObject);
            mapScript.boatIcons.Remove(transform);
            mapScript.boats.Remove(gameObject);
            mapScript.currentSelection = -1;
            Destroy(icon);

            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        MapBoats boatsScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();

        // Check if it's the correct port
        Debug.Log(transform.tag[transform.tag.Length - 1] + " == " + other.transform.tag[other.tag.Length - 1]);
        if (transform.tag[transform.tag.Length - 1] == other.transform.tag[other.tag.Length - 1]) {
            // Ship is at the right port
            Debug.Log("Step 0");
            boatsScript.PopUp("Yippee", Color.green, 4);
        } else {
            // Ship isn't at right port
            Debug.Log("Step 0");
            boatsScript.PopUp("Womp womp", Color.red, 4);
        }

        MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
        GameObject icon = mapScript.boatIcons[transform].gameObject;
        mapScript.icons.Remove(mapScript.boatIcons[transform].gameObject);
        mapScript.boatIcons.Remove(transform);
        mapScript.boats.Remove(gameObject);
        mapScript.currentSelection = -1;
        Destroy(icon);

        GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);
        Destroy(gameObject);
    }
}
