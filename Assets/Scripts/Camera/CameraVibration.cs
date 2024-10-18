using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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

    public void ShakeOnceStart()
    {
        StartCoroutine("ShakeOnce", timeBetweenIndShakes); // Start coroutine to shake the camera for a brief period of time
    }

    private IEnumerator ShakeOnce(float waitTime)
    {
        float shakeDuration = 0.15f;
        bool shaking = true;
        while (shaking)
        {
            
            if (shakeDuration > 0)
            {
                transform.localPosition = originalTransform + Random.insideUnitSphere * powerfulShakeMagnitude; // Changes the camera's offset to a random value within an area around the original position
                yield return new WaitForSeconds(waitTime);
                shakeDuration -= waitTime;
            }
            else
            {
                shaking = false;
                transform.localPosition = originalTransform; // Resets the camera's position to the original state
            }
            yield return new WaitForSeconds(waitTime);
        }
    }
    public void ConstantShakeStart()
    {
        StartCoroutine("ConstantShake", timeBetweenIndShakes);
    }
    public void ConstantShakeCancel()
    {
        StopCoroutine("ConstantShake"); // Stop shaking coroutine
        transform.localPosition = originalTransform; // Resets the camera's position to the original state
    }

    private IEnumerator ConstantShake(float waitTime)
    {
        while (true)
        {
            transform.localPosition = originalTransform + Random.insideUnitSphere * shakeMagnitude; // Changes the camera's offset to a random value within an area around the original position
            yield return new WaitForSeconds(waitTime);

        }
    }
}
