using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScriptTest : MonoBehaviour {
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject background;
    [SerializeField] private Image player1;
    [SerializeField] private Image player2;
    [SerializeField] private float buttonRadius;
    [SerializeField] private GameObject cam;

    void Start() {
        
    }

    void Update() {
        if (Input.touchCount > 0) { 
            Touch touch = Input.GetTouch(0);

            Vector2 rectPosition = new Vector2(player1.rectTransform.position.x, player1.rectTransform.position.y);
            if ((touch.position - rectPosition).magnitude < buttonRadius) {
                cam.AddComponent<CameraRotation>();
                canvas.SetActive(false);
            }

            rectPosition = new Vector2(player2.rectTransform.position.x, player2.rectTransform.position.y);
            if ((touch.position - rectPosition).magnitude < buttonRadius) {
                player1.gameObject.SetActive(false);
                player2.gameObject.SetActive(false);
                background.SetActive(false);
            }
        }
    }
}
