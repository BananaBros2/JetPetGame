using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVibration : MonoBehaviour
{
    [Header("Shake Power")]
    [Range(0.01f, 0.5f)] public float shakeMagnitude = 0.04f;  // Power of constant screen shaking
    [Range(0.01f, 0.5f)] public float powerfulShakeMagnitude = 0.12f; // Power of quick bursts of screen shake
    [Range(0.001f, 1)] public float timeBetweenIndShakes = 0.01f; // Time between each individual camera displacement

    private Vector3 originalTransform;


    private void Awake()
    {
        originalTransform = transform.localPosition;
    }

    /// <summary> Starts the coroutine to shake the screen for a brief period of time </summary>
    public void ShakeOnceStart(float duration = 0.15f)
    {
        StartCoroutine(ShakeOnce(timeBetweenIndShakes, duration)); // Start coroutine to shake the camera for a set period of time
    }

    /// <summary> Coroutine for a brief but more powerful screen shake </summary>
    private IEnumerator ShakeOnce(float waitTime, float duration)
    {
        float shakeDuration = duration; // Initialising variables for while loop that determine time remaining 
        bool shaking = true;
        while (shaking)
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalTransform + Random.insideUnitSphere * powerfulShakeMagnitude; // Changes the camera's offset to a random value within an area around the original position
                shakeDuration -= waitTime;  // Decrease shake duration remaining
            }
            else
            {
                shaking = false; // Exit loop
                transform.localPosition = originalTransform; // Resets the camera's position to the original state
            }
            yield return new WaitForSeconds(waitTime); // Loop back to start of 'While' after some (waitTime) delay
        }
    }


    /// <summary> Starts the coroutine to endlessly shake the screen until cancelled through ConstantShakeCancel() </summary>
    public void ConstantShakeStart()
    {
        StartCoroutine("ConstantShake", timeBetweenIndShakes);
    }

    /// <summary> Stop coroutine for constant screen shaking </summary>
    public void ConstantShakeCancel()
    {
        StopCoroutine("ConstantShake"); // Stop shaking coroutine
        transform.localPosition = originalTransform; // Resets the camera's position to the original state
    }

    /// <summary> </summary>
    private IEnumerator ConstantShake(float waitTime)
    {
        while (true)
        {
            transform.localPosition = originalTransform + Random.insideUnitSphere * shakeMagnitude; // Changes the camera's offset to a random value within an area around the original position
            yield return new WaitForSeconds(waitTime); // Time until next displacement

        }
    }
}
