using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour // This is a very movement-focused game so a lot of the logic is in here
{
    [Header("References")]
    public GameObject boomerang;
    public GameObject explosionPrefab;
    public CameraVibration cameraShaker;
    public CanvasTransition transition;
    private Rigidbody2D rb;
    private PlayerAnimationScript animationScript;
    private SpriteRenderer batteryRenderer;
    private SpriteRenderer boomerangRenderer;
    private GameManager gameManager;


    [Header("Layer Detection")]
    public LayerMask Ground;
    public LayerMask Special;
    [Range(0.1f, 0.99f)] public float groundDetectionRangeX = 0.5f; // Values need to be hard-coded more-so but good for debugging
    [Range(0.1f, 0.99f)] public float groundDetectionRangeY = 0.53f; // Same as above
    private bool grounded;
    

    [Header("Movement")]
    public float moveSpeed = 30;
    public float jumpHeight = 5;
    public float regularGravity = 1.75f;
    public float unheldJumpGravity = 8;
    public float fallingGravity = 3;
    public float fastFallGravity = 5;


    [Header("Climbing")]
    private bool climbing;
    public float climbingSpeed = 3;
    public float snapToLadderSpeed = 1;
    [Range(1, 10)] public int ladderDistDivision = 4;
    public float timeBeforeNextLadder = 0.5f;
    private float secondsBetweenLadders;


    [Header("Jetpack")]
    public float fuelRemaining = 100;
    public float fuelRechargeRate = 1; 
    private float boostCurPower = 0;
    private bool jetpackShaking;
    public Vector3 verticalBurstForce = new Vector3(25, 30, 34);
    public Vector3 horizontalBurstForce = new Vector3(15, 20, 25);
    private bool canNextJet = true;
    private string jetTypeUsing = "None";
    private float horizontalJetSpeedMultiplier = 1.25f;


    [Header("Battery Sprites")]
    public List<Sprite> singleChargeSprites = new List<Sprite>();
    public List<Sprite> doubleChargeSprites = new List<Sprite>();
    public List<Sprite> tripleChargeSprites = new List<Sprite>();


    [Header("Boomerang")]
    public bool boomerangAvailable = true;

    [Header("Forgiveness")]
    [Range(1, 50)] public int coyoteTime = 8;
    private int curCoyoteTime;
    [Range(1, 50)] public int lastInputLimit = 5;
    private int lastInput = 0;
    public List<Vector3> mostExtremeForces;
    private int exForceStep = -1;


    [Header("Items Collected")]
    public bool unlockAllItems;
    public int chargesObtained;
    public bool vThrustObtained;
    public bool hThrustObtained;
    public bool vBurstObtained;
    public bool hBurstObtained;
    public bool boomerangObtained;


    [Header("Miscellaneous")]
    public bool disableInput = true;
    private string animationAction;



    private void Awake() // Called once when first existent 
    {
        rb = GetComponent<Rigidbody2D>(); // Get and set rb as the rigidbody2d component
        batteryRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Sprite Renderer for jetpack battery
        boomerangRenderer = transform.GetChild(3).GetComponent<SpriteRenderer>(); // Sprite Renderer for boomerang
        animationScript = GetComponent<PlayerAnimationScript>(); // Animator on character sprite
    }

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>(); // Get the GameManager
        chargesObtained = gameManager.chargesObtained; // Saved player stats from game manager, able to transfer player progress through scene loads
        vThrustObtained = gameManager.vThrustObtained;
        hThrustObtained = gameManager.hThrustObtained;
        vBurstObtained = gameManager.vBurstObtained;
        hBurstObtained = gameManager.hBurstObtained;
        boomerangObtained = gameManager.boomerangObtained;
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
                updateJetFuel(fuelRechargeRate * chargesObtained);
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
                secondsBetweenLadders = timeBeforeNextLadder; // Stop the player from accidentally grabbing the same ladder after leaving it
                return;
            }
            else if (!climbing && Input.GetAxisRaw("Vertical") < 0 && secondsBetweenLadders < 0) // If on top platform and down is pressed
            {
                climbing = true;
                this.transform.SetParent(specialGrounded.transform.parent); // Attach to ladder from top
                this.transform.parent.GetComponent<LadderValues>().DeactivatePlatform();
            }
        }


        Jump(); // Method for inital jump input with buffering

        VerticalBurstJetpack(); // Single Vertical boost which requires a two button to be pressed
        HorizontalBurstJetpack(); // Single Horizontal boost which requires a two button to be pressed

        if (boomerangAvailable) { ThrowBoomerang(); } // Method for creating the boomerang
        else { boomerangRenderer.color = new Color(1, 1, 1, 0); }

    }

    private void FixedUpdate()
    {
        if (disableInput)
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.7f, rb.velocity.y); // Stop player from sliding when input is disabled
            return;
        }

        curCoyoteTime--; // Countdown timers to various aspects
        lastInput--;
        secondsBetweenLadders -= Time.fixedDeltaTime; // Minor inconsistency, but both fixedDeltaTime timer and fixedframe integer timers work


        if (jetpackShaking) 
        {
            if (!(Input.GetButton("AbilityOne") && !grounded && rb.velocity.y < 5 && fuelRemaining > 0))
            {
                cameraShaker.ConstantShakeCancel(); // Turns off shake if no longer applicable
                jetpackShaking = false;
            }
        }

        if (climbing && transform.parent != null) // CLIMBING
        {
            animationScript.AnimationChange("Climb", grounded, 0, rb.velocity.x, rb.velocity.y); // Climbing Animation
            rb.gravityScale = 0; // Don't apply gravity whilst on ladder

            // Snap to ladder middle
            transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.parent.position.x, transform.position.y), snapToLadderSpeed);

            if (Input.GetButton("Jump")) // If attempting to jump off ladder
            {
                this.transform.parent.GetComponent<LadderValues>().ActivatePlatform();
                this.transform.SetParent(null); // Detach from ladder
                climbing = false;
                secondsBetweenLadders = timeBeforeNextLadder; // Time until another ladder can be grabbed


                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight);
                return; // Don't continue ladder abilities
            }

            // LADDER MOVEMENT ----#
            rb.velocity = new Vector2(0, climbingSpeed * Input.GetAxisRaw("Vertical"));
            // LADDER MOVEMENT ----#

            if (grounded && Input.GetAxisRaw("Vertical") < 0) // If touching ground and continues to press down
            {
                this.transform.parent.GetComponent<LadderValues>().ActivatePlatform(); 
                this.transform.SetParent(null); // Detach from ladder
                climbing = false;
                secondsBetweenLadders = timeBeforeNextLadder; // Time until another ladder can be grabbed
            }

            return; // Don't continue to non-ladder related code
        }

        if (canNextJet) // If not using any jet type which often change gravity to get better trajectories
        {
            if (Input.GetAxis("Vertical") < 0 && rb.velocity.y < 4 && !grounded) // If player is pressing down while jumping (Fast falling)
            {
                rb.gravityScale = fastFallGravity;
            }
            else if (rb.velocity.y < 0 && !grounded) // Increase gravity when falling and not grounded
            {
                rb.gravityScale = fallingGravity;
            }
            else if (!Input.GetButton("Jump") && rb.velocity.y > 4) // Apply larger gravity if player is not pressing jump to get shorter hops
            {
                rb.gravityScale = unheldJumpGravity;
            }
            else // Regular starting gravity
            {
                rb.gravityScale = regularGravity;
            }
        }


        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (jetTypeUsing == "HBurst") // Decelleration when using the Horizontal jet burst
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.85f, rb.velocity.y);
        }
        else if (jetTypeUsing == "JetHorizontal") // Movement and Decelleration when using the horizontal jet, will move faster than normal speed
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed * horizontalJetSpeedMultiplier, 0)); 
            rb.velocity = new Vector2(rb.velocity.x * 0.85f, rb.velocity.y);
        }
        else if (grounded) // Movement and Decelleration when grounded
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed, 0));
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, rb.velocity.y);
        }
        else if (Mathf.Abs(rb.velocity.x) > 6 && horizontalInput < 0) // Movement and Decelleration when going normal speed in the air
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.93f, rb.velocity.y);
        }
        else if (horizontalInput == 0) // Decelleration when not touching the horizontal movement keys
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.93f, rb.velocity.y);
        }
        else if (Mathf.Abs(rb.velocity.x) < 6) // Movement and Decelleration when going faster than normal
        {
            rb.AddForce(new Vector2(horizontalInput * moveSpeed, 0));
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, rb.velocity.y);
        }


        VerticalJetPack(); // Vertical boost which requires constant input
        HorizontalJetpack(); // Horizontal boost which requires constant input


        if (jetTypeUsing == "None") // v Animation Value conversion code v
        {
            animationAction = "None";
        }
        animationScript.AnimationChange(animationAction, grounded, (int)horizontalInput, rb.velocity.x, rb.velocity.y);
        if (animationAction == "JetBurst")
        {
            animationAction = "None";
        }

    }

    /// <summary> Method for player jumping with controllable input buffer </summary>
    private void Jump()
    {
        if (lastInput > 0) // Inititates a jump if the jump was pressed within the last n (lastInputLimit) fixedframes, good for an instant jump when landing
        {
            if (curCoyoteTime > 0) // Can jump even if in midair as long as they were grounded within n (curCoyoteTime) fixedframes ago
            {                     // curCoyoteTime is constantly reset to max when grounded so will jump 100% of the time when grounded

                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight); // Add upwards velocity, tweaked further in FixedUpdate(), reduces horizontal speed slightly so that instant buffered jumps don't store velocity 
                lastInput = 0; // Resets lastinput so another is required before instant jumping again
                curCoyoteTime = 0; // Resets coyote jumping until next landing
            }
        }
        else if (Input.GetButton("Jump") && jetTypeUsing != "HBurst") // If didn't have jump input saved, will instead take this route, 
        {           // prevents jumping while horizontally bursting so that the player can't rise into the air with the lack of gravity

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
    }

    /// <summary> Method for a constant upwards boost </summary>
    private void VerticalJetPack()
    {
        if (!(vThrustObtained || unlockAllItems)) { return; }

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
    }

    /// <summary> Method for a single upwards burst </summary>
    private void VerticalBurstJetpack()
    {
        if (!(vBurstObtained || unlockAllItems)) { return; }

        if (Input.GetButtonDown("AbilityOne") && Input.GetButton("Alternative") && fuelRemaining > 0 && canNextJet)
        {

            float burstPower = Mathf.Clamp(Mathf.CeilToInt(fuelRemaining / 33), 1, 3);

            if (burstPower >= 3) { rb.velocity = new Vector2(rb.velocity.x, verticalBurstForce.z); }
            else if (burstPower == 2) { rb.velocity = new Vector2(rb.velocity.x, verticalBurstForce.y); }
            else { rb.velocity = new Vector2(rb.velocity.x, verticalBurstForce.x); }

            gameManager.GetComponent<GameManager>().StartCoroutine("FreezeFrame", burstPower / 3f);
            cameraShaker.ShakeOnceStart(burstPower * 0.05f);

            updateJetFuel(-100);

            rb.gravityScale = 8;

            jetTypeUsing = "VBurst";
            canNextJet = false;

            animationAction = "JetBurst";
        }
        if (rb.velocity.y < 0 && jetTypeUsing == "VBurst")
        {
            canNextJet = true;
            jetTypeUsing = "None";
        }
    }

    /// <summary> Method for a constant horizontal boost </summary>
    private void HorizontalJetpack()
    {
        if (!(hThrustObtained || unlockAllItems)) { return; }

        if (Input.GetButton("AbilityTwo") && canNextJet && fuelRemaining > 1 && !grounded)
        {
            boostCurPower += 0.1f;
            rb.gravityScale = Mathf.Max(3 - boostCurPower, 0);
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.7f);

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
    }

    /// <summary> Method for a single Horizontal burst (Dash) </summary>
    private void HorizontalBurstJetpack()
    {
        if (!(hBurstObtained || unlockAllItems)) { return; }

        if (Input.GetButtonDown("AbilityTwo") && Input.GetButton("Alternative") && fuelRemaining > 0 && canNextJet)
        {

            int direction = animationScript.flipped ? -1 : 1;
            float burstPower = Mathf.Clamp(Mathf.CeilToInt(fuelRemaining / 33), 1, 3);

            if (burstPower >= 3) { rb.velocity = new Vector2(horizontalBurstForce.z * direction, 0); }
            else if (burstPower == 2) { rb.velocity = new Vector2(horizontalBurstForce.y * direction, 0); }
            else { rb.velocity = new Vector2(horizontalBurstForce.x * direction, 0); }

            gameManager.GetComponent<GameManager>().StartCoroutine("FreezeFrame", burstPower / 3f);
            cameraShaker.ShakeOnceStart(burstPower * 0.05f);

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

    private void ThrowBoomerang()
    {
        if (!(boomerangObtained || unlockAllItems)) 
        {
            return; 
        }

        boomerangRenderer.color = new Color(1, 1, 1, 1); // Make boomerang icon above head visible

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

            boomerangAvailable = false;
        }
    }


    /// <summary> Method for checking if the player is on the ground </summary>
    private bool groundCheck() 
    {
        bool grounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRangeX, 0.05f), 0, -transform.up, groundDetectionRangeY, Ground);

        if (!climbing && !grounded) // If not on determined to be 'grounded', then also check if on 'Special' layer when not climbing
        { grounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRangeX, 0.05f), 0, -transform.up, groundDetectionRangeY, Special); }
        
        if (rb.velocity.y > 0.01f && grounded) // Revoke grounded if player has an upwards velocity, stops platform chain-jumping
        { grounded = false; }

        return grounded; // Returns the final determined bool
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out MovingObject movingObject) && groundCheck() && collision.enabled)
        {
            this.transform.SetParent(collision.transform);
        }
        if (collision.transform.TryGetComponent<FallingPlatform>(out FallingPlatform platform) && collision.enabled)
        {
            platform.AttemptCollapse(); // Initiate falling platforming
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.transform.TryGetComponent(out MovingObject movingObject) && groundCheck() && collision.enabled) 
        {
            this.transform.SetParent(collision.transform);

            PreserveVelocities(movingObject); // Platform velocity buffering

            if (collision.transform.TryGetComponent<FallingPlatform>(out FallingPlatform platform))
            {
                platform.AttemptCollapse(); // Initiate falling platforming
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
                PreserveVelocities(movingObject); // In case player never 'onCollisionStays2D' on the platform for more than 1 frame
                
                Vector3 mostExtremeForce = Vector3.zero; 
                foreach (Vector3 force in mostExtremeForces)
                {
                    if (force.z > mostExtremeForce.z) // Iterate through list and find vector with the highest score
                    { mostExtremeForce = force; }
                }

                for (int i = 0; i < mostExtremeForces.Count; i++) { mostExtremeForces[i] = Vector3.zero; } // Reset all list values to zero
                exForceStep = -1; // Not technically required, but returns index to a point that will start overwriting at the first value again
                rb.velocity = new Vector2(rb.velocity.x + mostExtremeForce.x, rb.velocity.y + mostExtremeForce.y); // Apply the final force
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard") && !disableInput) // If entering a object with the 'Hazard' tag then kill the player
        {
            KillPlayer();
        }
        if(collision.CompareTag("Item") && !disableInput)
        {
            collision.transform.parent = transform;
            collision.transform.localPosition = Vector2.up;
            disableInput = true;
            animationScript.AnimationChange("Pickup", grounded, 0, 0, 0);
            StartCoroutine("PickupItem", collision.gameObject);

            gameManager.itemsCollected.Add(collision.GetComponent<UpgradeItem>().itemID);

            switch (collision.GetComponent<UpgradeItem>().upgradeType) // Update obtained list
            {
                case "Charge":
                    chargesObtained++;
                    gameManager.chargesObtained++;
                    break;
                case "VThrust":
                    vThrustObtained = true;
                    gameManager.vThrustObtained = true;
                    break;
                case "HThrust":
                    hThrustObtained = true;
                    gameManager.hThrustObtained = true;
                    break;
                case "VBurst":
                    vBurstObtained = true;
                    gameManager.vBurstObtained = true;
                    break;
                case "HBurst":
                    hBurstObtained = true;
                    gameManager.hBurstObtained = true;
                    break;
                case "Boomerang":
                    boomerangObtained = true;
                    gameManager.boomerangObtained = true;
                    break;
                default:
                    Debug.LogError("Unknown item: " + collision.GetComponent<UpgradeItem>().upgradeType);
                    break;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Climbable") && Input.GetAxisRaw("Vertical") == 1 && secondsBetweenLadders < 0) // Checks if applicable to climb a ladder
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


    public IEnumerator PickupItem(GameObject item)
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(item);
        disableInput = false;
    }

    /// <summary> Adds the last 10 velocities from a moving object to a list </summary>
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

    /// <summary> Begins all the effects of the player dying </summary>
    private void KillPlayer() // Kill player 
    {
        animationScript.AnimationChange("Death", true, 0, 0, 0); // Play death animation, other values no longer matter
        disableInput = true; // Remove the player's ability to use inputs
        gameManager.GetComponent<GameManager>().StartCoroutine("FreezeFrame", 3);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        rb.velocity = Vector3.zero;
        transition.DiedTransition(); // Order the transition object to play the fade out effect and then reload the scene
    }
}
