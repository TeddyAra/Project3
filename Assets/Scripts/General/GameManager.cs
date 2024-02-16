using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject normalCameraPrefab;
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private float shipDistance;
    [SerializeField] private float shipSpeed;
    [SerializeField] private float cameraHeight;
    [SerializeField] private List<Material> materials = new List<Material>();
    private PhotonView view;

    private bool gameStarted = false;
    private List<GameObject> ships = new List<GameObject>();

    void Start() {
        view = GetComponent<PhotonView>();

        switch (PhotonNetwork.PlayerList.Length) {
            case 1: 
                Instantiate(cameraPrefab, Vector3.up * cameraHeight, Quaternion.Euler(90, -90, 0));
                //StartGame();
                break;
            case 2:
                Instantiate(normalCameraPrefab);
                Instantiate(mapPrefab);
                StartGame();
                break;
        }

        codeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;
    }

    void StartGame() {
        gameStarted = true;
        codeText.text = "";

        for (int i = 0; i < 4; i++) {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.Euler(0, i * 90, 0);
            switch (i) {
                case 0:
                    pos.x -= shipDistance;
                    pos.z -= shipDistance;
                    break;
                case 1:
                    pos.x -= shipDistance;
                    pos.z += shipDistance;
                    break;
                case 2:
                    pos.x += shipDistance;
                    pos.z += shipDistance;
                    break;
                case 3:
                    pos.x += shipDistance;
                    pos.z -= shipDistance;
                    break;
            }

            GameObject ship = PhotonNetwork.Instantiate(shipPrefab.name, pos, rot);
            view.RPC("InitBoatRPC", RpcTarget.All, ship, i);
            ships.Add(ship);
        }
    }

    [PunRPC]
    void InitBoatRPC(GameObject ship, int i) {
        ship.GetComponentsInChildren<Renderer>()[1].material = materials[i];
    }

    void Update() {
        if (!gameStarted) return;

        foreach (GameObject ship in ships) {
            view.RPC("MoveBoatRPC", RpcTarget.All, ship);
        }
    }

    [PunRPC]
    void MoveBoatRPC(GameObject ship) {
        ship.transform.Translate(Vector3.forward * Time.deltaTime * shipSpeed);
    }
}
