using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class MapBoats : MonoBehaviour, IOnEventCallback {
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float touchSize;
    [SerializeField] private RectTransform leftPad;
    [SerializeField] private float padMoveSpeed;
    [SerializeField] private float boatTranslationAmount;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectionSprite;

    [HideInInspector] public Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();
    [HideInInspector] public List<GameObject> boats = new List<GameObject>();
    [HideInInspector] public List<GameObject> icons = new List<GameObject>();
    private GameObject selectedBoat;
    [HideInInspector] public int currentSelection;
    private float touchDist;
    private PhotonView view;
    private TMP_Text codeText;
    private int prevTouches = 0;
    private bool leftShown = false;

    void Start() {
        view = GetComponent<PhotonView>();
        icons = GameObject.FindGameObjectsWithTag("Icon").ToList();
        codeText = GameObject.FindGameObjectWithTag("Text").GetComponent<TMP_Text>();
    }

    private void OnEnable() {
        Debug.Log("OnEvent() Enabled");
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable() {
        Debug.Log("OnEvent() Disabled");
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
                newIcon.transform.SetSiblingIndex(newIcon.transform.GetSiblingIndex() - 2);
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
                    newIcon.transform.SetSiblingIndex(newIcon.transform.GetSiblingIndex() - 2);
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
            boatIcon.Value.anchoredPosition = new Vector3(boatIcon.Key.position.x * boatTranslationAmount, boatIcon.Key.position.z * boatTranslationAmount, 0);
        }

        // If the player is touching the screen
        if (Input.touchCount == 1 && prevTouches == Input.touchCount - 1) {
            touchDist = touchSize;
            int num = -1;

            for (int i = 0; i < boatIcons.Count; i++) {
                // Check if boat icon has been touched
                Vector2 iconPos = new Vector2(boatIcons.ElementAt(i).Value.position.x, boatIcons.ElementAt(i).Value.position.y);
                if ((Input.GetTouch(0).position - iconPos).magnitude < touchDist) {
                    touchDist = (Input.GetTouch(0).position - iconPos).magnitude;
                    num = i;
                }
            }

            if (num != -1) {
                Select(num);
                Debug.Log($"Icon {num} selected");
            }
        }

        // Keep track of the last frame's touches
        prevTouches = Input.touchCount;
    }

    // Select a boat
    public void Select(int num) {
        Debug.Log("Num: " + num);
        if (currentSelection != -1) icons[currentSelection].gameObject.GetComponent<UnityEngine.UI.Image>().sprite = normalSprite;
        selectedBoat = boats[num];
        icons[num].gameObject.GetComponent<UnityEngine.UI.Image>().sprite = selectionSprite;
        currentSelection = num;
    }
    
    // Move a boat
    public void MoveBoat(bool left) {
        selectedBoat.GetComponent<BoatScript>().MoveBoat(left);
    }

    public void PopUp(string text, Color color, float time) { 
        Debug.Log("Step 1");
        StartCoroutine(PopUpCor(text, color, time));
    }

    // Shows a pop up message on screen
    public IEnumerator PopUpCor(string text, Color color, float time) {
        Debug.Log("Step 2");
        float timer = 0;
        codeText.text = text;
        codeText.color = color;

        while (timer < time) {
            Debug.Log("Waiting");
            timer += Time.deltaTime;
            yield return null;
        }

        codeText.text = "";
    }

    // Moves the left padding containing boat information to the left or right
    public void MovePadLeft() {
        //StartCoroutine(MovePadLeftCor());
        leftPad.localPosition = new Vector2(leftPad.localPosition.x + (leftPad.sizeDelta.x * (leftShown ? -1 : 1)), 0);
        leftShown = !leftShown;
        Debug.Log("Clicked");
    }

    IEnumerator MovePadLeftCor() {
        float originalPos = leftPad.localPosition.x;

        while (leftPad.localPosition.x > originalPos - leftPad.sizeDelta.x) {
            leftPad.localPosition = new Vector2(0, 0);
            yield return null;
        }

        leftShown = !leftShown;
    }
}