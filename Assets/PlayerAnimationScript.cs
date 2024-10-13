using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAnimationScript : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public bool flipped;
    public bool animating;


    public float fastFallVelocity;

    public void AnimationChange(string action, bool grounded, int movDir, float velX, float velY)
    {
        animator.SetBool("Grounded", grounded);
        animator.SetFloat("VelocityY", velY);
        animator.SetFloat("AbsoluteVelocityX", Mathf.Abs(velX));

        if (!grounded && ((movDir == 1 && flipped) || (movDir == -1 && !flipped)))
        {
            flipped = !flipped;
            spriteRenderer.flipX = flipped;
        }

        if (action == "Death") // Death
        {

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



            if (movDir > 0 && flipped) //Turn right
            {
                animator.SetTrigger("TurnAround");
                flipped = false;
                spriteRenderer.flipX = false;
                print("turn right");
            }
            else if (movDir < 0 && !flipped) // Turn left
            {
                animator.SetTrigger("TurnAround");
                flipped = true;
                spriteRenderer.flipX = true;
                print("turn left");
            }

            if (movDir > 0) // Walk right
            {
                animator.SetBool("Moving", true);
                //print("right");
            }
            else if (movDir < 0) // Walk left
            {
                animator.SetBool("Moving", true);
                //print("left");
            }
            else
            {
                animator.SetBool("Moving", false);
            }

        }

        animator.SetBool("UsingJet", false);
        animator.SetBool("JetSideways", false);
        animator.SetBool("JetFail", false);

        if (action == "Jet") // Vertical Jet Actions
        {
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
