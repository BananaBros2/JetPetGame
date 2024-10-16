using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAnimationScript : MonoBehaviour
{
    public Animator animator;
    public Animator flameAnimator;

    public SpriteRenderer spriteRenderer;
    public bool flipped;
    public bool animating;


    public float fastFallVelocity;

    public void AnimationChange(string action, bool grounded, int movDir, float velX, float velY)
    {
        if (animator.GetBool("Died")) { return; }


        animator.SetBool("Grounded", grounded);
        animator.SetFloat("VelocityY", velY);
        animator.SetFloat("AbsoluteVelocityX", Mathf.Abs(velX));

        if (!grounded && ((movDir == 1 && flipped) || (movDir == -1 && !flipped)) && action != "JetSideways")
        {
            flipped = !flipped;
            spriteRenderer.flipX = flipped;
        }

        if (action == "Death") // Death
        {
            animator.SetBool("Died", true);
        }
        if (action == "Pickup") // Pickup
        {

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
        else
        {
            animator.SetBool("Climbing", false);
        }

        if (velY > 0 && !grounded) // Jump/Going up
        {

        }
        else if (action == "FastFall" && velY < fastFallVelocity && !grounded ) // Fast Fall
        {

        }
        else if (velY < 0 && !grounded) // Falling
        {

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

            if (movDir > 0) // Walk right
            {
                animator.SetBool("Moving", true);
            }
            else if (movDir < 0) // Walk left
            {
                animator.SetBool("Moving", true);
            }
            else
            {
                animator.SetBool("Moving", false);
            }

        }

        animator.SetBool("UsingJet", false);
        animator.SetBool("JetSideways", false);
        animator.SetBool("JetFail", false);
        flameAnimator.SetBool("Vertical", false);
        flameAnimator.SetBool("Horizontal", false);

        if (action == "JetVertical") // Vertical Jet Actions
        {
            flameAnimator.SetBool("Vertical", true);
            animator.SetBool("UsingJet", true);
        }
        else if (action == "JetHorizontal") // Vertical Jet Actions
        {
            flameAnimator.SetBool("Horizontal", true);
            animator.SetBool("UsingJet", true);
        }
        else if (action == "JetBurst") // Vertical Jet Actions
        {
            flameAnimator.SetTrigger("Burst");
            animator.SetBool("UsingJet", true);
        }
        else if (action == "JetSideways") // Vertical Instant Boost (Jetpack Dash)
        {
            animator.SetBool("JetSideways", true);
        }
        else if (action == "JetFail") // No fuel left
        {
            animator.SetBool("JetFail", true);
        }


    }
}
