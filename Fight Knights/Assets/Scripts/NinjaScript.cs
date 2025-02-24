using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaScript : PlayerController
{
    float startThrowTime = .158f;
    float wholeAnimationTime = .375f;
    float totalTimeThroughThrow;
    [SerializeField] float shurikenSpeed, kunaiSpeed = 80f;
    [SerializeField] GameObject shurikenPrefab, kunaiPrefab;
    GameObject shurikenInstantiated, kunaiInstantiated;
    [SerializeField] Transform shuripos2, shuripos3;
    [SerializeField]Collider bodyCollider;
    bool recoveringFromDash = false;

    float currentDashSpeed;
    Vector3 dashTowards;

    NinjaDashCollider ndc;

    public override void Awake()
    {
        base.Awake();
        ndc = GetComponentInChildren<NinjaDashCollider>();
    }
    protected override void Update()
    {

        switch (state)
        {
            case State.Normal:
                HandleMovement();
                HandleThrowingHands();
                HandleShielding();
                HandleAButton();
                break;
            case State.Knockback:
                HandleKnockback();
                HandleThrowingHands();
                HandleShielding();
                break;
            case State.ParryState:
                HandleParry();
                HandleShielding();
                break;
            case State.PowerDashing:
                HandlePowerDashing();
                HandleShielding();
                HandleThrowingHands();
                HandleAButton();
                break;
            case State.WaveDahsing:
                HandleWaveDashing();
                HandleShielding();
                HandleThrowingHands();
                break;
            case State.ParryStunned:
                HandleShielding();
                HandleParryStunned();
                break;
            case State.Dashing:
                HandleDash();
                HandleThrowingHands();
                break;
            case State.Grabbed:
                HandleGrabbed();
                HandleThrowingHands();
                HandleShielding();
                break;
            case State.Grabbing:
                HandleGrabbing();
                break;
            case State.AirDodging:
                HandleAirDodge();
                HandleKnockback();
                break;
            case State.Stunned:
                HandleStunned();
                HandleThrowingHands();
                HandleShielding();
                break;
        }

        CheckForInputs();
        FaceLookDirection();
        if (state != State.Dashing)
        {
            if (bodyCollider.isTrigger == true || ndc.hasNDCed()) //checks if the collider hasnt been changed from a trigger and if the ninja is ignoring a collider. If he is ignoring a collider he will turn off the ignore
            {
                rb.useGravity = true;
                ndc.TurnOnCollisions();
                bodyCollider.isTrigger = false;
            }
        }
    }

    protected override void FixedUpdate()
    {

        switch (state)
        {
            case State.Normal:
                FixedHandleMovement();
                break;
            case State.PowerDashing:
                FixedHandlePowerDashing();
                break;
            case State.WaveDahsing:
                FixedHandleWaveDashing();
                break;
            case State.Dashing:
                //FixedHandleMovement();
                FixedHandleDash();
                break;
        }
    }

    protected override void HandleThrowingHands()
    {

        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }

        if (punchedLeft && !returningRight)
        {
            totalTimeThroughThrow += Time.deltaTime;
            if (totalTimeThroughThrow >= startThrowTime)
            {
                kunaiInstantiated = Instantiate(kunaiPrefab, GrabPosition.position, GrabPosition.rotation);
                kunaiInstantiated.GetComponent<Rigidbody>().AddForce(kunaiInstantiated.transform.right * kunaiSpeed, ForceMode.Impulse);
                kunaiInstantiated.GetComponent<HandleCollider>().SetPlayer(this, this.rightHandTransform);
                punchedLeft = false;
                returningRight = true;
            }
        }

        if (punchedRight && !returningRight)
        {
            totalTimeThroughThrow += Time.deltaTime;
            if (totalTimeThroughThrow >= startThrowTime)
            {
                shurikenInstantiated = Instantiate(shurikenPrefab, GrabPosition.position, GrabPosition.rotation);
                shurikenInstantiated.GetComponent<Rigidbody>().AddForce(shurikenInstantiated.transform.right * shurikenSpeed, ForceMode.Impulse);
                shurikenInstantiated.GetComponent<HandleCollider>().SetPlayer(this, this.rightHandTransform);

                shurikenInstantiated = Instantiate(shurikenPrefab, shuripos2.position, shuripos2.rotation);
                shurikenInstantiated.GetComponent<Rigidbody>().AddForce(shurikenInstantiated.transform.right * shurikenSpeed, ForceMode.Impulse);
                shurikenInstantiated.GetComponent<HandleCollider>().SetPlayer(this, this.rightHandTransform);

                shurikenInstantiated = Instantiate(shurikenPrefab, shuripos3.position, shuripos3.rotation);
                shurikenInstantiated.GetComponent<Rigidbody>().AddForce(shurikenInstantiated.transform.right * shurikenSpeed, ForceMode.Impulse);
                shurikenInstantiated.GetComponent<HandleCollider>().SetPlayer(this, this.rightHandTransform);

                punchedRight = false;
                returningRight = true;
            }
        }

        if (returningRight)
        {
            totalTimeThroughThrow += Time.deltaTime;
            if (totalTimeThroughThrow >= wholeAnimationTime)
            {
                returningLeft = false;
                returningRight = false;
            }
        }


        if (punchedLeft || punchedRight)
        {
            moveSpeed = 0;
        }

        if (!punchedLeft && !punchedRight)
        {
            moveSpeed = moveSpeedSetter;
        }
        if (shielding)
        {
            moveSpeed = 0;
        }

        if (state == State.Normal)
        {
            returnSpeed = 8f;
        }
        if (returningLeft && returningRight)
        {

            returnSpeed = 4f;
        }
        if (state == State.Dashing)
        {

            returnSpeed = 4f;
        }
    }

    protected override void CheckForPunchLeft()
    {
        if (releasedLeft)
        {
            punchedLeftTimer -= Time.deltaTime;
        }
        if (pressedLeft)
        {
            punchedLeftTimer = inputBuffer;
            pressedLeft = false;
        }
        if (state != State.Normal && state != State.PowerDashing) return;
        if (state == State.Stunned) return;
        if (returningLeft || punchedLeft) return;
        if (punchedRight || returningRight) return;
        if (state == State.WaveDahsing && rb.linearVelocity.magnitude > 10f) return;
        if (state == State.Knockback) return;
        if (state == State.Grabbed) return;
        if (punchedLeftTimer > 0)
        {
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }


            if (shielding) shielding = false;
            punchedLeft = true;
            punchedLeftTimer = 0;
            totalTimeThroughThrow = 0f;
        }
    }
    protected override void CheckForPunchRight()
    {
        if (releasedRight)
        {
            punchedRightTimer -= Time.deltaTime;
        }
        if (pressedRight)
        {
            punchedRightTimer = inputBuffer;
            pressedRight = false;
        }
        if (state != State.Normal && state != State.PowerDashing && state != State.Dashing) return;
        if (state == State.Stunned) return;
        if (returningLeft || punchedLeft) return;
        if (returningRight || punchedRight) return;
        if (state == State.WaveDahsing && rb.linearVelocity.magnitude > 10f) return;
        if (state == State.Knockback) return;
        if (state == State.Dashing && currentDashSpeed > 40f) return;
        if (state == State.Grabbed) return;
        if (punchedRightTimer > 0)
        {
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }

            if (shielding) shielding = false;
            punchedRight = true;
            punchedRightTimer = 0;
            totalTimeThroughThrow = 0f;
        }
    }
    protected override void Dash(Vector3 dashDirection)
    {
        shielding = false;
        currentDashSpeed = 100f;
        dashTowards = dashDirection;
        rb.useGravity = false;
        state = State.Dashing;
        bodyCollider.isTrigger = true;
        SetAnimatorToIdle();
    }
    protected void FixedHandleDash()
    {
        /*float yVelo;
        if (rb.velocity.y < 0)
        {
            yVelo = rb.velocity.y;
        }
        else
        {
            yVelo = 0f;
        }*/
        rb.linearVelocity = new Vector3(dashTowards.x * currentDashSpeed, rb.linearVelocity.y, dashTowards.z * currentDashSpeed);
        
    }
    protected override void HandleDash()
    {
        if (currentDashSpeed > moveSpeedSetter)
            bodyCollider.isTrigger = true;
        float powerDashSpeedMulti = 6f;
        currentDashSpeed -= currentDashSpeed * powerDashSpeedMulti * Time.deltaTime;

        float powerDashMinSpeed = 5f;
        if (currentDashSpeed < powerDashMinSpeed)
        {
            state = State.Normal;
        }
        if (currentDashSpeed < moveSpeedSetter)
        {
            if (bodyCollider.isTrigger == true)
            {
                rb.useGravity = true;
                bodyCollider.isTrigger = false;
            }
        }
    }

    protected override void FaceLookDirection()
    {
        if (punchedLeft || punchedRight || returningRight || returningLeft) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;
        if (state == State.Dashing) return;

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0)
        {
            lastLookedPosition = lookTowards;
        }

        Look();
    }
    protected override void CheckForDash()
    {
        if (state != State.Dashing && isDashing)
        {
            isDashing = false;
        }
        //if you press dash set dash buffer = to input buffer
        if (pressedDash)
        {
            dashBuffer = inputBuffer;
            pressedDash = false;
        }

        //when dash button is released subtract time from dashbuffer so it only goes down when youre not pressing dash
        if (releasedDash)
        {
            dashBuffer -= Time.deltaTime;
        }

        //return area
        if (state != State.Normal) return;
        if (state == State.Dashing) return;
        if (state == State.Stunned) return;
        if (grabbing) return;
        if (state == State.Grabbed) return;
        if (punchedLeft || returningLeft || punchedRight) return;


        //check if hasdashedtimer is good to go if not return

        //then if dash buffer is greater than 0 dash
        if (dashBuffer > 0)
        {
            if (lookDirection.magnitude != 0)
            {
                if (!returningRight)
                {
                    Debug.Log("ChangeLookDirection");
                    Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                    transform.right = lookTowards;
                }
            }
            dashBuffer = 0;
            Dash(transform.right);
        }
    }
    

    public void EndDash()
    {
        state = State.Normal;
        if (bodyCollider.isTrigger == true)
        {
            rb.useGravity = false;
            ndc.TurnOnCollisions();
            bodyCollider.isTrigger = false;
        }
    }
}
