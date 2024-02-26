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
using Unity.VisualScripting;

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
    private List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> obstaclePoints = new List<Transform>();

    void Start() {
        view = GetComponent<PhotonView>();

        // Check which player number the player has
        switch (PhotonNetwork.PlayerList.Length) {
            // The first player
            case 1: 
                // Make the watch tower camera
                Instantiate(cameraPrefab, Vector3.up * cameraHeight, Quaternion.Euler(90, -90, 0));
                if (singlePlayerTest) StartGame();
                break;
            // The second player
            case 2:
                // Make a normal camera and map ui
                Instantiate(normalCameraPrefab);
                Instantiate(mapPrefab);
                if (!singlePlayerTest) StartGame();
                break;
        }

        // Display the code of the room at the top of the screen
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
        // Don't do anything if there's no game to play
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
            ship.GetComponent<BoatScript>().MoveBoat(currentIndex == 0 ? true : false);

            // Spawns the obstacle(s) if needed
            if (waves[currentIndex].obstacles.Length != 0) {
                foreach (var obstacle in waves[currentIndex].obstacles) obstacle.Spawn();
            }

            // Let the other player know a ship has spawned
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; 
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

    // Draw the spawn points and obstacles
    [ExecuteInEditMode]
    private void OnDrawGizmos() {
        // Set the colour
        Gizmos.color = Color.red;

        // Check if the list has been updated
        GameObject[] pointsList = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Count != pointsList.Length) {
            spawnPoints.Clear();
            foreach (var point in pointsList) {
                spawnPoints.Add(point.transform);
            }
        }

        // Draw the arrows
        foreach (Transform spawnPoint in spawnPoints) {
            Gizmos.DrawSphere(spawnPoint.position, 0.1f);
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 4);
            Gizmos.DrawRay(spawnPoint.position + spawnPoint.forward * 4, -spawnPoint.forward + spawnPoint.right);
            Gizmos.DrawRay(spawnPoint.position + spawnPoint.forward * 4, -spawnPoint.forward - spawnPoint.right);
        }

        // Check if the list has been updated
        GameObject[] obstaclesList = GameObject.FindGameObjectsWithTag("Obstacle");
        if (obstaclePoints.Count != obstaclesList.Length) {
            obstaclePoints.Clear();
            foreach (var obstacle in obstaclesList) {
                obstaclePoints.Add(obstacle.transform);
            }
        }

        // Draw the arrows
        foreach (Transform obstacle in obstaclePoints) {
            Gizmos.DrawSphere(obstacle.position, 0.2f);
        }
    }
}