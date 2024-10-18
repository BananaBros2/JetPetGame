using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Player Progress")]
    public int chargesObtained = 1;
    public bool jet1Obtained = false;
    public bool jet2Obtained = false;
    public bool jet3Obtained = false;
    public bool jet4Obtained = false;
    public bool boomerangObtained = false;

    [Header("Respawn Information")]
    public Vector2 respawnPosition; // The last position the player activated a checkpoint at

    [Header("Scene Change Information")]
    [HideInInspector] public int targetDoorID = -1;
    public float scrollDirection = 0;
    private CanvasTransition canvasTransition;

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
        if (targetDoorID != -1)
        {
            canvasTransition = GameObject.FindGameObjectWithTag("Transition").GetComponent<CanvasTransition>();
            canvasTransition.RevealTransition(scrollDirection);

            foreach (GameObject entrance in GameObject.FindGameObjectsWithTag("Entrance"))
            {
                if (entrance.GetComponent<NextAreaDoor>().doorID == targetDoorID)
                {
                    GameObject.FindGameObjectWithTag("Player").transform.position = entrance.transform.GetChild(0).position;
                    break;
                }
            }

            targetDoorID = -1;
            return;
        }
        else if (respawnPosition != new Vector2(0, 0)) // If respawnPosition has not yet been changed, Only applicable at the start of the game
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = respawnPosition; // Relocate the player to the last saved checkpoint
        }

    }

    /// <summary> Load a scene through it's name</summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single); // Load the scene from the given name
    }

    /// <summary> Reload the currently actively scene </summary>
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single); // Reload the currently active scene
    }

    /// <summary> Briefly freeze the game </summary>
    public IEnumerator FreezeFrame(float duration)
    {
        yield return new WaitForSeconds(0.005f);

        Time.timeScale = 0.001f;
        yield return new WaitForSeconds(0.0001f * duration);
        Time.timeScale = 1;
    }
}
