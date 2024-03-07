using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Linq;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks {
    [SerializeField] private TMP_Text joinCode;
    [SerializeField] private GameObject keyboard;
    [SerializeField] private GameObject cameraPrefab;

    [Header("Pages")]
    [SerializeField] private GameObject main;
    [SerializeField] private GameObject pick;
    [SerializeField] private GameObject p1;
    [SerializeField] private GameObject p2;
    [SerializeField] private GameObject code;
    [SerializeField] private GameObject scores;
    [SerializeField] private GameObject settings;

    private GameObject currentScreen;

    private void Start() {
        currentScreen = main;
    }

    public void CreateRoom() {
        string roomName = "";

        for (int i = 0; i < 5; i++) {
            roomName += Random.Range(1, 9).ToString();
        }

        PhotonNetwork.CreateRoom(roomName);
    }

    public void JoinRoom() {
        if (joinCode.text.Length != 5 || PhotonNetwork.PlayerList.Length == 2) return;
        PhotonNetwork.JoinRoom(joinCode.text);
    }

    public override void OnJoinedRoom() {
        PhotonNetwork.LoadLevel("Game");
    }

    public void Click(string num) {
        if (joinCode.text.Length > 5) joinCode.text = "";
        if (num == "x") {
            if (joinCode.text != "") joinCode.text = joinCode.text.Remove(joinCode.text.Length - 1, 1);
        } else if (joinCode.text.Length == 5) return;
        else joinCode.text += num;
    }

    public void NewPage(string page) {
        currentScreen.SetActive(false);

        switch (page) {
            case "main":
                main.SetActive(true);
                currentScreen = main;
                break;
            case "pick":
                pick.SetActive(true);
                currentScreen = pick;
                break;
            case "p1":
                p1.SetActive(true);
                currentScreen = p1;
                break;
            case "p2":
                p2.SetActive(true);
                currentScreen = p2;
                break;
            case "code":
                code.SetActive(true);
                currentScreen = code;
                break;
            case "scores":
                scores.SetActive(true);
                currentScreen = scores;
                break;
            case "settings":
                settings.SetActive(true);
                currentScreen = settings;
                break;
        }
    }
}
