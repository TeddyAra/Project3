using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScriptTest : MonoBehaviour {
    [SerializeField] private Image player1;
    [SerializeField] private Image player2;
    [SerializeField] private float buttonRadius;
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private GameObject serverPrefab;

    private bool changedScene = false;

    void Update() {
        if (Input.touchCount > 0) {
            if (changedScene) return;
            Touch touch = Input.GetTouch(0);

            Vector2 rectPosition = new Vector2(player1.rectTransform.position.x, player1.rectTransform.position.y);
            if ((touch.position - rectPosition).magnitude < buttonRadius) {
                DontDestroyOnLoad(Instantiate(clientPrefab));
                SceneManager.LoadScene("Player1UT");
                changedScene = true;
            }

            rectPosition = new Vector2(player2.rectTransform.position.x, player2.rectTransform.position.y);
            if ((touch.position - rectPosition).magnitude < buttonRadius) {
                DontDestroyOnLoad(Instantiate(serverPrefab));
                DontDestroyOnLoad(Instantiate(clientPrefab));

                SceneManager.LoadScene("Player2UT");
                changedScene = true;
            }
        }
    }
}
