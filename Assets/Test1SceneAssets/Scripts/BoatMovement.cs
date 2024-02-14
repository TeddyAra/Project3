using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoatMovement : MonoBehaviour
{

    public float boatSpeed; 
    [SerializeField] private OverseerScript overseerScript; 
    
    


    // Update is called once per frame
    void Update()
    {
        transform.Translate(-boatSpeed * Time.deltaTime, 0, 0); 
        overseerScript.score.text = "Boats saved = " + overseerScript.arrivedBoats + "/4"; 
        
    }

    public void MoveRight()
    {
        transform.Translate(0, 0, 5); 
    }
    public void MoveLeft()
    {
        transform.Translate(0, 0, -5); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Port"))
        {
            overseerScript.arrivedBoats++;
        }
    }
}
