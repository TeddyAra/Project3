using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapBoatMovement : MonoBehaviour
{

    [SerializeField] private GameObject boat; 
    public bool isActive = false; 
    public bool isDead = false; 
    public Sprite deadShip; 
    public Image spriteRenderer; 
    public Sprite activeShip; 
    public Sprite regularShip; 

 
    void Update()
    {
        if (boat != null)
        {
            float boatXposition = boat.transform.position.x * 3.2f;
            float boatZposition = boat.transform.position.z * 3.2f;

            transform.localPosition = new Vector2(boatXposition, boatZposition);
            
        }
        else
        {
            spriteRenderer.sprite = deadShip; 
            isDead = true; 
            isActive = false; 
        }

        if (isActive && isDead == false)
        {
            spriteRenderer.sprite = activeShip;
        }

        if (!isActive && isDead == false)
        {
            spriteRenderer.sprite = regularShip;
        }
        

        
    }

    public void Activate()
    {
        isActive = !isActive; 
    }
}
