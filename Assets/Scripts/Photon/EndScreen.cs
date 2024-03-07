using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {
    [SerializeField] private TMP_Text text;

    private void Start() {
        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        if (Points.pointAmount > highscore) {
            PlayerPrefs.SetInt("Highscore", Points.pointAmount);
        }

        text.text = $"Your score: {Points.pointAmount}\nHighscore: {highscore}";
    }

    public void Ready() {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }
}
