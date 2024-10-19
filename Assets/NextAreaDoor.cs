using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextAreaDoor : MonoBehaviour
{
    [Header("References")]
    public CanvasTransition transition;

    [Header("SceneLinking")]
    public int doorID = -1; // ID of this door which other scene's door's will need to link to this specific door
    public string targetSceneName; // Which scene will load
    public int targetDoorID = -1; // Which door the player will 'enter' from in the next loaded scene
    [Tooltip("Default direction 0 will transition the screen from right to left")]
    [Range(0,360)] public float direction = 0;

    private bool activated; // Bool to stop this from activating multiple times, alternative would be to destroy this script using Destroy(this)


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activated)
        {
            activated = true; 
            transition.CoverTransition(targetSceneName, targetDoorID, direction); // Activate scene change sequence
        }
    }

}
