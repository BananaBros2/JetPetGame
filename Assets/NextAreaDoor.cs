using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextAreaDoor : MonoBehaviour
{
    [Header("References")]
    public CanvasTransition transition;

    [Header("SceneLinking")]
    public int doorID = -1;
    public string targetSceneName;
    public int targetDoorID = -1; // Which door the player will 'enter' from in the next loaded scene
    [Range(0,360)] public float direction;

    private bool activated;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activated)
        {
            activated = true; // Activate scene change sequence
            transition.CoverTransition(targetSceneName, targetDoorID, direction);
        }
    }

}
