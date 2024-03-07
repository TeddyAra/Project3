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

public class MapBoats : MonoBehaviour, IOnEventCallback {

    [SerializeField] private AudioSource audioSource; 
    public AudioClip pageTurn; 

    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float touchSize;
    [SerializeField] private RectTransform leftPad;
    [SerializeField] private float padMoveSpeed;
    [SerializeField] private float boatTranslationAmount;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectionSprite;
    [SerializeField] private CustomButton turnLeft;
    [SerializeField] private CustomButton turnRight;
    [SerializeField] private float guideMoveAmount;

    [SerializeField] private GameObject tab1; 
    [SerializeField] private GameObject tab2; 
    [SerializeField] private GameObject tab3; 

    [HideInInspector] public Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();
    [HideInInspector] public List<GameObject> boats = new List<GameObject>();
    [HideInInspector] public List<GameObject> icons = new List<GameObject>();
    [HideInInspector] public Dictionary<Transform, RectTransform> arrows = new Dictionary<Transform, RectTransform>();
    [HideInInspector] public GameObject selectedBoat;
    [HideInInspector] public int currentSelection;
    private float touchDist;
    private PhotonView view;
    private TMP_Text codeText;
    private int prevTouches = 0;
    private bool leftShown = false;
    private GameManager manager;

    [HideInInspector] public GameObject announcement;
    [HideInInspector] public TMP_Text announcementText;
    private List<string> futureTexts = new List<string>();
    private bool checkLeft = false;

    void Start() {
        view = GetComponent<PhotonView>();
        icons = GameObject.FindGameObjectsWithTag("Icon").ToList();
        codeText = GameObject.FindGameObjectWithTag("Text").GetComponent<TMP_Text>();
        manager = FindObjectOfType<GameManager>();
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
        if (photonEvent.Code > 100) return;

        Debug.Log($"Event received with code {photonEvent.Code}");

        // Check if the event is for a new ship
        switch (photonEvent.Code) {
            case GameManager.NewShip:
                NewShip();
                break;
            case GameManager.TutorialShip:
                NewShip();
                break;
            case GameManager.Announcement:
                string[] data = (string[])photonEvent.CustomData;

                ShowAnnouncement();
                Announce(data[0]);
                Debug.Log("Announcement: " + data[0]);

                futureTexts.Clear();
                if (data.Length > 1) {
                    for (int i = 0; i < data.Length; i++) {
                        futureTexts.Add(data[i]);
                    }
                }
                break;
            case GameManager.HideText:
                HideWaiting();
                break;
            case GameManager.CheckForLeft:
                checkLeft = true;
                break;
            case GameManager.NextClicked:
                Next();
                break;
            case GameManager.CheckForReady:
                
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
        /*if (boats.Count == 0) {
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
        }*/

        bool selecting = false;
        if (boats.Count == 0) selecting = true;

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

        if (selecting) {
            Select(0);
        }
    }

    public void Next() {
        Debug.Log("Next clicked");
        if (futureTexts.Count == 0) {
            SendEvent(GameManager.TaskDone);
            HideAnnouncement();
            manager.playerReady.text = "Waiting for the other player...";
        } else {
            Announce(futureTexts[0]);
            futureTexts.RemoveAt(0);
        }
    }

    private void Announce(string text) {
        announcementText.text = text;
    }

    private void ShowAnnouncement() {
        announcement.SetActive(true);
    }

    private void HideAnnouncement() {
        announcement.SetActive(false);
    }

    private void HideWaiting() {
        manager.playerReady.text = "";
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
        /*if (!manager.gameStarted) {
            SendEvent(left ? GameManager.StartTurnLeft : GameManager.StartTurnRight);
            return;
        }

        BoatScript script = selectedBoat.GetComponent<BoatScript>();
        script.turning = true;
        script.left = left;
        Debug.Log("Started turning");*/

        Debug.Log("Start turning " + (left ? "left" : "right"));
        SendEvent(left ? GameManager.StartTurnLeft : GameManager.StartTurnRight);
        return;
    }

    public void StopTurn() {
        /*if (!manager.gameStarted) {
            SendEvent(GameManager.StopTurn);
            return;
        }

        selectedBoat.GetComponent<BoatScript>().turning = false;
        Debug.Log("Stopped turning");*/

        Debug.Log("Stop turning");
        SendEvent(GameManager.StopTurn);
        return;
    }

    // Select a boat
    public void Select(int num) {
        if (currentSelection != -1) icons[currentSelection].gameObject.GetComponent<UnityEngine.UI.Image>().sprite = normalSprite;
        selectedBoat = boats[num];
        icons[num].gameObject.GetComponent<UnityEngine.UI.Image>().sprite = selectionSprite;
        currentSelection = num;
        SendEvent(GameManager.NewSelection, selectedBoat.GetComponent<BoatScript>().view.ViewID);
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
        MovePadSound();  
        leftPad.localPosition = new Vector2(leftPad.localPosition.x + (guideMoveAmount * (leftShown ? -1 : 1)), 0);
        leftShown = !leftShown;

        if (checkLeft) {
            checkLeft = false;
            SendEvent(GameManager.TaskDone);
        }
    }

    public void MovePadSound()
    {
        audioSource.PlayOneShot(pageTurn);
    }

    private void SendEvent(byte code, int photonid = -1) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = manager.singlePlayerTest ? ReceiverGroup.All : ReceiverGroup.Others };
        if (PhotonNetwork.RaiseEvent(code, photonid, raiseEventOptions, SendOptions.SendReliable)) Debug.Log($"Event sent with code {code}");
    }

    public void ChangeTab(int num)
    {
        tab1.SetActive(false); 
        tab2.SetActive(false);
        tab3.SetActive(false);

        Debug.Log("hello"); 

        switch (num)
        {
            case 1: 
                tab1.SetActive(true); 
                break;

            case 2:
                tab2.SetActive(true);
                break; 
            case 3: 
                tab3.SetActive(true);
                break; 
        }
    }
}