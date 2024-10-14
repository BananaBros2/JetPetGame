using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CanvasTransition : MonoBehaviour
{
    private GameObject gameManager;
    private Image blockSprite;
    private GameObject player;

    public CinemachineBrain cinemachineBrain;
    private float cinemachineNormalSpeed;

    public bool activatedInput;

    public float revealSpeed;
    public float endDestination = 1000;

    private string currentState = "Revive";


    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        blockSprite = transform.GetComponent<Image>();
        player = GameObject.FindGameObjectWithTag("Player");

        cinemachineNormalSpeed = cinemachineBrain.m_DefaultBlend.m_Time;
        cinemachineBrain.m_DefaultBlend.m_Time = 0.01f;
    }

    public void RevealTransition()
    {
        currentState = "Uncover";
    }

    public void OutTransition()
    {
        transform.position = new Vector2(transform.position.x, endDestination + Screen.height);
        currentState = "Cover";
    }


    public void DiedTransition()
    {
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 0);
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        currentState = "Die";
    }

    public void DiedRevealTransition()
    {
        blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, 1);
        transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        currentState = "Revive";
    }


    private void FixedUpdate()
    {
        if (currentState == "Uncover")
        {
            if (transform.position.y > -Screen.height / 2 - endDestination)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - revealSpeed * Time.deltaTime);
            }
            else if (!activatedInput)
            {
                cinemachineBrain.m_DefaultBlend.m_Time = cinemachineNormalSpeed;
                player.GetComponent<PlayerMovement>().disableInput = false;
            }
        }
        else if (currentState == "Cover")
        {
            if (transform.position.y > Screen.height / 2)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - revealSpeed * Time.deltaTime);
            }
            else
            {
                gameManager.GetComponent<GameManager>().LoadNextScene();
                Destroy(this);
            }
        }
        else if (currentState == "Revive")
        {
            if (blockSprite.color.a > 0)
            {
                blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, blockSprite.color.a - 0.08f);
            }
            else if (!activatedInput)
            {
                cinemachineBrain.m_DefaultBlend.m_Time = cinemachineNormalSpeed;
                player.GetComponent<PlayerMovement>().disableInput = false;
            }
        }
        else if (currentState == "Die")
        {
            if (blockSprite.color.a < 1)
            {
                blockSprite.color = new Color(blockSprite.color.r, blockSprite.color.g, blockSprite.color.b, blockSprite.color.a + 0.08f);
            }
            else
            {
                gameManager.GetComponent<GameManager>().LoadNextScene();
                Destroy(this);
            }
        }

    }

}
