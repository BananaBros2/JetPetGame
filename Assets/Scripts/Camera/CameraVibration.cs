using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVibration : MonoBehaviour
{

    Vector3 originalTransform;
    private float shakeMagnitude = 0.04f;

    private void Awake()
    {
        originalTransform = new Vector3(0,0,0);
    }

    public void ShakeOnceStart()
    {
        StartCoroutine("ShakeOnce", 0.01f);
    }

    private IEnumerator ShakeOnce(float waitTime)
    {
        float shakeDuration = 0.15f;
        bool shaking = true;
        while (shaking)
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalTransform + Random.insideUnitSphere * shakeMagnitude * 3;
                shakeDuration -= waitTime;
            }
            else
            {
                shaking = false;
                transform.localPosition = originalTransform;
            }
            yield return new WaitForSeconds(waitTime);
        }
    }
    public void ConstantShakeStart()
    {
        StartCoroutine("ConstantShake", 0.01f);
    }
    public void ConstantShakeCancel()
    {
        StopCoroutine("ConstantShake");
        transform.localPosition = originalTransform;
    }

    private IEnumerator ConstantShake(float waitTime)
    {
        while (true)
        {
            transform.localPosition = originalTransform + Random.insideUnitSphere * shakeMagnitude;
            yield return new WaitForSeconds(waitTime);

        }
    }
}
