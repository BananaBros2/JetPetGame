using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVibration : MonoBehaviour
{

    Vector3 originalTransform;
    public float shakeMagnitude = 0.04f;
    public float powerfulShakeMagnitude = 0.12f;
    public float timeBetweenIndShakes = 0.01f;

    private void Awake()
    {
        originalTransform = new Vector3(0,0,0);
    }

    public void ShakeOnceStart()
    {
        //StartCoroutine("ShakeOnce", timeBetweenIndShakes); // Start coroutine to shake the camera for a brief period of time
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
