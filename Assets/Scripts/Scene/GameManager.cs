using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector2 respawnPosition; // The last position the player activated a checkpoint at


    void Awake() // Important that this activates before any other object, otherwise other objects may fail to find any GameManager
    {
        if (GameObject.FindGameObjectWithTag("GameManager") != null) // Checks if there is already a GameManager loaded from previous scene
        {
            Destroy(this.gameObject); // Destroy self so that only the original remains
            return;
        }
        else // If no other GameManager is located
        {
            DontDestroyOnLoad(this.gameObject); // Persist between scenes so that information stored can be saved
            transform.tag = "GameManager"; // Add the tag to the object so that other future GameManagers can locate this by tag

        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (respawnPosition != new Vector2(0, 0)) // If respawnPosition has not yet been changed
        {

            GameObject.FindGameObjectWithTag("Player").transform.position = respawnPosition; // Relocate the player to the last saved checkpoint
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single); // Load the scene from the given name
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single); // Reload the currently active scene
    }

}
