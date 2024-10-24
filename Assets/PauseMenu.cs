using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseObject;
    public CanvasTransition transitioner;
    public GameObject exitButton;

    private float normalTimeScale = 1;

    // Start is called before the first frame update

    void Start()
    {
        pauseObject.SetActive(false);

#if !UNITY_EDITOR && UNITY_WEBGL
        Destroy(exitButton);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            FlipPause();
        }
    }

    public void TimePauseAdjustment(bool paused)
    {
        if (paused)
        {
            normalTimeScale = Time.timeScale;
            Time.timeScale = 0.0001f;
        }
        else
        {
            Time.timeScale = normalTimeScale;
        }
    }

    public void FlipPause()
    {
        pauseObject.SetActive(!pauseObject.activeInHierarchy);
        TimePauseAdjustment(pauseObject.activeInHierarchy);
    }

    public void ExitToMenu()
    {
        transitioner.ExitTransition();
        Destroy(pauseObject);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
