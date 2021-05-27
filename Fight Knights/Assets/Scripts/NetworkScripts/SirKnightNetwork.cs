using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SirKnightNetwork : NetworkPlayerController
{
    [SerializeField] GameObject swordSlash, swordThrustParticle, swordThrustSword, swordSlashSword, smokeTeleportPrefab, heavySlashPrefab;
    [SerializeField] Transform swordCrit, thrustPosition;
    bool recoveringFromDash = false;
    float dashDistance;
    [SerializeField] LayerMask wallForDash;
    protected override void Update()
    {
        if (!IsOwner)
        {
            rb.velocity = Vector3.zero;
            state = State.Normal;
            return;
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

        if (!IsOwner)
        {
            rb.velocity = Vector3.zero;
            state = State.Normal;
            return;
        }
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
        if (leftHandTransform.localPosition.x <= 0f)
        {

            swordThrustSword.SetActive(false);
            swordSlashSword.SetActive(true);
        }
        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }
        if (punchedLeft && returningLeft == false)
        {
            swordThrustSword.SetActive(true);
            swordSlashSword.SetActive(false);
            animatorUpdated.SetBool("Rolling", false);
            punchedLeftTimer = 0;
            //leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), (punchSpeed - 20) * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange)
            {
                if (swordSlash != null)
                {

                    SpawnLeftParticleServerRpc(thrustPosition.position, transform.right);
                }
                returningLeft = true;
            }
        }
        if (returningLeft)
        {
            punchedLeft = false;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);

            if (leftHandTransform.localPosition.x <= 1f)
            {

                //leftHandCollider.enabled = false;
            }
            if (leftHandTransform.localPosition.x <= 0f)
            {
                returningLeft = false;
            }
            if (leftHandTransform.localPosition.x <= 2f)
            {
                swordThrustSword.SetActive(false);
                swordSlashSword.SetActive(true);
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

                    SpawnRightParticleServerRpc(GrabPosition.position, transform.right);
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

        if (punchedLeft || punchedRight)
        {
            moveSpeed = moveSpeedSetter / 2;
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

    protected override void Dash(Vector3 dashDirection)
    {
        StartCoroutine(DashRoutine(.525f));
        recoveringFromDash = false;
        if (!CanMove(dashDirection, dashDistance))
        {
            isDashing = true;
            shielding = false;
            punchedRight = false;
            returningRight = false;
            state = State.Dashing;
        }
    }
    protected void FixedHandleDash()
    {
        if (!recoveringFromDash)
        {
            Vector3 newVelocity = new Vector3(transform.right.normalized.x * 40, rb.velocity.y, transform.right.normalized.z * 30);
            rb.velocity = newVelocity;
        }
        else
        {
            Vector3 newVelocityy = new Vector3(0, rb.velocity.y, 0);
            rb.velocity = newVelocityy;
        }
    }

    IEnumerator DashRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        recoveringFromDash = true;

        SpawnHeavyParticleServerRpc(GrabPosition.position, transform.right);
        StartCoroutine(DashRecoveryRoutine(.525f));
    }
    IEnumerator DashRecoveryRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        recoveringFromDash = false;
        state = State.Normal;
    }
    protected override void HandleDash()
    {
        
    }

    private bool CanMove(Vector3 dir, float distance)
    {
        return Physics.Raycast(transform.position, dir, distance, wallForDash);
    }




    protected override void FaceLookDirection()
    {
        if (punchedLeft || punchedRight || returningLeft || rightHandTransform.localPosition.x > 2f && returningRight) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;
        if (state == State.Dashing) return;

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0)
        {
            lastLookedPosition = lookTowards;
        }

        Look();
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
        if (punchedRight || returningRight) return;
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;
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
        if (returningLeft || punchedLeft) return;
        if (returningRight || punchedRight) return;
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;
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

    [ServerRpc]
    private void SpawnLeftParticleServerRpc(Vector3 dir, Vector3 rot)
    {
        SpawnLeftParticleClientRpc(dir, rot);
    }
    [ClientRpc]
    private void SpawnLeftParticleClientRpc(Vector3 dir, Vector3 rot)
    {

        GameObject thrust = Instantiate(swordThrustParticle, dir, thrustPosition.rotation);
        thrust.transform.right = rot;
        HandleColliderNetwork handleCollider = thrust.GetComponent<HandleColliderNetwork>();
        handleCollider.SetPlayer(this, leftHandParent);
    }
    [ServerRpc]
    private void SpawnRightParticleServerRpc(Vector3 dir, Vector3 rot)
    {
        SpawnRightParticleClientRpc(dir, rot);
    }
    [ClientRpc]
    private void SpawnRightParticleClientRpc(Vector3 dir, Vector3 rot)
    {

        GameObject slash = Instantiate(swordSlash, dir, Quaternion.identity);
        slash.transform.right = rot;
        HandleColliderNetwork handleCollider = slash.GetComponent<HandleColliderNetwork>();
        handleCollider.SetPlayer(this, leftHandParent);
    }
    [ServerRpc]
    private void SpawnHeavyParticleServerRpc(Vector3 dir, Vector3 rot)
    {
        SpawnHeavyParticleClientRpc(dir, rot);
    }
    [ClientRpc]
    private void SpawnHeavyParticleClientRpc(Vector3 dir, Vector3 rot)
    {

        GameObject heavySlash = Instantiate(heavySlashPrefab, dir, Quaternion.identity);
        heavySlash.transform.right = rot;
        HandleColliderNetwork handleCollider = heavySlash.GetComponent<HandleColliderNetwork>();
        handleCollider.SetPlayer(this, leftHandParent);
    }
}
