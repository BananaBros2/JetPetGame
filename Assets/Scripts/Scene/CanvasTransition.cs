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

    [Header("Stored Status Values")]
    private float cinemachineNormalSpeed; // Variable for saving cinemachine scroll duration value
    private string currentState = "Revive"; // Defaults to revive where if this isn't changed it can be presume player died to trigger reload
    private Vector2 outTargetPosition; // Stores position for "Uncover"

    private string nextScene = "Unknown"; // Given by the NextAreaDoor script
    private int nextSceneDoor;

    private float restartBuffer = 35; // Time after death before screen fades out, decided to hard-code this with the value of 35

    
    private void Start() // Called when first active
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager"); // Gets the GameManager object which handles the scene loading after the transition is complete
        blockSprite = transform.GetComponent<Image>(); // Object that covers the screen and will scroll/fade in/out
        player = GameObject.FindGameObjectWithTag("Player"); // Get the player object so that can reactivate the movement when the opening transition is finished
        cinemachineNormalSpeed = cinemachineBrain.m_DefaultBlend.m_Time; // Save the default scroll speed for the camera
        cinemachineBrain.m_DefaultBlend.m_Time = 0.01f;  // Sets the scroll duration to be very low so that the camera can scroll to the player when a scene is loaded
    }


    /// <summary>Scrolling screen transition that will block game view</summary>
    public void CoverTransition(string requestedScene, int targetDoorID, float direction)
    {
        currentState = "Cover";  // Will be used later in fixedUpdate to create the scrolling transition
        transform.rotation = Quaternion.Euler(0,0,0); // Reset any rotation from the uncover transition when previously loading the scene
        transform.Rotate(0, 0, direction);
        
        // Start off screen in the direction of transform.right which was just changed by the rotation
        transform.position = new Vector2((transform.right.x * Screen.width * 2) + Screen.width / 2, (transform.right.y * Screen.height * 2) + Screen.height / 2);

        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 1); // Reset Opacity to be fully opaque

        nextScene = requestedScene; // Save requested scene values
        nextSceneDoor = targetDoorID;

    }

    /// <summary>Scrolling screen transition that will reveal game view</summary>
    public void RevealTransition(float direction)
    {
        currentState = "Uncover";  // Will be used later in fixedUpdate to create the scrolling transition
        transform.Rotate(0, 0, direction);

        // Travel in the reverse direction so that the UI sprite is visually flipped
        outTargetPosition = new Vector2((-transform.right.x * Screen.width * 2) + Screen.width / 2, (-transform.right.y * Screen.height * 2) + Screen.height / 2);

    }

    /// <summary> Fade-in screen transition that will block game view</summary>
    public void DiedTransition()
    {
        currentState = "Die"; // Will be used to trigger fade in FixedUpdate
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 0); // Set initial opacity to 0, although this will likely already be transparent
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2); // Place blocking image back to the center of the screen
    }
    public void ExitTransition()
    {
        currentState = "ExitStage"; // Will be used to trigger fade in FixedUpdate
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 0); // Set initial opacity to 0, although this will likely already be transparent
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2); // Place blocking image back to the center of the screen
    }

    /// <summary> Fade-out screen transition that will reveal game view</summary>
    public void DiedRevealTransition()
    {
        currentState = "Revive"; // Will be used to trigger fade in FixedUpdate, of course you could use coroutines but this method works fine
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 1); // Set initial opacity to max so that it can then be reduced
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2); // Place blocking image back to the center of the screen

    }


    private void FixedUpdate()
    {
        if (currentState == "FinishedTransition") { return; } // Prevent switch statement from firing if not required

        switch (currentState)
        {
            case "Uncover": 
                if (Mathf.Abs(transform.position.x - outTargetPosition.x) > 1 || Mathf.Abs(transform.position.y - outTargetPosition.y) > 1) // If at target point with minor flexibility
                {
                    transform.position = Vector2.MoveTowards(transform.position, outTargetPosition, scrollHideSpeed); // Move away from screen center
                }
                else
                {
                    cinemachineBrain.m_DefaultBlend.m_Time = cinemachineNormalSpeed; // Reset the camera speed to normal
                    player.GetComponent<PlayerMovement>().disableInput = false; // Renable the player's ability to move
                    currentState = "FinishedTransition";
                }
                break;
            case "Cover": // INVOLVES SCENE LOAD
                if (Mathf.Abs(transform.position.x - Screen.width / 2) > 1 || Mathf.Abs(transform.position.y - Screen.height / 2) > 1) // If covering screen with minor flexibility
                {
                    transform.position = Vector2.MoveTowards(transform.position, new Vector2(Screen.width/2, Screen.height/2), scrollHideSpeed); // Move towards screen center
                }
                else
                {
                    gameManager.GetComponent<GameManager>().targetDoorID = nextSceneDoor; // Transfer over requested entrance information
                    gameManager.GetComponent<GameManager>().scrollDirection = transform.rotation.eulerAngles.z; // Transfer over rotation so next uncover transition can match this
                    gameManager.GetComponent<GameManager>().LoadScene(nextScene); // Initiate loading to the next requested scene
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
                restartBuffer--; // Time before fade out starts, just so that dying doesn't instantly restart and the user can analyse a bit where they went wrong
                if (restartBuffer == 30) // After 5 fixedframes will hide the player character's sprites, syncs up with the explosion which will hide this
                {
                    player.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                    player.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false; 
                }
                if (restartBuffer < 0) // Wait time over
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
            default:
                Debug.LogError("Unknown Canvas State: " + currentState); // Debugging Message
                break;
        }
    }

    private void Update()
    {
        if (currentState == "ExitStage")
        {
            if (blockSprite.color.a < 1)  // Keep increasing the opacity of the blocking object until no longer visible
            {
                blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, blockSprite.color.a + opacityIncreaseRate * (Time.deltaTime+0.00001f * 10000)); // Reduce Opacity by set value
            }
            else
            {
                Time.timeScale = 1;
                gameManager.GetComponent<GameManager>().LoadScene("MainMenu");
                Destroy(this); // Destroy this script off the object so that it doesn't try to load the next scene multiple times.
            }
        }
    }

}
