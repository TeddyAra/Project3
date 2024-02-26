using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class GameManager : MonoBehaviour {
    [Serializable]
    public struct Wave {
        public bool newWave;
        public float spawnDelay;
        public Transform spawnPoint;
        public GameObject shipType;
        public Obstacle[] obstacles;
    }

    [Header("Waves")]
    [SerializeField] private List<Wave> waves;

    [Header("References")]
    [SerializeField] private bool singlePlayerTest;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject normalCameraPrefab;
    [SerializeField] private GameObject shipPrefab;
    public List<Material> materials = new List<Material>();

    [Header("Settings")]
    [SerializeField] private float shipDistance;
    [SerializeField] private float shipSpeed;
    [SerializeField] private float cameraHeight;

    private PhotonView view;
    private bool gameStarted = false;
    [HideInInspector] public List<GameObject> ships = new List<GameObject>();
    private float spawnTimer;
    private int currentIndex = 0;
    private bool waitingForWin = false;

    void Start() {
        view = GetComponent<PhotonView>();

        switch (PhotonNetwork.PlayerList.Length) {
            case 1: 
                Instantiate(cameraPrefab, Vector3.up * cameraHeight, Quaternion.Euler(90, -90, 0));
                if (singlePlayerTest) StartGame();
                break;
            case 2:
                Instantiate(normalCameraPrefab);
                Instantiate(mapPrefab);
                if (!singlePlayerTest) StartGame();
                break;
        }

        codeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;
    }

    void StartGame() {
        gameStarted = true;
        codeText.text = "";

        /*for (int i = 0; i < 4; i++) {
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
            ship.GetComponentsInChildren<Renderer>()[1].material = materials[i];
            ships.Add(ship);
        }*/
    }

    void Update() {
        if (!gameStarted) return;

        // Move all ships
        foreach (GameObject ship in ships) {
            ship.transform.Translate(Vector3.forward * Time.deltaTime * shipSpeed);
        }

        // Checks if the game has been won
        if (waitingForWin) {
            if (ships.Count == 0) {
                gameStarted = false;
                waitingForWin = false;
                Debug.Log("Players have won!");
            }
            return;
        }

        // Check if waiting for a new wave
        if (waves[currentIndex].newWave && ships.Count > 0) return;
        spawnTimer += Time.deltaTime;

        // If a new ship should be spawned
        if (spawnTimer > waves[currentIndex].spawnDelay) {
            // Spawn the ship and reset the spawn timer
            GameObject ship = PhotonNetwork.Instantiate(waves[currentIndex].shipType.name, waves[currentIndex].spawnPoint.position, waves[currentIndex].spawnPoint.rotation);
            ships.Add(ship);
            spawnTimer = 0;

            // Spawns the obstacle(s) if needed
            if (waves[currentIndex].obstacles.Length != 0) {
                foreach (var obstacle in waves[currentIndex].obstacles) obstacle.Spawn();
            }

            // Let the other player know a ship has spawned
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; 
            PhotonNetwork.RaiseEvent(1, "NewShip", raiseEventOptions, SendOptions.SendReliable);

            // Check if it's the end of the game or not
            if (currentIndex != waves.Count - 1) {
                currentIndex++;
                return;
            } else {
                waitingForWin = true;
            }
        }
    }
}
