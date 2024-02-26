using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTest : Obstacle {
    public override void Spawn() {
        gameObject.SetActive(true);
        Debug.Log("Test obstacle spawned");
    }
}
