using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject mapPrefab;

    void Start() {
        switch (PhotonNetwork.PlayerList.Length) {
            case 1: 
                Instantiate(cameraPrefab);
                break;
            case 2:
                Instantiate(new Camera());
                Instantiate(mapPrefab);
                break;
        } 
    }
    
    void Update() {
        
    }
}
