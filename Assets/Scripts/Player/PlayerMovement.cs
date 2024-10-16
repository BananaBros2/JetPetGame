using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public CameraVibration cameraShaker;
    public CanvasTransition transition;

    public LayerMask Ground;
    public LayerMask Special;

    public bool disableInput = true;

    [Range(0.1f, 0.99f)] public float groundDetectionRangeX = 0.5f; // Values need to be hard-coded more-so but good for debugging
    [Range(0.1f, 0.99f)] public float groundDetectionRangeY = 0.53f; // Same as above

    private bool grounded;
    private int curCoyoteTime;
    [Range(1, 50)] public int coyoteTime = 8;

    private int lastInput = 0;
    [Range(1, 50)] public int lastInputLimit = 5;
    public float jumpHeight = 5;
    public float moveSpeed = 30;
    public float climbingSpeed = 3;
    private float boostCurPower = 0;
    private bool jetpackShaking;

    [Range(1, 10)] public int ladderDistDivision = 4;
    private bool climbing;
    public float timeBeforeNextLadder = 0.2f;
    private float timeBetweenLadders;

    public int chargesObtained = 1;
    public float fuelRemaining = 100;

    private SpriteRenderer batteryRenderer;
    public List<Sprite> singleChargeSprites = new List<Sprite>();
    public List<Sprite> doubleChargeSprites = new List<Sprite>();
    public List<Sprite> tripleChargeSprites = new List<Sprite>();

    public GameObject boomerang;

    private PlayerAnimationScript animationScript;
    private string animationAction;

    public List<Vector3> mostExtremeForces;
    int exForceStep = -1;

    private bool canNextJet = true;
    public Vector3 verticalBurstForce = new Vector3(25, 30, 34);
    public Vector3 horizontalBurstForce = new Vector3(15, 20, 25);
    private int horizontalBurstLeft;
    private string jetTypeUsing = "None";

    private void Awake() // Called once when first existent 
    {
        rb = GetComponent<Rigidbody2D>(); // Get and set rb as the rigidbody2d component
        batteryRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Sprite Renderer for jetpack battery
        animationScript = GetComponent<PlayerAnimationScript>(); // Animator on character sprite
    }

    
    void Update() // Update is called once per frame
    {
        if (disableInput) { return; } // Stop all immediate controls when disableInput is true

        if (Input.GetButtonDown("Restart")) // Restart functionality 
        {
            KillPlayer(); 
            return; // Prevents any accidental inputs before disableInput activates
        }


        grounded = groundCheck(); // Set 'grounded' variable as the value returned from groundCheck(), better for perfomance then calling groundCheck() everytime it's needed
        if ((grounded || climbing) && !Input.GetButton("AbilityOne") && !Input.GetButton("AbilityTwo")) // Top up coyote Timer and Start refueling jetpack if on the ground or a ladder and not currently Horizontal Jetting
        {

            if (fuelRemaining < 100 * chargesObtained) // Only refuels up to current maximum
            {
                updateJetFuel(1*chargesObtained);
            }

            curCoyoteTime = coyoteTime; // CoyoteTimer reset
        }

        RaycastHit2D specialGrounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRangeX / ladderDistDivision, 0.05f), 0, -transform.up, groundDetectionRangeY, Special);
        if (specialGrounded) // Check if colliding with a 'special' layer object
        {
            if (climbing && Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") < 0) // If down is pressed after touching bottom platform of ladder 
            {
                this.transform.parent.GetComponent<LadderValues>().ActivatePlatform(); // Removes platform stopping you from falling and reenables top platform
                this.transform.SetParent(null); // Detach from the ladder
                climbing = false;
                timeBetweenLadders = timeBeforeNextLadder; // Stop the player from accidentally grabbing the same ladder after leaving it
                return;
            }
            else if (!climbing && Input.GetAxisRaw("Vertical") < 0 && timeBetweenLadders < 0) // If on top platform and down is pressed
            {
                climbing = true;
                this.transform.SetParent(specialGrounded.transform.parent);
                this.transform.parent.GetComponent<LadderValues>().DeactivatePlatform();
            }

        }


        if (lastInput > 0) // Inititates a jump if the jump was pressed within the last n (lastInputLimit) fixedframes, good for an instant jump when landing
        {
            if (curCoyoteTime > 0) // Can jump even if in midair as long as they were grounded within n (curCoyoteTime) fixedframes ago
            {                     // curCoyoteTime is constantly reset to max when grounded so will jump 100% of the time when grounded

                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight); // Add upwards velocity, tweaked further in FixedUpdate()
                lastInput = 0; // Resets lastinput so another is required before instant jumping again
                curCoyoteTime = 0; // Resets coyote jumping until next landing
            }
        }
        else if (Input.GetButton("Jump")) // If didn't have jump input saved, will instead take this route
        {
            if (curCoyoteTime > 0 || grounded) // If grounded or just left ground with coyoteTime remaining
            {
                
                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight); // Add upwards velocity
                lastInput = 0; // Resets lastinput so another is required before instant jumping again
                curCoyoteTime = 0; // Resets coyote jumping until next landing
            }
            else
            {
                lastInput = lastInputLimit; // Saves jump input if unable to currently jump (Jump buffering)
            }
        }


        if (Input.GetButtonDown("AbilityOne") && Input.GetButton("Alternative") && fuelRemaining > 0 && canNextJet)
        {

            float burstPower = Mathf.Clamp(Mathf.CeilToInt(fuelRemaining / 33), 1, 3);

            if (burstPower >= 3) { rb.velocity = new Vector2(rb.velocity.x, verticalBurstForce.z); }
            else if (burstPower == 2) { rb.velocity = new Vector2(rb.velocity.x, verticalBurstForce.y); }
            else { rb.velocity = new Vector2(rb.velocity.x, verticalBurstForce.x); }

            cameraShaker.ShakeOnceStart();

            updateJetFuel(-100);

            rb.gravityScale = 8;

            jetTypeUsing = "VBurst";
            canNextJet = false;

            animationAction = "JetBurst";
        }
        if(rb.velocity.y < 0 && jetTypeUsing == "VBurst")
        {
            canNextJet = true;
            jetTypeUsing = "None";
        }


        if (Input.GetButtonDown("AbilityTwo") && Input.GetButton("Alternative") && fuelRemaining > 0 && canNextJet)
        {
            int direction = animationScript.flipped ? -1 : 1;
            float burstPower = Mathf.Clamp(Mathf.CeilToInt(fuelRemaining / 33), 1, 3);
            
            if (burstPower >= 3) { rb.velocity = new Vector2(horizontalBurstForce.z * direction, 0); }
            else if (burstPower == 2) { rb.velocity = new Vector2(horizontalBurstForce.y * direction, 0); }
            else { rb.velocity = new Vector2(horizontalBurstForce.x * direction, 0); }

            cameraShaker.ShakeOnceStart();

            updateJetFuel(-100);

            rb.gravityScale = 0;

            jetTypeUsing = "HBurst";
            canNextJet = false;

            animationAction = "JetSideways";
        }
        else if (Mathf.Abs(rb.velocity.x) < 3 && jetTypeUsing == "HBurst")
        {
            canNextJet = true;
            jetTypeUsing = "None";
        }



        if (Input.GetButtonDown("AbilityThree"))
        {
            BoomerangScript newBoomerang = Instantiate(boomerang, transform.position, Quaternion.identity).GetComponent<BoomerangScript>();
            newBoomerang.player = transform.gameObject;

            int horizontalDirection = 0;
            if (Input.GetAxisRaw("Vertical") == 0)
            {
                horizontalDirection = transform.GetComponent<PlayerAnimationScript>().flipped ? -1 : 1;
            }
            newBoomerang.direction = new Vector2(horizontalDirection, Input.GetAxisRaw("Vertical")).normalized;
            print("epic");
        }

    }


    private void FixedUpdate()
    {
        if (disableInput) 
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.7f, rb.velocity.y);
            return; 
        }

        curCoyoteTime--;
        lastInput--;
        timeBetweenLadders-= Time.fixedDeltaTime;
        horizontalBurstLeft--;

        if (jetpackShaking)
        {
            if(!(Input.GetButton("AbilityOne") && !grounded && rb.velocity.y < 5 && fuelRemaining > 0))
            {
                cameraShaker.ConstantShakeCancel();
                jetpackShaking = false;
            }
        }

        if (climbing && transform.parent != null)
        {
            animationScript.AnimationChange("Climb", grounded, 0, rb.velocity.x, rb.velocity.y);

            transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.parent.position.x, transform.position.y), 5 * Time.deltaTime);
            rb.gravityScale = 0;
            if (Input.GetButton("Jump"))
            {
                this.transform.parent.GetComponent<LadderValues>().ActivatePlatform();
                this.transform.SetParent(null);
                climbing = false;
                timeBetweenLadders = 0.5f;


                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight);
                return;
            }

            rb.velocity = new Vector2(0, climbingSpeed * Input.GetAxisRaw("Vertical"));

            if (grounded && Input.GetAxisRaw("Vertical") < 0)
            {
                this.transform.parent.GetComponent<LadderValues>().ActivatePlatform();
                this.transform.SetParent(null);
                climbing = false;
                timeBetweenLadders = 0.5f;
                return;
            }

            return;
        }

        if (canNextJet)
        {
            if (Input.GetAxis("Vertical") < 0 && rb.velocity.y < 4 && !grounded)
            {
                rb.gravityScale = 5;
            }
            else if (rb.velocity.y < 0 && rb.velocity.y < 2 && !grounded)
            {
                rb.gravityScale = 3;
            }
            else if (!Input.GetButton("Jump") && rb.velocity.y > 4)
            {
                rb.gravityScale = 8;
            }
            else
            {
                rb.gravityScale = 1.75f;
            }
        }


        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (jetTypeUsing == "HBurst")
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.85f, rb.velocity.y);
        }
        else if (jetTypeUsing == "JetHorizontal")
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed * 1.4f, 0));
            rb.velocity = new Vector2(rb.velocity.x * 0.85f, rb.velocity.y);
        }
        else if (grounded)
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed, 0));
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, rb.velocity.y);
        }
        else if (Mathf.Abs(rb.velocity.x) > 6 && horizontalInput < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.93f, rb.velocity.y);
        }
        else if (horizontalInput == 0)
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed, 0));
            rb.velocity = new Vector2(rb.velocity.x * 0.93f, rb.velocity.y);
        }
        else if (Mathf.Abs(rb.velocity.x) < 6)
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed, 0));
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, rb.velocity.y);
        }




        // Jetpack

        if (Input.GetButton("AbilityOne") && !grounded && rb.velocity.y < 5 && fuelRemaining > 1 && canNextJet)
        {
            boostCurPower = Mathf.Clamp(boostCurPower + 0.08f, 0, 1);
            rb.AddForce(new Vector2(0, 50 * boostCurPower));
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -7, 6));

            animationAction = "JetVertical";

            if (!jetpackShaking)
            {
                cameraShaker.ConstantShakeStart();
            }
            jetpackShaking = true;

            
            updateJetFuel(-1.5f);

            jetTypeUsing = "JetVertical";
        }
        else if (Input.GetButton("AbilityOne") && fuelRemaining <= 0 && jetTypeUsing != "VBurst")
        {
            animationAction = "JetFail";
        }
        else if (jetTypeUsing == "JetVertical") // Resets jetTypeUsing and built up power if no longer applicable
        {
            jetTypeUsing = "None";
            boostCurPower = 0;
        }



        if (Input.GetButton("AbilityTwo") && canNextJet && fuelRemaining > 1 && !grounded)
        {
            boostCurPower += 0.1f;
            rb.gravityScale = Mathf.Max(3 - boostCurPower, 0);
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.8f);

            animationAction = "JetHorizontal";

            if (!jetpackShaking)
            {
                cameraShaker.ConstantShakeStart();
            }
            jetpackShaking = true;


            updateJetFuel(-1f);

            jetTypeUsing = "JetHorizontal";
        }
        else if (Input.GetButton("AbilityTwo") && fuelRemaining <= 0 && jetTypeUsing != "HBurst")
        {
            animationAction = "JetFail";
        }
        else if (jetTypeUsing == "JetHorizontal") // Resets jetTypeUsing if no longer applicable
        {
            jetTypeUsing = "None";
        }




        if (jetTypeUsing == "None")
        {
            animationAction = "None";
        }
        animationScript.AnimationChange(animationAction, grounded, (int)horizontalInput, rb.velocity.x, rb.velocity.y);
        if(animationAction == "JetBurst")
        {
            animationAction = "None";
        }
    }

    private bool groundCheck() // Function for checking if the player is stood on top of an object in the 'Ground' layer
    {
        bool grounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRangeX, 0.05f), 0, -transform.up, groundDetectionRangeY, Ground);

        if (!climbing && !grounded) // If not on determined to be 'grounded', then also check if on 'Special' layer when not climbing
        { grounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRangeX, 0.05f), 0, -transform.up, groundDetectionRangeY, Special); }
        
        if (rb.velocity.y > 0.01f && grounded) // Revoke grounded if player has an upwards velocity, stops platform chain-jumping
        { grounded = false; }

        return grounded; // Returns the final determined bool
    }

    private void updateJetFuel(float value)
    {
        fuelRemaining = Math.Clamp(fuelRemaining + value, 0, 100 * chargesObtained);

        switch (chargesObtained)
        {
            case 1:
                batteryRenderer.sprite = singleChargeSprites[(int)Mathf.Round(fuelRemaining / 33.3f + 0.4f)];
                break;
            case 2:
                batteryRenderer.sprite = doubleChargeSprites[(int)Mathf.Round(fuelRemaining / 33.3f + 0.4f)];
                break;
            default:
                batteryRenderer.sprite = tripleChargeSprites[(int)Mathf.Round(fuelRemaining / 33.3f + 0.4f)];
                break;
        }


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out MovingObject movingObject) && groundCheck() && collision.enabled)
        {
            this.transform.SetParent(collision.transform);
        }
        if (collision.transform.TryGetComponent<FallingPlatform>(out FallingPlatform platform) && collision.enabled)
        {
            platform.AttemptCollapse();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.transform.TryGetComponent(out MovingObject movingObject) && groundCheck() && collision.enabled)
        {
            this.transform.SetParent(collision.transform);

            PreserveVelocities(movingObject);

            if (collision.transform.TryGetComponent<FallingPlatform>(out FallingPlatform platform))
            {
                platform.AttemptCollapse();
            }

        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if (this.transform.parent != null && !climbing) // If exited an object while not climbing and is a child of an object
        {

            this.transform.SetParent(null);

            if (collision.transform.TryGetComponent(out MovingObject movingObject) && !grounded)
            {
                PreserveVelocities(movingObject); // In case player never 'stays' on the platform for more than 1 frame
                
                Vector3 mostExtremeForce = Vector3.zero;
                foreach (Vector3 force in mostExtremeForces)
                {
                    if (force.z > mostExtremeForce.z)
                    { mostExtremeForce = force; }
                }

                for (int i = 0; i < mostExtremeForces.Count; i++) { mostExtremeForces[i] = Vector3.zero; }
                exForceStep = -1;
                rb.velocity = new Vector2(rb.velocity.x + mostExtremeForce.x, rb.velocity.y + mostExtremeForce.y);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard") && !disableInput) // If entering a object with the 'Hazard' tag then kill the player
        {
            KillPlayer();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Climbable") && Input.GetAxisRaw("Vertical") == 1 && timeBetweenLadders < 0) // Checks if applicable to climb a ladder
        {
            climbing = true;
            this.transform.SetParent(collision.transform); // Childs the player to the ladder
            this.transform.parent.GetComponent<LadderValues>().DeactivatePlatform(); // Swaps the collision of the ladder platforms
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (this.transform.parent != null && collision.CompareTag("Climbable")) // Check if applicable to get off ladder
        {
            this.transform.parent.GetComponent<LadderValues>().ActivatePlatform(); // Swaps the ladder platform's collisions

            rb.velocity = Vector3.zero; // Resets the players velocity so that they don't bounce at top of ladder
            this.transform.SetParent(null); // Unparents the player from the ladder
            climbing = false;
        }
    }

    private void PreserveVelocities(MovingObject movingObject) // Stores the last n (10) velocity values from the stood on moving object
    {
        exForceStep++; // Iterate to next 'mostExtremeForces' value for replacement
        if (exForceStep == mostExtremeForces.Count) { exForceStep = 0; } // Loop back to start (oldest values) of list
        float exForceX = movingObject.displacement.x / Time.fixedDeltaTime; // Store the X value of the moving object
        float exForceY = movingObject.displacement.y / Time.fixedDeltaTime; // Store the Y value of the moving object

        Vector3 exVelocity = new Vector3(exForceX, exForceY, // Set the X and Y values of the Vector3 to the ^previous values
            Mathf.Abs(exForceX + Input.GetAxis("Horizontal")) + Mathf.Abs(exForceY + Input.GetAxis("Vertical")));
        // The Z value of the Vector3 is set to the total absolute values along with the players current input to influence the direction 
        mostExtremeForces[exForceStep] = exVelocity; // Replaces the old value with the newly created vector3
    }

    private void KillPlayer() // Kill player 
    {
        animationScript.AnimationChange("Death", true, 0, 0, 0); // Play death animation, other values no longer matter
        disableInput = true; // Remove the player's ability to use inputs
        transition.DiedTransition(); // Order the transition object to play the fade out effect and then reload the scene
    }
}
