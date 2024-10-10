using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformHolder : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject player;
    public void SpawnNewPlatform()
    {
        StartCoroutine("PlatformCountdown");
    }

    private IEnumerator PlatformCountdown()
    {
        yield return new WaitForSeconds(4);

        bool notSpawned = true;
        while (notSpawned)
        {
            GameObject newPlatform = Instantiate(platformPrefab, transform.position, Quaternion.identity, transform);
            
            notSpawned = false;
            yield return new WaitForSeconds(0.5f);

        }
    }
}

