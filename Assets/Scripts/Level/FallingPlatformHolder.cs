using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformHolder : MonoBehaviour
{
    [Header("References")]
    public GameObject platformPrefab; // Prefab for falling object
    
    [Header("Timing")]
    [Range(0.1f, 25)] public float respawnTime = 4;


    /// <summary> Function for initiating platform respawn </summary>
    public void SpawnNewPlatform()
    {
        StartCoroutine("PlatformCountdown");
    }

    /// <summary> Will create platform after 4 seconds have passed </summary>
    private IEnumerator PlatformCountdown()
    {
        yield return new WaitForSeconds(respawnTime); // Waits 4 seconds before spawning new platform

        GameObject newPlatform = Instantiate(platformPrefab, transform.position, Quaternion.identity, transform); // Create replacement platform
    
        // Was going to add a detection to see if anything was blocking the platform from spawning but would just create inconsistency for players
    } 
}

