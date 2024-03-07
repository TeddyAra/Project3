using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {
    [SerializeField] private TMP_Text highscore;
    [SerializeField] private TMP_Text score;

    private void Start() {
        int newScore = PlayerPrefs.GetInt("Highscore", 0);
        if (Points.pointAmount > newScore) {
            PlayerPrefs.SetInt("Highscore", Points.pointAmount);
        }

        highscore.text = $"Your score: {highscore}";
        score.text = $"Highscore: {Points.pointAmount}";
    }

    public void Ready() {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }
}
