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
        PhotonNetwork.LoadLevel("Engineer 1");
    }

    public void OpenKeyboard() {
        joinCode.text = "";
        keyboard.SetActive(true);
    }

    public void CloseKeyboard() { 
        keyboard.SetActive(false);
    }

    public void Click(string num) {
        if (num == "x" && joinCode.text != "") joinCode.text = joinCode.text.Remove(joinCode.text.Length - 1, 1);
        else if (joinCode.text.Length == 5) return;
        else joinCode.text += num;
    }
}
