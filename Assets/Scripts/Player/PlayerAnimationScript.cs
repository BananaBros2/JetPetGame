using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAnimationScript : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private Animator flameAnimator;
    private SpriteRenderer spriteRenderer;

    [HideInInspector] public bool flipped;


    private void Start()
    {
        spriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>(); // Sprite Renderer for the player character
        animator = transform.GetChild(1).GetComponent<Animator>(); // Animator for the player character
        flameAnimator = transform.GetChild(2).GetComponent<Animator>(); // Animator for the jet flames
    }

    /// <summary> Function for sending various data to the player animators</summary> 
    /// <param name="action"> The type of action to animate </param>
    /// <param name="grounded"> Whether or not the player is touching the ground </param>
    /// <param name="movDir"> Direction the player is attempting to move towards </param>
    /// <param name="velX"> Player's current velocity on the X-axis </param>
    /// <param name="velY"> Player's current velocity on the Y-axis </param>
    public void AnimationChange(string action, bool grounded, int movDir, float velX, float velY)
    {
        if (animator.GetBool("Died")) { return; }

        animator.SetBool("Grounded", grounded); // Set basic movement animator bools
        animator.SetFloat("VelocityY", velY);
        animator.SetFloat("AbsoluteVelocityX", Mathf.Abs(velX));
        animator.SetBool("Moving", (Mathf.Abs(movDir) > 0) ? true : false);


        if (!grounded && ((movDir == 1 && flipped) || (movDir == -1 && !flipped)) && action != "JetSideways")
        {
            flipped = !flipped; // Will flip player's sprite to face direction currently walking towards if they are not grounded or Horizontally jetting
            spriteRenderer.flipX = flipped;
        }
        

        if (action == "Climb" && Mathf.Abs(velY) > 0) // Actively Climbing
        {
            animator.SetBool("Moving", true);
            animator.SetBool("Climbing", true);
        }
        else if (action == "Climb") // Climbing no movement
        {
            animator.SetBool("Moving", false);
            animator.SetBool("Climbing", true);
        }
        else // Not climbing
        {
            animator.SetBool("Climbing", false);
        }

        if (grounded)
        {
            if (movDir > 0 && flipped && action != "JetSideways") //Turn right (While not burst jetting horizontally)
            {
                animator.SetTrigger("TurnAround");
                flipped = false;
                spriteRenderer.flipX = false;
            }
            else if (movDir < 0 && !flipped && action != "JetSideways") // Turn left (While not burst jetting horizontally)
            {
                animator.SetTrigger("TurnAround");
                flipped = true;
                spriteRenderer.flipX = true;
            }
        }


        animator.SetBool("UsingJet", false); // Resetting various animator parameters
        animator.SetBool("JetSideways", false); // Certainly not the best way to do this
        animator.SetBool("JetFail", false);
        animator.SetBool("Pickup", false);
        flameAnimator.SetBool("Vertical", false);
        flameAnimator.SetBool("Horizontal", false);

        switch (action)
        {
            case "Death": // Death animation bool
                animator.SetBool("Died", true);
                break;
            case "Pickup":
                animator.SetBool("Pickup", true);
                break;
            case "JetVertical": // Vertical Jet Actions
                flameAnimator.SetBool("Vertical", true);
                animator.SetBool("UsingJet", true);
                break;
            case "JetHorizontal": // Horizontal Jet Actions
                flameAnimator.SetBool("Horizontal", true);
                animator.SetBool("UsingJet", true);
                break;
            case "JetBurst": // Vertical Burst Jet Actions
                flameAnimator.SetTrigger("Burst");
                animator.SetBool("UsingJet", true);
                break;
            case "JetSideways": // Horizontal Burst Jet Actions
                animator.SetBool("JetSideways", true); // Would do 'animator.SetBool(action, true)' to reduce size, but it wouldn't work with other values
                break;
            case "JetFail": // No fuel remaining while attempting to jet
                animator.SetBool("JetFail", true);
                break;
            default:
                break;
        }


    }
}
