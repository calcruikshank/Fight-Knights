using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bichette : PlayerController
{
    [SerializeField] GameObject clonePrefab;
    GameObject cloneInstantiated;

    bool recoveringFromDash = false;

    float dashedTimer;
    float dashedRecoverTimer;
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
    protected override void Dash(Vector3 dashDirection)
    {
        cloneInstantiated = Instantiate(clonePrefab, transform.position, transform.rotation);
        animatorUpdated.SetTrigger("Dash");
        dashedTimer = 0f;
        dashedRecoverTimer = 0f;
        recoveringFromDash = false;
        
            isDashing = true;
            if (shielding) shielding = false;
            //shielding = false;
            punchedRight = false;
            returningRight = false;
            state = State.Dashing;
    }
    protected void FixedHandleDash()
    {
        if (!recoveringFromDash)
        {
            Vector3 newVelocity = new Vector3(transform.right.normalized.x * 40, rb.linearVelocity.y, transform.right.normalized.z * 30);
            rb.linearVelocity = -newVelocity;
        }
        else
        {
            Vector3 newVelocityy = new Vector3(0, rb.linearVelocity.y, 0);
            rb.linearVelocity = newVelocityy;
        }
    }
    protected override void HandleDash()
    {
        dashedRecoverTimer += Time.deltaTime;
        if (dashedRecoverTimer >= .525f && recoveringFromDash == false)
        {
            if (cloneInstantiated != null)
            {
                cloneInstantiated.GetComponent<ExplodeClone>().SetPlayer(this);
                cloneInstantiated.GetComponent<ExplodeClone>().ExplodeTheClone();
            }
            recoveringFromDash = true;
            /*GameObject heavySlash = Instantiate(heavySlashPrefab, GrabPosition.position, Quaternion.identity);
            heavySlash.transform.right = transform.right;
            HandleCollider handleCollider = heavySlash.GetComponent<HandleCollider>();
            handleCollider.SetPlayer(this, leftHandParent);*/
        }
        if (dashedRecoverTimer >= 1.15f)
        {
            state = State.Normal;
            isDashing = false;
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

}
