using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviour, IOnEventCallback {
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

    [Header("Testing")]
    [SerializeField] private bool singlePlayerTest;
    [SerializeField] private bool skipTutorial;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialShip;
    [SerializeField] private Transform tutorialSpawn;

    [Header("References")]
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject normalCameraPrefab;
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Pinwheel.Poseidon.PWater water;

    private PhotonView view;
    private bool gameStarted = false;
    [HideInInspector] public List<GameObject> ships = new List<GameObject>();
    private float spawnTimer;
    private int currentIndex = 0;
    private bool waitingForWin = false;
    private List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> obstaclePoints = new List<Transform>();
    private Transform cam;

    private bool twoDone;
    private BoatScript tutorialShipScript;

    [HideInInspector] public const byte NewShip = 1;
    [HideInInspector] public const byte TutorialShip = 2;
    [HideInInspector] public const byte TaskDone = 3;

    void Start() {
        view = GetComponent<PhotonView>();

        // Update the waves list to work with new waves
        for (int i = 0; i < waves.Count; i++) {
            if (waves[i].newWave && waves[i].spawnDelay <= 0) {
                Wave newWave = waves[i];
                newWave.spawnDelay = 1;
                waves[i] = newWave;
            }
        }

        // Display the code of the room at the top of the screen
        if (PhotonNetwork.CurrentRoom != null) codeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;

        // Check which player number the player has
        switch (PhotonNetwork.PlayerList.Length) {
            // The first player
            case 1: 
                // If testing
                if (singlePlayerTest) {
                    // Make a normal camera
                    //Instantiate(normalCameraPrefab);
                    cam = Instantiate(cameraPrefab, cameraPos.position, Quaternion.Euler(90, -90, 0)).GetComponentInChildren<Camera>().transform;

                    // Change the code text's parent
                    GameObject map = Instantiate(mapPrefab);
                    GameObject parent = codeText.transform.parent.gameObject;
                    codeText.transform.SetParent(map.transform);
                    Destroy(parent);

                    // Start the game
                    if (skipTutorial) StartGame();
                    else StartCoroutine(Tutorial());
                // If not testing
                } else {
                    // Instantiate the gyroscope camera
                    cam = Instantiate(cameraPrefab, cameraPos.position, Quaternion.Euler(90, -90, 0)).GetComponentInChildren<Camera>().transform;
                }
                break;
            // The second player
            case 2:
                if (singlePlayerTest) return;
                // Make a normal camera and map ui
                Instantiate(normalCameraPrefab);
                Instantiate(mapPrefab);
                /*if (!singlePlayerTest) {
                    if (skipTutorial) StartGame();
                    else StartCoroutine(Tutorial());
                }*/
                //SendEvent(StartTutorial);
                view.RPC("StartGameRPC", RpcTarget.Others);
                break;
        }
    }

    private void SendEvent(byte code) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        if (PhotonNetwork.RaiseEvent(code, null, raiseEventOptions, SendOptions.SendReliable)) Debug.Log($"Event sent with code {code}");
    }

    IEnumerator Tutorial() {
        Debug.Log("Tutorial started");

        // Make tutorial ship
        GameObject ship = PhotonNetwork.Instantiate(tutorialShip.name, tutorialSpawn.position, tutorialSpawn.rotation);
        ship.GetComponent<SimpleBuoyController>().water = water;
        tutorialShipScript = ship.GetComponent<BoatScript>();

        // Send event to other player
        SendEvent(TutorialShip);

        // Pause the game
        Pause();

        // Wait for player 1 to look at the ship
        bool shipFound = false;
        while (!shipFound) {
            RaycastHit hit;
            if (Physics.Raycast(cam.position, cam.forward, out hit)) {
                if (hit.transform.gameObject == ship) {
                    shipFound = true;
                    Debug.Log("Ship found");
                }
            }
            yield return null;
        }

        // Wait for both players to be done
        while (!twoDone) {
            yield return null;
        }

        // Resume the game
        Resume();
    }

    private void Pause() {
        Debug.Log("Paused");
        tutorialShipScript.paused = true;

        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        foreach (Obstacle obstacle in obstacles) {
            obstacle.paused = true;
        }
    }

    private void Resume() {
        Debug.Log("Resumed");
        tutorialShipScript.paused = false;

        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        foreach (Obstacle obstacle in obstacles) {
            obstacle.paused = false;
        }
    }

    public void OnEvent(EventData photonEvent) {
        Debug.Log($"Event received with code {photonEvent.Code}");

        // Check which event for the tutorial it is
        switch (photonEvent.Code) {
            case TaskDone:
                twoDone = true;
                break;
        }
    }

    // Ensures that game logic is handled by player 1
    [PunRPC]
    void StartGameRPC() {
        if (skipTutorial) StartGame();
        else StartCoroutine(Tutorial());
    }

    void StartGame() {
        gameStarted = true;
        codeText.text = "";
        Debug.Log("Game started");
    }

    void Update() {
        // Don't do anything if there's no game to play or if not managing the game
        if (!gameStarted) return;

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
            Debug.Log("Ship made");
            // Spawn the ship and reset the spawn timer
            GameObject ship = PhotonNetwork.Instantiate(waves[currentIndex].shipType.name, waves[currentIndex].spawnPoint.position, waves[currentIndex].spawnPoint.rotation);
            ship.GetComponent<SimpleBuoyController>().water = water;
            ships.Add(ship);
            spawnTimer = 0;

            // Spawns the obstacle(s) if needed
            if (waves[currentIndex].obstacles.Length != 0) {
                foreach (var obstacle in waves[currentIndex].obstacles) obstacle.Spawn();
            }

            // Let the other player know a ship has spawned
            SendEvent(NewShip);

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

        // Draw the obstacles
        foreach (Transform obstacle in obstaclePoints) {
            Gizmos.DrawSphere(obstacle.position, 1);
        }
    }
}