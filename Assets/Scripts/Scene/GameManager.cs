using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector2 respawnPosition;

    // Start is called before the first frame update
    void Awake()
    {
        if (GameObject.FindGameObjectWithTag("GameManager") != null)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            transform.tag = "GameManager";
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (respawnPosition != new Vector2(0, 0))
        {

            GameObject.FindGameObjectWithTag("Player").transform.position = respawnPosition;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
