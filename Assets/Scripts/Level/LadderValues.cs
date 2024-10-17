using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderValues : MonoBehaviour
{
    // Most of the ladder logic is handled by the player movement script

    GameObject topPlatform; // Platform for letting the player stand on top of the ladder
    GameObject bottomPlatform; // Temporary platform for stopping the player from falling off the ladder unintentionally

    private void Awake()
    {
        topPlatform = transform.GetChild(0).gameObject;
        bottomPlatform = transform.GetChild(1).gameObject;
        bottomPlatform.SetActive(false); // Set inital active state to off so that player can't jump off this platform while not on the ladder
    }

    public void ActivatePlatform() // Activate top platform
    {
        topPlatform.SetActive(true);
        bottomPlatform.SetActive(false);
    }

    public void DeactivatePlatform() // Activate bottom platform
    {
        topPlatform.SetActive(false);
        bottomPlatform.SetActive(true);
    }
}
