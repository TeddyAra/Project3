using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBoats : MonoBehaviour {
    [SerializeField] private Dictionary<Transform, RectTransform> boatIcons = new Dictionary<Transform, RectTransform>();

    private GameObject[] boats;
    private GameObject[] icons;

    void Start() {
        boats = GameObject.FindGameObjectsWithTag("Boat");
        icons = GameObject.FindGameObjectsWithTag("Icon");

        for (int i = 0; i < boats.Length; i++) {
            boatIcons.Add(boats[i].transform, icons[i].GetComponent<RectTransform>());
        }
    }

    void Update() {
        foreach (var boatIcon in boatIcons) {
            boatIcon.Value.position = new Vector3(boatIcon.Key.position.x / 10, boatIcon.Key.position.z / 10, 0);
        }
    }
}