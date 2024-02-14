using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverseerScript : MonoBehaviour
{
    public int arrivedBoats = 0; 
    public TMP_Text score; 


    [System.Serializable]
    public struct BoatButton {

        public MapBoatMovement button;
        public GameObject boat;
        public BoatMovement boatMovement; 
    }

    [SerializeField] private List<BoatButton> boats = new List<BoatButton>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < boats.ToList().Count; i++)
        {
            BoatButton boatStruct = boats[i]; 
            boatStruct.boatMovement = boats[i].boat.GetComponent<BoatMovement>();
            boats[i] = boatStruct;
        }

    }

    // Update is called once per frame
    void Update()
    {

        foreach (var boatButton in boats)
        {

            if (boatButton.button.isActive == true)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    boatButton.boatMovement.MoveRight(); 
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    boatButton.boatMovement.MoveLeft(); 
                }
            }
        }

        for (int i = 0;i < boats.ToList().Count;i++)
        {
            if (Input.GetKeyDown($"{i + 1}")){
                boats[i].button.isActive = !boats[i].button.isActive ; 
                for (int j = 0; j < boats.ToList().Count; j++)
                {
                    if (j == i)
                    {
                        continue; 
                    }
                    boats[j].button.isActive = false; 
                }
            }

            
        }
    }


}
