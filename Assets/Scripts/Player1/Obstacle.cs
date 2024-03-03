using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacle : MonoBehaviour {
    [HideInInspector] public bool paused;

    public abstract void Spawn();
}
