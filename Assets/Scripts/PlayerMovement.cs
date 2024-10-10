using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public CameraVibration cameraShaker;

    public LayerMask Ground;
    public LayerMask Special;

    public float groundDetectionRange = 0.5f;

    private bool grounded;
    private int coyoteTime;
    private int lastInput = 0;
    public float jumpHeight = 5;
    public float moveSpeed = 30;

    private float boostCurPower = 0;
    private bool jetpackShaking;

    private bool climbing;
    public float timeBeforeNextLadder = 0.2f;
    private float timeBetweenLadders;

    public int chargesObtained = 1;
    public float fuelRemaining = 100;

    private SpriteRenderer batteryRenderer;
    public List<Sprite> singleChargeSprites = new List<Sprite>();
    public List<Sprite> doubleChargeSprites = new List<Sprite>();
    public List<Sprite> tripleChargeSprites = new List<Sprite>();

    


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); 
        batteryRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Sprite Renderer for jetpack battery

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        grounded = groundCheck();


        RaycastHit2D specialGrounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRange / 5, 0.05f), 0, -transform.up, 0.5f, Special);
        if (specialGrounded)
        {
            if (climbing && Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") < 0)
            {
                this.transform.parent.GetComponent<LadderValues>().ActivatePlatform();
                this.transform.SetParent(null);
                climbing = false;
                timeBetweenLadders = timeBeforeNextLadder;
                return;
            }
            else if (!climbing && Input.GetAxisRaw("Vertical") < 0 && timeBetweenLadders < 0)
            {
                climbing = true;
                this.transform.SetParent(specialGrounded.transform.parent);
                this.transform.parent.GetComponent<LadderValues>().DeactivatePlatform();
            }

        }


        if (lastInput > 0)
        {
            if (coyoteTime > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight);
                lastInput = 0;
                coyoteTime = 0;
            }
        }
        else if (Input.GetButton("Jump"))
        {
            if (coyoteTime > 0 || grounded)
            {
                
                rb.velocity = new Vector2(rb.velocity.x * 0.8f, jumpHeight);
                lastInput = 0;
                coyoteTime = 0;
            }
            else
            {
                lastInput = 5;
            }
        }


        if (Input.GetButtonDown("AbilityThree"))
        {
            //cameraShaker.ShakeOnceStart();
            //chargesObtained++;
            print("epic");
        }
    }

    private void FixedUpdate()
    {
        coyoteTime--;
        lastInput--;
        timeBetweenLadders-= Time.fixedDeltaTime;

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

            rb.velocity = new Vector2(0, 5 * Input.GetAxisRaw("Vertical"));

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


        float horizontalInput = Input.GetAxisRaw("Horizontal");


        if (grounded)
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

        if (Input.GetButton("AbilityOne") && !grounded && rb.velocity.y < 5 && fuelRemaining > 0)
        {
            boostCurPower = Mathf.Clamp(boostCurPower + 0.08f, 0, 1);
            rb.AddForce(new Vector2(0, 50 * boostCurPower));
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -7, 6));
            
            if(!jetpackShaking)
            {
                cameraShaker.ConstantShakeStart();
            }
            jetpackShaking = true;

            updateJetFuel(-1.5f);
        }
        else
        {
            boostCurPower = 0;
        }

        if (Input.GetButton("AbilityTwo"))
        {
            //rb.gravityScale = 0;
        }

    }

    private bool groundCheck()
    {
        bool grounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRange, 0.05f), 0, -transform.up, 0.5f, Ground);

        if (!climbing && !grounded)
        { grounded = Physics2D.BoxCast(transform.position, new Vector2(groundDetectionRange, 0.05f), 0, -transform.up, 0.48f, Special); }
        if (rb.velocity.y > 0.01f && grounded)
        { grounded = false; }

        
        if (grounded || climbing)
        {
            if(fuelRemaining < 100*chargesObtained)
            {
                updateJetFuel(1);
            }

            coyoteTime = 8;
        }

        return grounded;
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


    public List<Vector3> mostExtremeForces;
    int exForceStep = -1;

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
        if (this.transform.parent != null && !climbing)
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

                for (int i = 0; i < 10; i++) { mostExtremeForces[i] = Vector3.zero; }
                exForceStep = -1;
                rb.velocity = new Vector2(rb.velocity.x + mostExtremeForce.x, rb.velocity.y + mostExtremeForce.y);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Climbable") && Input.GetAxisRaw("Vertical") == 1 && timeBetweenLadders < 0)
        {
            climbing = true;
            this.transform.SetParent(collision.transform);
            this.transform.parent.GetComponent<LadderValues>().DeactivatePlatform();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (this.transform.parent != null)
        {
            this.transform.parent.GetComponent<LadderValues>().ActivatePlatform();

            rb.velocity = Vector3.zero;
            this.transform.SetParent(null);
            climbing = false;
        }
    }

    private void PreserveVelocities(MovingObject movingObject)
    {
        exForceStep++;
        if (exForceStep == 10) { exForceStep = 0; }
        float exForceX = movingObject.displacement.x / Time.fixedDeltaTime;
        float exForceY = movingObject.displacement.y / Time.fixedDeltaTime;

        Vector3 exVelocity = new Vector3(exForceX, exForceY, Mathf.Abs(exForceX + Input.GetAxisRaw("Horizontal")) + Mathf.Abs(exForceY + Input.GetAxisRaw("Vertical")));
        mostExtremeForces[exForceStep] = exVelocity;
    }
}
