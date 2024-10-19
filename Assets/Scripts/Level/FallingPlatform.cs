using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Invulnerability")]
    public float invulnerabilityDuration = 1;
    private bool invulnerable = true;
    private bool triggeredToFall = false;

    [Header("Platform Shake")]
    float shakeDuration = 0.2f;
    private float shakeMagnitude = 0.05f;

    private Vector3 originalTransform;


    private void Awake()
    {
        originalTransform = new Vector3(0, 0, 0);
        StartCoroutine("InvulnerabilityTime", invulnerabilityDuration);

    }

    /// <summary> Small coroutine for turning off invulnerability after a set time </summary>
    private IEnumerator InvulnerabilityTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        invulnerable = false;
    }

    /// <summary> Will start collapse of platform if not invulnerable or already falling </summary>
    public void AttemptCollapse()
    {
        if (!triggeredToFall && !invulnerable)
        {
            triggeredToFall = true;
            StartCoroutine("Collapse", 0.01f);
        }

    }

    /// <summary> Begin platform decent and trigger respawn for a replacement </summary>
    private IEnumerator Collapse(float waitTime) 
    {
        while (true)
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalTransform + Random.insideUnitSphere * shakeMagnitude; // Same method for shake as the ScreenVibration Script
                shakeDuration -= Time.deltaTime;
            }
            else // After shaking for set amount of time:
            {
                StopAllCoroutines(); 
                transform.localPosition = originalTransform; // Reset back to original position without any shake offset

                Rigidbody2D rb = transform.AddComponent<Rigidbody2D>(); // Adds new rigidbody2D so that it will fall and freezes Z rotation
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                transform.parent.GetComponent<FallingPlatformHolder>().SpawnNewPlatform(); // Spawn replacement
                transform.SetParent(null); // Sever connection to parent
                Invoke("DestroyPlatform", 5); // Start destroy countdown
            }
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary> Destroy platform safely by checking the child count </summary>
    private void DestroyPlatform() // Destroy the platform only if there is nothing still attached to it (i.e. the player)
    {
        if(transform.childCount == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Invoke("DestroyPlatform", 4); // Restart timer if failed
        }
    }
}
