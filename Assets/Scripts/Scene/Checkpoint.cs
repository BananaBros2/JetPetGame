using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            try
            {
                GameObject checkpoint = GameObject.FindGameObjectWithTag("GameManager");
                checkpoint.GetComponent<GameManager>().respawnPosition = transform.GetChild(0).position;
            }
            catch
            {
                Debug.LogError("Can't find checkpoint!");
            }
        }

    }
}
