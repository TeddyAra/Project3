using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IOnEventCallback {

    [SerializeField] private AudioSource shipSpawn;
    public AudioClip shipSpawnSound; 
   
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
    [SerializeField] private TMP_Text announcementText;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private float searchThreshHold;
    [SerializeField] private GameObject zoomImage;
    [SerializeField] private GameObject gyroImage;

    [Header("References")]
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject normalCameraPrefab;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Pinwheel.Poseidon.PWater water;
    [SerializeField] private AudioSource shipDestroy;
    [SerializeField] private AudioSource dockReach;

    private PhotonView view;
    [HideInInspector] public bool gameStarted = false;
    [HideInInspector] public List<GameObject> ships = new List<GameObject>();
    private float spawnTimer;
    private int currentIndex = 0;
    private bool waitingForWin = false;
    private List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> obstaclePoints = new List<Transform>();
    private Transform cam;
    private BoatScript selectedScript;

    private bool twoDone;
    private BoatScript tutorialShipScript;
    private bool nextClicked;
    private bool tutorialDone;
    private bool tutorialFailed;
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
    [HideInInspector] public const byte NextClicked = 8;
    [HideInInspector] public const byte StartTurnLeft = 9;
    [HideInInspector] public const byte StartTurnRight = 10;
    [HideInInspector] public const byte StopTurn = 11;
    [HideInInspector] public const byte NewSelection = 12;

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
                    readyButton.transform.SetParent(map.transform);
                    gyroImage.transform.SetParent(map.transform);
                    zoomImage.transform.SetParent(map.transform);

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
        if (!handling) map.Next();
        else nextClicked = true;
    }

    private void Announce(string text) {
        nextClicked = false;
        announcementText.text = text;
    }

    private void ShowAnnouncement(GameObject img = null) {
        announcement.SetActive(true);

        if (img != null) img.SetActive(true);
    }

    private void HideAnnouncement(GameObject img = null) {
        announcement.SetActive(false);
        SendEvent(HideText);

        if (img != null) img.SetActive(false);
    }

    private void ShowWaiting() {
        playerReady.text = "Waiting for the other player...";
        nextButton.SetActive(false);
    }

    private void HideWaiting() {
        playerReady.text = "";
        nextButton.SetActive(true);
        twoDone = false;
    }

    public void ShipFail() {
        view.RPC("ShipFailRPC", RpcTarget.All);
    }

    [PunRPC]
    private void ShipFailRPC() {
        if (!tutorialDone) {
            Points.pointAmount -= 100;
            if (Points.pointAmount < 0) Points.pointAmount = 0;
        }

        tutorialFailed = true;
    }

    public void ShipSucceed(bool right) {
        view.RPC("ShipSucceedRPC", RpcTarget.All, right);
    }

    [PunRPC]
    private void ShipSucceedRPC(bool right) {
        if (!tutorialDone) Points.pointAmount += right ? 100 : 50;

        tutorialDone = true;
    }

    public void Ready() {
        if (handling) oneReady = true;
        else view.RPC("ReadyRPC", RpcTarget.Others);
    }

    [PunRPC] 
    private void ReadyRPC() {
        twoReady = true;
    }

    [PunRPC]
    private void ShowReady() { 
        readyButton.transform.gameObject.SetActive(true);
    }

    [PunRPC]
    private void HideReady() {
        readyButton.transform.gameObject.SetActive(false);
    }

    IEnumerator Tutorial() {
        codeText.text = "";

        // Announcement telling the player to look around
        ShowAnnouncement();
        gyroImage.SetActive(true);
        Announce("Move your phone to look around the area!");

        SendEvent(Announcement, new string[] {
            "On yer map ye can see the seas and land around the lighthouse. Any incoming vessels also appear on this map. Now, our map is� behind the times, so not all dangers of the sea are visible to you. Rely on your spotter for proper navigation of ships!"
        });
        while (!nextClicked) yield return null;

        ShowWaiting();
        while (!twoDone) {
            yield return null;
        }
        HideWaiting();

        // Make tutorial ship
        GameObject ship = PhotonNetwork.Instantiate(tutorialShip.name, tutorialSpawn.position, tutorialSpawn.rotation);
        ship.GetComponent<SimpleBuoyController>().water = water;
        tutorialShipScript = ship.GetComponent<BoatScript>();
        tutorialShipScript.shipDestroy = shipDestroy;
        tutorialShipScript.shipReachDock = dockReach;
        SendEvent(NewShip);

        Pause();

        HideAnnouncement();
        gyroImage.SetActive(false);
        ShowAnnouncement();

        // Announcement telling the player there's a new ship
        Announce("Me ship senses be tingling! Let�s investigate our newfound vessel!");

        SendEvent(Announcement, new string[] {
            "A new ship has appeared on our map! But we don�t yet know the bay port that this vessel calls home.", " Consult yer trusty guide as well as your ship spotter on the appearance of the ship to figure out where it�s from.",
            "When yer sure of our vessel�s destination, continue by pressing the button in the top right corner"
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
            Vector3 pos1 = ship.transform.position - cam.position;
            Vector3 pos2 = cam.position + cam.forward * pos1.magnitude;
            float difference = (pos2 - pos1).magnitude;
            Debug.Log(difference);
            if (difference < searchThreshHold) {
                shipFound = true;
            }
            yield return null;
        }

        // Announcement telling the player to zoom in
        ShowAnnouncement();
        zoomImage.SetActive(true);
        Announce("You young bucks have the luck of using one of them brand new touch-screen binoculars. Back in my day you had nothing but the own peepers in ye skull to scan the ocean waters!");
        while (!nextClicked) yield return null;

        // Wait for the player to zoom in
        HideAnnouncement();
        zoomImage.SetActive(false);
        Camera actualCam = cam.gameObject.GetComponent<Camera>();
        while (actualCam.fieldOfView > requiredZoom) yield return null;

        // Announcement telling the player to describe the ship
        ShowAnnouncement();
        Announce("But what port does this old trawler call home? Your comrade has the ol� shipguide to tell them exactly that, and it�s up to you to give them the ship�s characteristics so they can figure it out.");
        while (!nextClicked) yield return null;

        // Wait for both players to start the game
        Announce("When yer sure of where the ship needs to go, press the button on top right. When ye and ye pall are both sure we can tell the ship to start moving.");
        while (!nextClicked) yield return null;
        
        Announce("Be warned though, others ships won�t wait for you to identify them, so keep a weather eye on the horizon!");
        while (!nextClicked) yield return null;

        HideAnnouncement();
        view.RPC("ShowReady", RpcTarget.All);
        while (!twoReady || !oneReady) yield return null;
        view.RPC("HideReady", RpcTarget.All);

        // Wait for the ship to get close to the obstacle
        Resume();
        while ((ship.transform.position - tutorialObstacle.position).magnitude > minimumDistance) {
            Debug.Log((ship.transform.position - tutorialObstacle.position).magnitude);
            yield return null;
        }

        // Announcement telling the player to tell the other player to watch for obstacles
        Pause();
        ShowAnnouncement();
        Announce("Now, the ol� map your mate has before them is what you might call a bit� behind the times, so they don�t know about the many hazards you can find in these waters.");
        while (!nextClicked) yield return null;
        
        Announce("You�re their only lifeline against these perils. So keep a good eye on the vessels, or in a blink of the lighthouse lamp they�re going to be greeting Davy Jones at the bottom of the sea.");
        while (!nextClicked) yield return null; //Might not be necesary, I don't think there was a return statement here orriginally

        SendEvent(Announcement, new string[] {
            "We know the ship�s destination, so let�s get it there! Select a vessel and press the arrows to steer it port or starboard."
        });
        while (!nextClicked) yield return null;

        

        // Show that player 1 is done
        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        // Wait for the ship to get to the port
        Resume();
        HideAnnouncement();
        while (!tutorialDone) {
            if (tutorialFailed) {
                FailedTutorial();
            }
            yield return null;
        }

        // Wait for both players to start the game
        Pause();
        ShowAnnouncement();
        Announce("And that�s all words can help you with, so time to get your sea legs! May you have fair winds and following seas, ye whippersnapper!");
        
        SendEvent(Announcement, new string[] {
            "And that�s all words can help you with, so time to get your sea legs! May you have fair winds and following seas, ye whippersnapper!"
        });
        while (!nextClicked) yield return null;

        // Show that player 1 is done
        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        Resume();
        HideAnnouncement();

        gameStarted = true;
    }

    private void FailedTutorial() {
        tutorialFailed = false;
        StopAllCoroutines();
        StartCoroutine(RestartTutorial());
    }

    IEnumerator RestartTutorial() {
        GameObject ship = PhotonNetwork.Instantiate(tutorialShip.name, tutorialSpawn.position, tutorialSpawn.rotation);
        ship.GetComponent<SimpleBuoyController>().water = water;
        tutorialShipScript = ship.GetComponent<BoatScript>();
        tutorialShipScript.shipDestroy = shipDestroy;
        tutorialShipScript.shipReachDock = dockReach;
        SendEvent(NewShip);

        while (!tutorialDone) { 
            if (tutorialFailed) {
                FailedTutorial();
            }
            yield return null;
        }

        // Wait for both players to start the game
        Pause();
        ShowAnnouncement();
        Announce("And that�s all words can help you with, so time to get your sea legs! May you have fair winds and following seas, ye whippersnapper!");

        SendEvent(Announcement, new string[] {
            "And that�s all words can help you with, so time to get your sea legs! May you have fair winds and following seas, ye whippersnapper!"
        });
        while (!nextClicked) yield return null;

        // Show that player 1 is done
        ShowWaiting();
        while (!twoDone) yield return null;
        HideWaiting();

        Resume();
        HideAnnouncement();

        gameStarted = true;
    }

    private void Pause() {
        tutorialShipScript.paused = true;
    }

    private void Resume() {
        tutorialShipScript.paused = false;
    }

    public void OnEvent(EventData photonEvent) {
        // Ignore some events
        if (photonEvent.Code > 100) return;

        BoatScript shipScript;

        // Check which event for the tutorial it is
        switch (photonEvent.Code) {
            case TaskDone:
                twoDone = true;
                break;
            case StartTurnLeft:
                shipScript = gameStarted ? selectedScript : tutorialShipScript;
                shipScript.turning = true;
                shipScript.left = true;
                break;
            case StartTurnRight:
                shipScript = gameStarted ? selectedScript : tutorialShipScript;
                shipScript.turning = true;
                shipScript.left = false;
                break;
            case StopTurn:
                shipScript = gameStarted ? selectedScript : tutorialShipScript;
                shipScript.turning = false;
                break;
            case NewSelection:
                int photonid = (int)photonEvent.CustomData;
                Debug.Log(photonid);
                selectedScript = PhotonNetwork.GetPhotonView(photonid).gameObject.GetComponent<BoatScript>();
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
    }

    [PunRPC]
    private void FinishGame() {
        SceneManager.LoadScene("EndScreen");
    }

    void Update() {
        // Don't do anything if there's no game to play or if not managing the game
        if (!gameStarted) return;

        // Checks if the game has been won
        if (waitingForWin) {
            if (ships.Count == 0) {
                gameStarted = false;
                waitingForWin = false;
                view.RPC("FinishGame", RpcTarget.All);
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
            ship.GetComponent<SimpleBuoyController>().water = water;
            BoatScript shipScript = ship.GetComponent<BoatScript>();
            shipScript.shipDestroy = shipDestroy;
            shipScript.shipReachDock = dockReach;
            ships.Add(ship);
            shipSpawn.PlayOneShot(shipSpawnSound); 

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
}