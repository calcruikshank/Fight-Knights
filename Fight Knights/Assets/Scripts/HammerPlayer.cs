using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerPlayer : PlayerController
{
    [SerializeField] GameObject HammerPrefab;
    [SerializeField] GameObject HammerInHand;
    [SerializeField] GameObject LightningBall;
    GameObject ThrownHammer;
    GameObject lightningBallInstantiated;
    Rigidbody ThrownHammerRB;
    float hammerSpeed = 90f;
    float returnRightHammerSpeed;
    public bool returnHammer = false;
    Vector3 oppositeHammerForce;


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
            if (state != State.Dashing)
            {
                animatorUpdated.SetBool("Flying", (false));
            }
        }
    }
    protected override void HandleThrowingHands()
    {
        if (animatorUpdated != null && ThrownHammer == null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            //animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }
        else
        {
            animatorUpdated.SetBool("punchingRight", (false));
        }
        animatorUpdated.SetBool("punchingLeft", (punchedLeft));
        animatorUpdated.SetBool("returningLeft", (returningLeft));

        if (punchedLeft && returningLeft == false)
        {

            punchedLeftTimer = 0;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange)
            {
                lightningBallInstantiated = Instantiate(LightningBall, leftHandTransform.transform.position, transform.rotation);
                lightningBallInstantiated.GetComponent<LightningBall>().SetPlayer(this);
                returningLeft = true;
            }
        }
        if (returningLeft)
        {
            punchedLeft = false;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);


            if (leftHandTransform.localPosition.x <= 0f)
            {
                returningLeft = false;
            }
        }



        if (punchedRight && returningRight == false && ThrownHammer == null)
        {

            punchedRightTimer = 0;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRangeRight, -.4f, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRangeRight)
            {
                ThrownHammer = Instantiate(HammerPrefab, GrabPosition.position, transform.rotation);

                ThrownHammer.GetComponent<ThrownHammer>().SetPlayer(this);
                ThrownHammerRB = ThrownHammer.GetComponent<Rigidbody>();

                ThrownHammerRB.AddForce((transform.right) * (hammerSpeed), ForceMode.Impulse);
                HammerInHand.SetActive(false);
                returningRight = true;
            }
        }
        if (returningRight)
        {
            punchedRight = false;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);

            if (rightHandTransform.localPosition.x <= 0f)
            {

                returningRight = false;
            }
        }

        if (ThrownHammer == null)
        {

            returnHammer = false;
        }
        if (returnHammer && ThrownHammer != null)
        {
            if (state == State.Knockback)
            {
                Debug.Log("True");
            }
            if (oppositeHammerForce == Vector3.zero)
            {
                oppositeHammerForce = -ThrownHammerRB.velocity;
            }
            if (ThrownHammerRB.velocity.magnitude > 10f)
            {
                //Debug.Log("-hammer velocity = " + oppositeHammerForce);
                ThrownHammerRB.AddForce(oppositeHammerForce * 5 * Time.deltaTime, ForceMode.Impulse);
            }
            if (ThrownHammerRB.velocity.magnitude <= 10f)
            {
                ThrownHammerRB.velocity = Vector3.zero;

                returnRightHammerSpeed += 300 * Time.deltaTime;
                ThrownHammerRB.transform.position = Vector3.MoveTowards(ThrownHammerRB.transform.position, HammerInHand.transform.position, returnRightHammerSpeed * Time.deltaTime);
            }
            if (ThrownHammerRB.transform.position == HammerInHand.transform.position)
            {
                HammerInHand.SetActive(true);
                Destroy(ThrownHammer);
                returnHammer = false;
            }
        }



        if (punchedLeft || punchedRight || returningLeft || returningRight)
        {
            moveSpeed = moveSpeedSetter;
        }
        if (!punchedLeft && !punchedRight && !returningLeft && !returningRight)
        {
            moveSpeed = moveSpeedSetter;
        }
        if (shielding)
        {
            moveSpeed = 0;
        }

        if (state == State.Normal)
        {
            returnSpeed = 15f;
        }
        if (returningLeft && returningRight)
        {
            returnSpeed = 6f;
        }
        if (state == State.Dashing)
        {
            returnSpeed = 6f;
        }
    }

    

    protected override void Dash(Vector3 dashDirection)
    {
        if (animatorUpdated != null && ThrownHammer == null)
        {
            animatorUpdated.SetBool("punchingRight", (true));
            //animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }
        isDashing = true;
        shielding = false;
        state = State.Dashing;

    }
    protected override void HandleDash()
    {
        if (ThrownHammerRB == null)
        {
            punchedRightTimer = 0;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRangeRight, -.4f, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRangeRight)
            {
                if (animatorUpdated != null && ThrownHammer == null)
                {
                    animatorUpdated.SetBool("Flying", (true));
                    //animatorUpdated.SetBool("punchingLeft", (punchedLeft));

                }
                ThrownHammer = Instantiate(HammerPrefab, GrabPosition.position, transform.rotation);

                ThrownHammer.GetComponent<ThrownHammer>().SetPlayer(this);
                ThrownHammerRB = ThrownHammer.GetComponent<Rigidbody>();

                ThrownHammerRB.AddForce((transform.right) * (hammerSpeed / 1.5f), ForceMode.Impulse);
                HammerInHand.SetActive(false);
                returningRight = true;
            }

        }
        if (ThrownHammerRB != null)
        {
            
            rb.velocity = ThrownHammerRB.velocity;
        }
        if (returnHammer && ThrownHammer != null)
        {
            if (oppositeHammerForce == Vector3.zero)
            {
                oppositeHammerForce = -ThrownHammerRB.velocity;
            }
            if (ThrownHammerRB.velocity.magnitude > 10f)
            {
                //Debug.Log("-hammer velocity = " + oppositeHammerForce);
                ThrownHammerRB.AddForce(oppositeHammerForce * 5 * Time.deltaTime, ForceMode.Impulse);
            }
            if (ThrownHammerRB.velocity.magnitude <= 10f)
            {
                ThrownHammerRB.velocity = Vector3.zero;

                returnRightHammerSpeed += 300 * Time.deltaTime;
                ThrownHammerRB.transform.position = Vector3.MoveTowards(ThrownHammerRB.transform.position, HammerInHand.transform.position, returnRightHammerSpeed * Time.deltaTime);
            }
            if (ThrownHammerRB.transform.position == HammerInHand.transform.position)
            {
                if (animatorUpdated != null)
                {
                    animatorUpdated.SetBool("Flying", (false));
                    //animatorUpdated.SetBool("punchingLeft", (punchedLeft));

                }
                HammerInHand.SetActive(true);
                Destroy(ThrownHammer);
                returnHammer = false;
                state = State.Normal;
            }
        }
    }




    public override void EndPunchRight()
    {
        punchedRight = false;
        returningRight = true;

        if (ThrownHammer != null)
        {
            returnHammer = true;
            oppositeHammerForce = -ThrownHammerRB.velocity;
            ThrownHammerRB.velocity = Vector3.zero;
            returnRightHammerSpeed = 0;
        }
    }
    protected override void CheckForPunchRight()
    {

        if (releasedRight && state != State.Dashing)
        {
            punchedRightTimer -= Time.deltaTime;
            if (ThrownHammer != null && !returnHammer)
            {
                returnHammer = true;
                oppositeHammerForce = -ThrownHammerRB.velocity;
                returnRightHammerSpeed = 0;
            }
        }
        if (pressedRight)
        {
            punchedRightTimer = inputBuffer;
            pressedRight = false;
        }
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;
        if (punchedLeft) return;
        if (returnHammer) return;
        if (returningRight) return;
        if (state == State.Knockback) return;
        if (state == State.Dashing) return;
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
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;
        if (returningLeft) return;
        if (state == State.Knockback) return;
        if (lightningBallInstantiated != null) return;
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
            if (ThrownHammer != null && !returnHammer)
            {
                if (state == State.Dashing) returnHammer = true;
                
                oppositeHammerForce = -ThrownHammerRB.velocity;
                returnRightHammerSpeed = 0;
            }
        }

        //return area
        if (state != State.Normal) return;
        if (state == State.Dashing) return;
        if (state == State.Stunned) return;
        if (!canDash) return;
        if (ThrownHammerRB != null) return;
        if (state == State.Grabbed) return;
        if (returningRight) return;
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
            Dash(transform.right.normalized);
        }
    }
}
