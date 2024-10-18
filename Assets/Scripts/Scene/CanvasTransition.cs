using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CanvasTransition : MonoBehaviour
{
    [Header("References")]
    private GameObject gameManager;
    private Image blockSprite;
    private GameObject player;
    public CinemachineBrain cinemachineBrain;

    [Header("Transition Speeds")]
    [Range(20, 300)] public float scrollRevealSpeed = 100; // Scroll Transitions
    [Range(20, 300)] public float scrollHideSpeed = 100;
    [Range(0.01f, 1)] public float opacityIncreaseRate = 0.08f; // Fade (Death) Transitions
    [Range(0.01f, 1)] public float opacityDecreaseRate = 0.08f;

    private float cinemachineNormalSpeed; // Variable for saving cinemachine scroll duration value
    private string currentState = "Revive";
    private string nextScene = "Unknown";


    private float restartBuffer = 35; // Time after death before screen fades out, decided to hard-code this with the value of 35

    private int nextSceneDoor;
    private Vector2 outTargetPosition;


    private void Start() // Called when first active
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager"); // Gets the GameManager object which handles the scene loading after the transition is complete
        blockSprite = transform.GetComponent<Image>(); // Object that covers the screen and will scroll/fade in/out
        player = GameObject.FindGameObjectWithTag("Player"); // Get the player object so that can reactivate the movement when the opening transition is finished
        cinemachineNormalSpeed = cinemachineBrain.m_DefaultBlend.m_Time; // Save the default scroll speed for the camera
        cinemachineBrain.m_DefaultBlend.m_Time = 0.01f;  // Sets the scroll duration to be very low so that the camera can scroll to the player when a scene is loaded
    }











    /// <summary>Scrolling screen transition that will reveal game view</summary>
    public void RevealTransition(float direction)
    {
        currentState = "Uncover";  // Will be used later in fixedUpdate to create the scrolling transition

        transform.Rotate(0, 0, direction);
        outTargetPosition = new Vector2((-transform.right.x * Screen.width * 2) + Screen.width / 2, (-transform.right.y * Screen.height * 2) + Screen.height / 2);

    }

    /// <summary>Scrolling screen transition that will block game view</summary>
    public void CoverTransition(string requestedScene, int targetDoorID, float direction)
    {
        currentState = "Cover";  // Will be used later in fixedUpdate to create the scrolling transition

        transform.rotation = Quaternion.Euler(0,0,0); // Reset rotation from last transition
        transform.Rotate(0, 0, direction);
        transform.position = new Vector2((transform.right.x * Screen.width * 2) + Screen.width / 2, (transform.right.y * Screen.height * 2) + Screen.height / 2);

        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 1); // Reset Opacity to be fully opaque

        nextScene = requestedScene;
        nextSceneDoor = targetDoorID;

    }









    /// <summary> Fade-in screen transition that will block game view</summary>
    public void DiedTransition()
    {
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 0); // Set initial opacity to 0, although this will likely already be transparent
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        currentState = "Die";
    }

    /// <summary> Fade-out screen transition that will reveal game view</summary>
    public void DiedRevealTransition()
    {
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 1); // Set initial opacity to max so that it can then be reduced
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        currentState = "Revive";
    }


    private void FixedUpdate()
    {
        switch (currentState)
        {
            case "Uncover": 
                if (Mathf.Abs(transform.position.x - outTargetPosition.x) > 1 || Mathf.Abs(transform.position.y - outTargetPosition.y) > 1)
                {
                    transform.position = Vector2.MoveTowards(transform.position, outTargetPosition, scrollHideSpeed);
                }
                else
                {
                    cinemachineBrain.m_DefaultBlend.m_Time = cinemachineNormalSpeed;
                    player.GetComponent<PlayerMovement>().disableInput = false;
                    currentState = "FinishedTransition";
                }
                break;
            case "Cover":
                if (Mathf.Abs(transform.position.x - Screen.width / 2) > 1 || Mathf.Abs(transform.position.y - Screen.height / 2) > 1)
                {
                    transform.position = Vector2.MoveTowards(transform.position, new Vector2(Screen.width/2, Screen.height/2), scrollHideSpeed);
                }
                else
                {
                    gameManager.GetComponent<GameManager>().targetDoorID = nextSceneDoor;
                    gameManager.GetComponent<GameManager>().scrollDirection = transform.rotation.eulerAngles.z;
                    gameManager.GetComponent<GameManager>().LoadScene(nextScene);
                    Destroy(this); // Destroy this script off the object so that it doesn't try to load the next scene multiple times.
                }
                break;
            case "Revive":
                if (blockSprite.color.a > 0) // Keep reducing the opacity of the blocking object until it is fully hidden
                {
                    blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, blockSprite.color.a - opacityDecreaseRate); // Increase Opacity by set value
                }
                else
                {
                    cinemachineBrain.m_DefaultBlend.m_Time = cinemachineNormalSpeed; // Reset the camera speed to normal
                    player.GetComponent<PlayerMovement>().disableInput = false; // Renable the player's ability to move
                    currentState = "FinishedTransition";
                }
                break;
            case "Die":
                restartBuffer--;
                if (restartBuffer == 30) 
                {
                    player.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                    player.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false; 
                }
                if (restartBuffer < 0)
                {
                    if (blockSprite.color.a < 1)  // Keep increasing the opacity of the blocking object until no longer visible
                    {
                        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, blockSprite.color.a + opacityIncreaseRate); // Reduce Opacity by set value
                    }
                    else
                    {
                        gameManager.GetComponent<GameManager>().ReloadScene();
                        Destroy(this); // Destroy this script off the object so that it doesn't try to load the next scene multiple times.
                    }
                }
                break;
            case "FinishedTransition":
                break;
            default:
                Debug.LogError("Unknown Canvas State: " + currentState); // Debugging Message
                break;
        }




    }

}
