using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Button {
    private bool left;
    private MapBoats mapBoatsScript;

    private new void Start() { 
        mapBoatsScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapBoats>();
        if (transform.name == "Left") left = true;
    }

    public override void OnPointerDown(PointerEventData eventData) {
         base.OnPointerDown(eventData);
        mapBoatsScript.StartTurn(left);
    }

    public override void OnPointerUp(PointerEventData eventData) {
        base.OnPointerUp(eventData);
        mapBoatsScript.StopTurn();
    }
}
