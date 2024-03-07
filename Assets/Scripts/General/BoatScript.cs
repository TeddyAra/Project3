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
    private PhotonView view;

    private void Start() {
        view = GetComponent<PhotonView>();
    }

    // Move forward
    private void Update() {
        if (paused) return;

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (turning) {
            Debug.Log(transform.eulerAngles.y);
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime * (left ? -1 : 1));
        }
    }

    // Checks for collisions with obstacles
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Obstacle")) {
            gameObject.SetActive(false);

            if (view.IsMine) {
                Debug.Log("Removed from manager");
                GameManager manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
                
                manager.ships.Remove(gameObject);
                manager.ShipFail();
                PhotonNetwork.Destroy(gameObject);
            } else {
                Debug.Log("Removed from map");
                MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
                GameObject icon = mapScript.boatIcons[transform].gameObject;

                if (mapScript.selectedBoat == gameObject) mapScript.currentSelection = -1;
                mapScript.arrows.Remove(transform);
                mapScript.icons.Remove(mapScript.boatIcons[transform].gameObject);
                mapScript.boatIcons.Remove(transform);
                mapScript.boats.Remove(gameObject);
                Destroy(icon);
            }
        }
    }

    // Checks for triggers with ports
    private void OnTriggerEnter(Collider other) {
        if (view.IsMine) {
            Debug.Log("Removed from manager");
            GameManager manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

            manager.ships.Remove(gameObject);
            PhotonNetwork.Destroy(gameObject);
        } else {
            Debug.Log("Removed from map");
            GameManager manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
            MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
            MapBoats boatsScript = mapScript.GetComponent<MapBoats>();

            // Check if it's the correct port
            if (transform.tag[transform.tag.Length - 1] == other.transform.tag[other.tag.Length - 1]) {
                // Ship is at the right port
                boatsScript.PopUp("Correct!", Color.green, 4);
                manager.ShipSucceed();
            } else {
                // Ship isn't at right port
                boatsScript.PopUp("Incorrect...", Color.red, 4);
                manager.ShipFail();
            }

            GameObject icon = mapScript.boatIcons[transform].gameObject;

            if (mapScript.selectedBoat == gameObject) mapScript.currentSelection = -1;
            mapScript.arrows.Remove(transform);
            mapScript.icons.Remove(mapScript.boatIcons[transform].gameObject);
            mapScript.boatIcons.Remove(transform);
            mapScript.boats.Remove(gameObject);
            Destroy(icon);
        }
    }
}
