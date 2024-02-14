using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float xSensitivity; 
    [SerializeField] private float ySensitivity;

    private float xRotation = 0f; 

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false; 
    }

    // Update is called once per frame
    void Update()
    {


        float x = xSensitivity * Input.GetAxis("Mouse X") * Time.deltaTime; 
        float y = ySensitivity * Input.GetAxis("Mouse Y") * Time.deltaTime; 

        xRotation -= y; 

        xRotation = Mathf.Clamp(xRotation, -90f, 90f); 

        transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y + x, 0f); 
        
    }
}
