using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{

    Vector3 originalTransform;
    float shakeDuration = 0.2f;
    private float shakeMagnitude = 0.05f;
    private float dampingSpeed = 1.0f;

    public float invulnerabilityDuration = 1;
    private bool invulnerable = true;

    bool triggeredToFall = false;

    private void Awake()
    {
        originalTransform = new Vector3(0, 0, 0);
        StartCoroutine("InvulnerabilityTime", invulnerabilityDuration);

    }


    private IEnumerator InvulnerabilityTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        invulnerable = false;
    }

    public void AttemptCollapse()
    {
        if (!triggeredToFall && !invulnerable)
        {
            triggeredToFall = true;
            StartCoroutine("Collapse", 0.01f);
        }

    }

    private IEnumerator Collapse(float waitTime)
    {
        while (true)
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalTransform + Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else
            {
                StopAllCoroutines();   
                transform.localPosition = originalTransform;
                Rigidbody2D rb = transform.AddComponent<Rigidbody2D>();
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                transform.parent.GetComponent<FallingPlatformHolder>().SpawnNewPlatform();
                transform.SetParent(null);
                Invoke("DestroyPlatform", 5);
            }
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void DestroyPlatform()
    {
        if(transform.childCount == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Invoke("DestroyPlatform", 5);
        }
    }
}
