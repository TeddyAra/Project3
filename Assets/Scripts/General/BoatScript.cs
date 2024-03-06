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

    [HideInInspector] public bool turning;
    [HideInInspector] public bool left;
    [HideInInspector] public bool paused;

    // Move forward
    private void Update() {
        if (paused) return;

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (turning) {
            
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime * (left ? -1 : 1));
        }
    }

    // Checks for collisions with obstacles
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Obstacle")) {
            gameObject.SetActive(false);
            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);

            MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
            GameManager manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

            GameObject icon = mapScript.boatIcons[transform].gameObject;
            mapScript.arrows.Remove(transform);
            mapScript.icons.Remove(mapScript.boatIcons[transform].gameObject);
            mapScript.boatIcons.Remove(transform);
            mapScript.boats.Remove(gameObject);
            mapScript.currentSelection = -1;
            Destroy(icon);

            manager.ShipFail();
            manager.ships.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    // Checks for triggers with ports
    private void OnTriggerEnter(Collider other) {
        MapBoats boatsScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
        GameManager manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

        // Check if it's the correct port
        Debug.Log(transform.tag[transform.tag.Length - 1] + " == " + other.transform.tag[other.tag.Length - 1]);
        if (transform.tag[transform.tag.Length - 1] == other.transform.tag[other.tag.Length - 1]) {
            // Ship is at the right port
            boatsScript.PopUp("Yippee", Color.green, 4);
            manager.ShipSucceed();
        } else {
            // Ship isn't at right port
            boatsScript.PopUp("Womp womp", Color.red, 4);
            manager.ShipFail();
        }

        MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
        GameObject icon = mapScript.boatIcons[transform].gameObject;
        mapScript.arrows.Remove(transform);
        mapScript.icons.Remove(mapScript.boatIcons[transform].gameObject);
        mapScript.boatIcons.Remove(transform);
        mapScript.boats.Remove(gameObject);
        mapScript.currentSelection = -1;
        Destroy(icon);

        manager.ships.Remove(gameObject);
        Destroy(gameObject);
    }
}
