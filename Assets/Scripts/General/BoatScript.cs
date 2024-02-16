using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatScript : MonoBehaviour {
    PhotonView view;

    private void Start() {
        view = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Rock")) {
            view.RPC("TurnOff", RpcTarget.All);
        }
    }

    [PunRPC]
    private void TurnOff() {
        Debug.Log("Turned off");
        gameObject.SetActive(false);
    }
}
