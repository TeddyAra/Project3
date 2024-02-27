using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
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
        }
    }

    private void OnTriggerEnter(Collider other) {
        // Check if it's the correct port
        Debug.Log(transform.tag[transform.tag.Length - 1] + " == " + other.transform.tag[other.tag.Length - 1]);
        if (transform.tag[transform.tag.Length - 1] == other.transform.tag[other.tag.Length - 1]) {
            Debug.Log("Ping");
            // Ship is at right port
        } else {
            Debug.Log("Muuuuh");
            // Ship isn't at right port
        }

        // Map script, keeps track of the icons
        MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
        Dictionary<Transform, RectTransform> dict = mapScript.boatIcons;

        // Remove the selection if needed
        if (mapScript.selectedPos == dict[transform]) mapScript.selection.gameObject.SetActive(false);

        // Remove the icon and ship from the lists
        mapScript.boats.Remove(gameObject);
        mapScript.icons.Remove(dict[transform].gameObject);

        Destroy(dict[transform].gameObject);
        dict.Remove(transform);

        // Remove the ship
        GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);
        gameObject.SetActive(false);

    }
}
