using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformHolder : MonoBehaviour
{
    public GameObject platformPrefab; // Prefab for falling object
    public GameObject player;

    public void SpawnNewPlatform()
    {
        StartCoroutine("PlatformCountdown");
    }

    private IEnumerator PlatformCountdown()
    {
        yield return new WaitForSeconds(4); // Waits 4 seconds before spawning new platform

        GameObject newPlatform = Instantiate(platformPrefab, transform.position, Quaternion.identity, transform); // Create replacement platform
    }
}

