using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public GameObject exitButton;
    public GameObject startButton;
    public GameObject continueButton;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectWithTag("GameManager") != null) // Checks if there is already a GameManager loaded from previous scene
        {
            Destroy(startButton);
        }
        else
        {
            Destroy(continueButton);
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        Destroy(exitButton);
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Area1", LoadSceneMode.Single); // Load the first scene
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
