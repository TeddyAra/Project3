using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {
    [SerializeField] private TMP_Text text;

    private void Start() {
        text.text = "Your score: " + Points.pointAmount;
    }

    public void Ready() {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }
}
