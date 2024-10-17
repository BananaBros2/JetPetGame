using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // If other object is the player
        {
            try
            {
                GameObject checkpoint = GameObject.FindGameObjectWithTag("GameManager");  // Find GameManager object
                checkpoint.GetComponent<GameManager>().respawnPosition = transform.GetChild(0).position; // Change the respawn location for when the scene next (re)loads
            }
            catch
            {
                Debug.LogError("Can't find checkpoint!"); // Debug message
            }
        }

    }
}
