using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatScript : MonoBehaviour {


    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Rock");
        if (collision.transform.CompareTag("Rock")) { 
            gameObject.SetActive(false);
        }
    }
}
