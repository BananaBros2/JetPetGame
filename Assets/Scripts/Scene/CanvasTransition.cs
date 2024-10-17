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
    public float scrollRevealSpeed = 4500; // Scroll Transitions
    public float scrollHideSpeed = 4500;
    public float opacityIncreaseRate = 0.08f; // Fade (Death) Transitions
    public float opacityDecreaseRate = 0.08f;

    private float cinemachineNormalSpeed; // Variable for saving cinemachine scroll duration value
    private string currentState = "Revive";
    private string nextScene = "Unknown";


    public float endDestination = 1000; // Temporary Solution


    private void Start() // Called when first active
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager"); // Gets the GameManager object which handles the scene loading after the transition is complete
        blockSprite = transform.GetComponent<Image>(); // Object that covers the screen and will scroll/fade in/out
        player = GameObject.FindGameObjectWithTag("Player"); // Get the player object so that can reactivate the movement when the opening transition is finished

        cinemachineNormalSpeed = cinemachineBrain.m_DefaultBlend.m_Time; // Save the default scroll speed for the camera
        cinemachineBrain.m_DefaultBlend.m_Time = 0.01f;  // Sets the scroll duration to be very low so that the camera can scroll to the player when a scene is loaded
    }

    /// <summary>Scrolling screen transition that will reveal game view</summary>
    public void RevealTransition()
    {
        currentState = "Uncover"; // Will be used later in fixedUpdate to create the scrolling transition
    }

    /// <summary>Scrolling screen transition that will block game view</summary>
    public void CoverTransition(string requestedScene)
    {
        transform.position = new Vector2(transform.position.x, endDestination + Screen.height);
        nextScene = requestedScene;
        currentState = "Cover";
       
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
            case "Uncover": // If you are reading this, then I forgot to do something ######################################################
                if (transform.position.y > -Screen.height / 2 - endDestination)
                {
                    transform.position = new Vector2(transform.position.x, transform.position.y - scrollRevealSpeed * Time.deltaTime);
                }
                else
                {
                    cinemachineBrain.m_DefaultBlend.m_Time = cinemachineNormalSpeed;
                    player.GetComponent<PlayerMovement>().disableInput = false;
                }
                break;
            case "Cover":
                if (transform.position.y > Screen.height / 2)
                {
                    transform.position = new Vector2(transform.position.x, transform.position.y - scrollHideSpeed * Time.deltaTime);
                }
                else
                {
                    gameManager.GetComponent<GameManager>().LoadScene("skibidi");
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
                }
                break;
            case "Die":
                if (blockSprite.color.a < 1)  // Keep increasing the opacity of the blocking object until no longer visible
                {
                    blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, blockSprite.color.a + opacityIncreaseRate); // Reduce Opacity by set value
                }
                else
                {
                    gameManager.GetComponent<GameManager>().ReloadScene();
                    Destroy(this); // Destroy this script off the object so that it doesn't try to load the next scene multiple times.
                }
                break;
            default:
                Debug.LogError("Unknown Canvas State: " + currentState); // Debugging Message
                break;
        }




    }

}
