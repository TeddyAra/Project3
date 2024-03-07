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
    [SerializeField] private int points;

    [HideInInspector] public bool turning;
    [HideInInspector] public bool left;
    [HideInInspector] public bool paused;
    [HideInInspector] public PhotonView view;

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

    [PunRPC]
    private void RemoveShip() { 
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

    // Checks for collisions with obstacles
    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision entered");

        if (collision.transform.CompareTag("Obstacle")) {
            view.RPC("RemoveShip", RpcTarget.All);
        }
    }

    [PunRPC]
    private void CheckDock(string tag) { 
        if (view.IsMine) {
            Debug.Log("Removed from manager");
            GameManager manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

            if (transform.tag[transform.tag.Length - 1] == tag[tag.Length - 1]) {
                // Ship is at the right port
                manager.ShipSucceed(true);
            } else {
                // Ship isn't at right port
                manager.ShipSucceed(false);
            }

            manager.ships.Remove(gameObject);
            PhotonNetwork.Destroy(gameObject);
        } else {
            Debug.Log("Removed from map");
            MapBoats mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
            MapBoats boatsScript = mapScript.GetComponent<MapBoats>();

            // Check if it's the correct port
            if (transform.tag[transform.tag.Length - 1] == tag[tag.Length - 1]) {
                // Ship is at the right port
                boatsScript.PopUp("Correct!", Color.green, 4);
            } else {
                // Ship isn't at right port
                boatsScript.PopUp("Incorrect...", Color.red, 4);
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

    // Checks for triggers with ports
    private void OnTriggerEnter(Collider other) {
        Debug.Log("Trigger entered");
        view.RPC("CheckDock", RpcTarget.All, other.tag);
    }
}
