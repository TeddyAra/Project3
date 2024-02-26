using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatScript : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Rock")) {
            gameObject.SetActive(false);
            GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().ships.Remove(gameObject);
        }
    }
}
