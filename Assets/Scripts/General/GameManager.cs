using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

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
    public bool singlePlayerTest;
    [SerializeField] private bool skipTutorial;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialShip;
    [SerializeField] private Transform tutorialSpawn;
    [SerializeField] private float requiredZoom;
    [SerializeField] private float minimumDistance;
    [SerializeField] private Transform tutorialObstacle;
    [SerializeField] private GameObject announcementPrefab;
    public TMP_Text playerReady;
    [SerializeField] private GameObject announcement;
    public Button readyButton;

    [Header("References")]
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject normalCameraPrefab;
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
    private bool nextClicked;
    private bool tutorialDone;
    private TMP_Text announcementText;
    private MapBoats map;
    private bool oneReady;
    private bool twoReady;
    private bool handling;

    [HideInInspector] public const byte NewShip = 1;
    [HideInInspector] public const byte TutorialShip = 2;
    [HideInInspector] public const byte TaskDone = 3;
    [HideInInspector] public const byte Announcement = 4;
    [HideInInspector] public const byte HideText = 5;
    [HideInInspector] public const byte CheckForLeft = 6;
    [HideInInspector] public const byte CheckForReady = 7;

    private void OnEnable() {
        Debug.Log("OnEvent() Enabled");
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable() {
        Debug.Log("OnEvent() Disabled");
        PhotonNetwork.RemoveCallbackTarget(this);
    }

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
                    cam = Instantiate(cameraPrefab, cameraPos.position, Quaternion.Euler(90, -90, 0)).GetComponentInChildren<Camera>().transform;

                    // Change the code text's parent
                    GameObject map = Instantiate(mapPrefab);
                    GameObject parent = codeText.transform.parent.gameObject;

                    codeText.transform.SetParent(map.transform);
                    playerReady.transform.SetParent(map.transform);
                    announcement.transform.SetParent(map.transform);

                    //announcement.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    announcementText = announcement.transform.GetComponentInChildren<TMP_Text>();
                    readyButton = announcement.transform.GetComponentInChildren<Button>();

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
                map = Instantiate(mapPrefab).GetComponent<MapBoats>();

                playerReady.transform.SetParent(map.transform);
                announcement.transform.SetParent(map.transform);
                readyButton.transform.SetParent(map.transform);
                map.announcement = announcement;
                map.announcementText = announcement.transform.GetComponentInChildren<TMP_Text>();

                view.RPC("StartGameRPC", RpcTarget.Others);
                break;
        }
    }

    private void SendEvent(byte code, string[] text = null) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = singlePlayerTest ? ReceiverGroup.All : ReceiverGroup.Others };
        if (PhotonNetwork.RaiseEvent(code, text, raiseEventOptions, SendOptions.SendReliable)) Debug.Log($"Event sent with code {code}");
    }

    public void Next() {
        nextClicked = true;
    }

    private void Announce(string text) {
        nextClicked = false;
        announcementText.text = text;
    }

    private void ShowAnnouncement() {
        announcement.SetActive(true);
    }

    private void HideAnnouncement() {
        announcement.SetActive(false);
        SendEvent(HideText);
    }

    private void ShowWaiting() {
        playerReady.text = "Waiting for the other player...";
    }

    private void HideWaiting() {
        playerReady.text = "";
    }

    public void ShipFail() { 
        
    }

    public void ShipSucceed() { 
        tutorialDone = true;
    }

    public void Ready() {
        if (handling) oneReady = true;
        else twoReady = true;
    }

    private void ShowReady() { 
        readyButton.transform.gameObject.SetActive(true);
    }

    private void HideReady() {
        readyButton.transform.gameObject.SetActive(false);
    }

    IEnumerator Tutorial() {
        Debug.Log("Tutorial started");

        // Announcement telling the player to look around
        ShowAnnouncement();
        Announce("Move your phone to look around the area!");

        SendEvent(Announcement, new string[] {
            "On yer map ye can see the seas and land around the lighthouse. Any incoming vessels also appear on this map. Now, our map is… behind the times, so not all dangers of the sea are visible to you. Rely on your spotter for proper navigation of ships!"
        });
        while (!nextClicked) yield return null;

        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        // Make tutorial ship
        GameObject ship = PhotonNetwork.Instantiate(tutorialShip.name, tutorialSpawn.position, tutorialSpawn.rotation);
        ship.GetComponent<SimpleBuoyController>().water = water;
        tutorialShipScript = ship.GetComponent<BoatScript>();
        SendEvent(TutorialShip);

        Pause();

        // Announcement telling the player there's a new ship
        Announce("Me ship senses be tingling! Let’s investigate our newfound vessel!");

        SendEvent(Announcement, new string[] {
            "A new ship has appeared on our map! But we don’t yet know the bay port that this vessel calls home. Consult yer trusty guide as well as your ship spotter on the appearance of the ship to figure out where it’s from.",
            "TEST HERE, TELL THE PLAYER THAT THEY CAN PRESS READY TOP RIGHT TO START THE GAME WHEN PLAYER 1 HAS SEEN THE SHIP"
        });
        SendEvent(CheckForLeft);
        while (!nextClicked) yield return null;

        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        // Remove announcement
        HideAnnouncement();

        // Wait for the player to look at the ship
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

        // Announcement telling the player to zoom in
        ShowAnnouncement();
        Announce("You young bucks have the luck of using one of them brand new touch-screen binoculars. Back in my day you had nothing but the own peepers in ye skull to scan the ocean waters!");
        while (!nextClicked) yield return null;

        // Wait for the player to zoom in
        HideAnnouncement();
        Camera actualCam = cam.gameObject.GetComponent<Camera>();
        while (actualCam.fieldOfView > requiredZoom) yield return null;

        // Announcement telling the player to describe the ship
        ShowAnnouncement();
        Announce("But what port does this old trawler call home? Your comrade has the ol’ shipguide to tell them exactly that, and it’s up to you to give them the ship’s characteristics so they can figure it out.");
        while (!nextClicked) yield return null;

        // Wait for both players to start the game
        Announce("TEST HERE, TELL THE PLAYER THAT THEY CAN PRESS READY TOP RIGHT TO START THE GAME");
        while (!nextClicked) yield return null;

        HideAnnouncement();
        ShowReady();
        while (!twoReady || !oneReady) yield return null;
        HideReady();

        // Wait for the ship to get close to the obstacle
        Resume();
        while ((ship.transform.position - tutorialObstacle.position).magnitude > minimumDistance) yield return null;

        // Announcement telling the player to tell the other player to watch for obstacles
        Pause();
        ShowAnnouncement();
        Announce("Now, the ol’ map your mate has before them is what you might call a bit… behind the times, so they don’t know about the many hazards you can find in these waters. You’re their only lifeline against these perils. So keep a good eye on the vessels, or in a blink of the lighthouse lamp they’re going to be greeting Davy Jones at the bottom of the sea.");

        SendEvent(Announcement, new string[] {
            "We know the ship’s destination, so let’s get it there! Select a vessel and press the arrows to steer it port or starboard."
        });
        while (!nextClicked) yield return null;

        // Show that player 1 is done
        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        // Wait for the ship to get to the port
        Resume();
        HideAnnouncement();
        while (!tutorialDone) yield return null;

        // Wait for both players to start the game
        Pause();
        ShowAnnouncement();
        Announce("And that’s all words can help you with, so time to get your sea legs! Keep a weather eye on the horizon and may you have fair winds and following seas, ye whippersnapper!");
        
        SendEvent(Announcement, new string[] {
            "And that’s all words can help you with, so time to get your sea legs! Keep a weather eye on the horizon and may you have fair winds and following seas, ye whippersnapper!"
        });
        while (!nextClicked) yield return null;

        // Show that player 1 is done
        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        Resume();
        HideAnnouncement();
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
        handling = true;
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