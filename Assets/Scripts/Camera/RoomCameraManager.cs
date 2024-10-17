using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCameraManager : MonoBehaviour
{
    public GameObject virtualCam; // Camera Controller

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            virtualCam.GetComponent<CinemachineVirtualCamera>().Priority = 11; // Increase the camera (location's) priority so that the camera pans over to the created area 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            virtualCam.GetComponent<CinemachineVirtualCamera>().Priority = 10; // Reset the camera (location's) priority
        }
    }
    

}
