using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapBoats : MonoBehaviour {
    [SerializeField] private float moveDistance;

    private Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();
    private GameObject[] boats;
    private GameObject[] icons;
    private RectTransform selection;
    private int selectedNum;
    private RectTransform selectedPos;
    private PhotonView view;

    void Start() {
        view = GetComponent<PhotonView>();

        boats = GameObject.FindGameObjectsWithTag("Boat");
        icons = GameObject.FindGameObjectsWithTag("Icon");

        for (int i = 0; i < boats.Length; i++) {
            boatIcons.Add(boats[i].transform, icons[i + 1].GetComponent<RectTransform>());
        }

        selection = icons[0].GetComponent<RectTransform>();
        selectedPos = icons[1].GetComponent<RectTransform>();
    }

    void Update() {
        foreach (var boatIcon in boatIcons) {
            boatIcon.Value.anchoredPosition = new Vector3(boatIcon.Key.position.x * 10, boatIcon.Key.position.z * 10, 0);
        }

        selection.anchoredPosition = selectedPos.anchoredPosition;
    }

    public void Select(int num) {
        selectedNum = num;
        selectedPos = icons[num + 1].GetComponent<RectTransform>();
    }
    
    public void MoveBoat(bool left) {
        boats[selectedNum].transform.Translate(Vector3.right * (left ? -moveDistance : moveDistance));
    }
}