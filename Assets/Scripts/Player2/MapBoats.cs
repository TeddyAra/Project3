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
    public RectTransform selection;

    [HideInInspector] public Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();
    [HideInInspector] public List<GameObject> boats = new List<GameObject>();
    [HideInInspector] public List<GameObject> icons = new List<GameObject>();
    private GameObject selectedBoat;
    private float touchDist;
    [HideInInspector] public RectTransform selectedPos;
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
        if (photonEvent.Code == GameManager.NewShip) {
            // Get all ships and check if they are already in the boats list
            // Hardcoded for now, change this to be dynamic later
            GameObject[] newBoats1 = GameObject.FindGameObjectsWithTag("Boat1");
            GameObject[] newBoats2 = GameObject.FindGameObjectsWithTag("Boat2");
            GameObject[] newBoats3 = GameObject.FindGameObjectsWithTag("Boat3");
            GameObject[] newBoats = newBoats1.Concat(newBoats2).ToArray().Concat(newBoats3).ToArray();

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

                Select(0);
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
            touchDist = touchSize;
            for (int i = 0; i < boatIcons.Count; i++) {
                // Check if boat icon has been touched
                Vector2 iconPos = new Vector2(boatIcons.ElementAt(i).Value.position.x, boatIcons.ElementAt(i).Value.position.y);
                if ((Input.GetTouch(0).position - iconPos).magnitude < touchDist) {
                    touchDist = (Input.GetTouch(0).position - iconPos).magnitude;
                    Select(i);
                    Debug.Log($"Icon {i} selected");
                }
            }
        }

        // Update the position of the selection shape
        if (selectedPos != null) selection.anchoredPosition = selectedPos.anchoredPosition;
    }

    // Select a boat
    public void Select(int num) {
        selection.gameObject.SetActive(true);
        selectedBoat = boats[num];
        selectedPos = icons[num].GetComponent<RectTransform>();
    }
    
    // Move a boat
    public void MoveBoat(bool left) {
        selectedBoat.GetComponent<BoatScript>().MoveBoat(left);
    }
}