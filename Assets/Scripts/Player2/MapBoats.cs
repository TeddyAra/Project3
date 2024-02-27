using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapBoats : MonoBehaviour, IOnEventCallback {
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float touchSize;

    private Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();
    private List<GameObject> boats = new List<GameObject>();
    private List<GameObject> icons = new List<GameObject>();
    private RectTransform selection;
    private int selectedNum;
    private RectTransform selectedPos;
    private PhotonView view;

    void Start() {
        view = GetComponent<PhotonView>();
        icons = GameObject.FindGameObjectsWithTag("Icon").ToList();
    }

    private void OnEnable() {
        Debug.Log("Enabled");
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable() {
        Debug.Log("Disabled");
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // An event has been received
    public void OnEvent(EventData photonEvent) {
        Debug.Log($"Event received with key {photonEvent.Code} ({GameManager.NewShip})");
        // Check if the event is for a new ship
        if (true) {
            Debug.Log("New ship event");
            // Get all ships and check if they are already in the boats list
            GameObject[] newBoats = GameObject.FindGameObjectsWithTag("Boat");

            // If this is the first ship
            if (boats.Count == 0) {
                // Add the boat to the boats list
                boats.Add(newBoats[0]);

                // Make a new icon for the ship
                GameObject newIcon = Instantiate(iconPrefab, Vector3.zero, Quaternion.identity);
                newIcon.transform.SetParent(transform);
                icons.Add(newIcon);
                boatIcons.Add(newBoats[0].transform, newIcon.GetComponent<RectTransform>());

                Debug.Log("First icon created");
                return;
            }

            // If this isn't the first ship
            foreach (GameObject boat in newBoats) {
                // If boat isn't already being tracked
                if (!boats.Contains(boat)) {
                    // Add the boat to the boats list
                    boats.Add(boat);

                    // Make a new icon for the ship
                    GameObject newIcon = Instantiate(iconPrefab, Vector3.zero, Quaternion.identity);
                    newIcon.transform.SetParent(transform);
                    icons.Add(newIcon);
                    boatIcons.Add(boat.transform, newIcon.GetComponent<RectTransform>());

                    Debug.Log("New icon created");
                    return;
                }
            }
        }
    }

    void Update() {
        // Update all icon positions
        foreach (var boatIcon in boatIcons) {
            boatIcon.Value.anchoredPosition = new Vector3(boatIcon.Key.position.x * 10, boatIcon.Key.position.z * 10, 0);
        }

        // If the player is touching the screen
        if (Input.touchCount > 0) { 
            for (int i = 1; i < boatIcons.Count; i++) {
                // Check if boat icon has been touched
                if ((Input.GetTouch(0).position - boatIcons.ElementAt(i).Value.anchoredPosition).magnitude < touchSize) {
                    Select(i - 1);
                    Debug.Log($"Icon {i - 1} selected");
                }
            }
        }

        // Update the position of the selection shape
        if (selection != null) selection.anchoredPosition = selectedPos.anchoredPosition;
    }

    // Select a boat
    public void Select(int num) {
        selectedNum = num;
        selectedPos = icons[num + 1].GetComponent<RectTransform>();
    }
    
    // Move a boat
    public void MoveBoat(bool left) {
        boats[selectedNum].GetComponent<BoatScript>().MoveBoat(left);
    }
}