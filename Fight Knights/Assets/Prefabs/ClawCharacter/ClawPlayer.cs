using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawPlayer : PlayerController
{
    [SerializeField] GameObject swordSlash, swordThrustParticle, swordThrustSword, swordSlashSword, slash, vinesPrefab, vinesInstantiated;
    [SerializeField] Transform swordCrit, thrustPosition;
    float snakeGrabRange = 15f;
    float grabSpeed = 100f;
    bool canDashAgain = true;
    float dashAnimationTime = .25f;

    protected override void Update()
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
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
                HandleMovement();
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
                break;
        }
    }

    protected override void HandleMovement()
    {
        movement.x = inputMovement.x;
        movement.z = inputMovement.y;
        if (movement.x != 0 || movement.z != 0)
        {
            lastMoveDir = movement;
        }
        if (animatorUpdated != null)
        {
            if (!shielding && state == State.Normal)
            {
                animatorUpdated.SetFloat("MoveSpeed", (movement.magnitude));
            }
            else
            {
                animatorUpdated.SetFloat("MoveSpeed", (0));
            }
        }
    }

    protected override void HandleThrowingHands()
    {
        
        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }
        if (leftHandTransform.localPosition.x < 4f)
        {
            if (opponent != null && opponent.state == State.Grabbed)
            {
                opponent.state = State.Normal;
                opponent = null;
            }
        }
       

        if (punchedLeft && returningLeft == false)
        {
            animatorUpdated.SetBool("Rolling", false);
            punchedLeftTimer = 0;
            //leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(snakeGrabRange, -.4f, -.4f), (grabSpeed) * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange)
            {
                leftHandTransform.gameObject.GetComponent<Collider>().enabled = true;
            }
            if (leftHandTransform.localPosition.x >= snakeGrabRange)
            {
                
                returningLeft = true;
            }
        }
        if (returningLeft)
        {
            punchedLeft = false;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(0, 0, 0), 30f * Time.deltaTime);

            if (leftHandTransform.localPosition.x <= 1f)
            {
                //leftHandCollider.enabled = false;
            }
            if (leftHandTransform.localPosition.x <= 0f)
            {
                returningLeft = false;
            }
            
        }



        if (punchedRight && returningRight == false)
        {
           

            animatorUpdated.SetBool("Rolling", false);
            punchedRightTimer = 0;
            //rightHandCollider.enabled = true;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                if (swordSlash != null)
                {
                    slash = Instantiate(swordSlash, GrabPosition.position, Quaternion.identity);
                    slash.transform.right = transform.right;
                    HandleCollider handleCollider = slash.GetComponent<HandleCollider>();
                    handleCollider.SetPlayer(this, leftHandParent);
                }

                returningRight = true;
            }
        }
        if (returningRight)
        {
            punchedRight = false;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);

            if (rightHandTransform.localPosition.x <= 1f)
            {
                //rightHandCollider.enabled = false;
            }
            if (rightHandTransform.localPosition.x <= 0f)
            {
                returningRight = false;
            }
        }

        if (punchedRight)
        {
            moveSpeed = 0;
        }

        if (punchedLeft || returningLeft)
        {
            moveSpeed = 0f;
        }
        if (!punchedLeft && !punchedRight && !returningLeft)
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

    protected override void Dash(Vector3 dashDirection)
    {
        isDashing = true;
        shielding = false;
        vinesInstantiated = Instantiate(vinesPrefab, new Vector3(GrabPosition.position.x, 0f, GrabPosition.position.z), Quaternion.identity);
        vinesInstantiated.GetComponent<HandleColliderShieldBreak>().SetPlayer(this, leftHandParent);
        StartCoroutine(DashCooldown(3f));
        canDash = false;
        dashAnimationTime = .25f;
        state = State.Dashing;
    }

    IEnumerator DashCooldown(float cd)
    {
        yield return new WaitForSecondsRealtime(cd);
        canDash = true;
    }
    protected override void HandleDash()
    {
        dashAnimationTime -= Time.deltaTime;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        if (dashAnimationTime <= 0f)
        {
            isDashing = false;
            state = State.Normal;
        }
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


        //check if hasdashedtimer is good to go if not return

        //then if dash buffer is greater than 0 dash
        if (dashBuffer > 0)
        {
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }
            dashBuffer = 0;
            animatorUpdated.SetTrigger("Dash");
            Dash(transform.right);
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

        if (state == State.Stunned) return;
        if (returningLeft || punchedLeft) return;
        if (punchedRight) return;
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

        if (state == State.Stunned) return;
        if (punchedLeft) return;
        if (returningRight || punchedRight) return;
        if (state == State.WaveDahsing && rb.linearVelocity.magnitude > 10f) return;
        
        if (state == State.Knockback) return;
        if (state == State.Dashing) return;
        if (state == State.Grabbed) return;
        if (punchedRightTimer > 0)
        {
            if (lookDirection.magnitude != 0 && !returningLeft && !punchedLeft)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }
            if (shielding) shielding = false;
            punchedRight = true;
            punchedRightTimer = 0;
        }
    }
    protected override void FaceLookDirection()
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        if (punchedLeft || punchedRight || returningLeft) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;
        if (grabbing) return;

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0f)
        {
            lastLookedPosition = lookTowards;
        }

        Look();
    }

}
