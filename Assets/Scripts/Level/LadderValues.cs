using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderValues : MonoBehaviour
{

    GameObject topPlatform;
    GameObject bottomPlatform;

    private void Awake()
    {
        topPlatform = transform.GetChild(0).gameObject;
        bottomPlatform = transform.GetChild(1).gameObject;
        bottomPlatform.SetActive(false);
    }

    public void ActivatePlatform()
    {
        topPlatform.SetActive(true);
        bottomPlatform.SetActive(false);
    }

    public void DeactivatePlatform()
    {
        topPlatform.SetActive(false);
        bottomPlatform.SetActive(true);
    }
}
