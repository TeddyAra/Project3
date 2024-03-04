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
//using static UnityEditor.Timeline.TimelinePlaybackControls;

public class MapBoats : MonoBehaviour, IOnEventCallback {
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float touchSize;
    [SerializeField] private RectTransform leftPad;
    [SerializeField] private float padMoveSpeed;
    [SerializeField] private float boatTranslationAmount;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectionSprite;
    [SerializeField] private CustomButton turnLeft;
    [SerializeField] private CustomButton turnRight;

    [HideInInspector] public Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();
    [HideInInspector] public List<GameObject> boats = new List<GameObject>();
    [HideInInspector] public List<GameObject> icons = new List<GameObject>();
    [HideInInspector] public Dictionary<Transform, RectTransform> arrows = new Dictionary<Transform, RectTransform>();
    private GameObject selectedBoat;
    [HideInInspector] public int currentSelection;
    private float touchDist;
    private PhotonView view;
    private TMP_Text codeText;
    private int prevTouches = 0;
    private bool leftShown = false;
    private int tutorialStep = 0;

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
        // Ignore some events
        if (new Byte[] { 201, 202, 206, 212 }.Contains(photonEvent.Code)) return;

        Debug.Log($"Event received with code {photonEvent.Code}");

        // Check if the event is for a new ship
        switch (photonEvent.Code) {
            case GameManager.NewShip:
                NewShip();
                break;
            case GameManager.TutorialShip:
                NewShip();
                break;
        }
    }

    private void NewShip() { 
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
            arrows.Add(newBoats[0].transform, newIcon.transform.GetChild(0).GetComponent<RectTransform>());
            //arrows.Add(newBoats[0].transform, newIcon.GetComponentInChildren<RectTransform>());
            boatIcons.Add(newBoats[0].transform, newIcon.GetComponent<RectTransform>());

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
                arrows.Add(boat.transform, newIcon.transform.GetChild(0).GetComponent<RectTransform>());
                //arrows.Add(boat.transform, newIcon.GetComponentInChildren<RectTransform>());
                boatIcons.Add(boat.transform, newIcon.GetComponent<RectTransform>());
                return;
            }
        }
    }

    void Update() {
        // Update all icon positions
        foreach (var boatIcon in boatIcons) {
            boatIcon.Value.anchoredPosition = new Vector3(boatIcon.Key.position.x * boatTranslationAmount, boatIcon.Key.position.z * boatTranslationAmount, 0);
        }

        foreach (GameObject boat in boats) {
            arrows[boat.transform].rotation = Quaternion.Euler(0, 0, -boat.transform.eulerAngles.y);
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

    public void StartTurn(bool left) {
        selectedBoat.GetComponent<BoatScript>().turning = true;
        selectedBoat.GetComponent<BoatScript>().left = left;
    }

    public void StopTurn() {
        selectedBoat.GetComponent<BoatScript>().turning = false;
    }

    // Select a boat
    public void Select(int num) {
        if (currentSelection != -1) icons[currentSelection].gameObject.GetComponent<UnityEngine.UI.Image>().sprite = normalSprite;
        selectedBoat = boats[num];
        icons[num].gameObject.GetComponent<UnityEngine.UI.Image>().sprite = selectionSprite;
        currentSelection = num;
    }

    public void PopUp(string text, Color color, float time) { 
        StartCoroutine(PopUpCor(text, color, time));
    }

    // Shows a pop up message on screen
    public IEnumerator PopUpCor(string text, Color color, float time) {
        float timer = 0;
        codeText.text = text;
        codeText.color = color;

        while (timer < time) {
            timer += Time.deltaTime;
            yield return null;
        }

        codeText.text = "";
    }

    // Moves the left padding containing boat information to the left or right
    public void MovePadLeft() {
        leftPad.localPosition = new Vector2(leftPad.localPosition.x + (leftPad.sizeDelta.x * (leftShown ? -1 : 1)), 0);
        leftShown = !leftShown;

        if (tutorialStep == 0) {
            tutorialStep++;
            SendEvent(GameManager.TaskDone);
        }
    }

    private void SendEvent(byte code) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        if (PhotonNetwork.RaiseEvent(code, null, raiseEventOptions, SendOptions.SendReliable)) Debug.Log($"Event sent with code {code}");
    }
}