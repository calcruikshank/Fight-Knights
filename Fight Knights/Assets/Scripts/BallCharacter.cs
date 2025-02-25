using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class BallCharacter : PlayerController
{
    float topSpeed = 30f;
    float topSpeedSetter = 35;
    [SerializeField] Transform playerRing;
    [SerializeField] public Collider bodyCollider;
    [SerializeField] GameObject ballDashParticle;
    
    bool canWaveDash;
    public override void Awake()
    {
        if (!IsOffline())
        {
            var netTransform = gameObject.AddComponent<NetworkTransform>();
            var netRigidbody = gameObject.AddComponent<NetworkRigidbody>();
        }
        Application.targetFrameRate = 600;
        rb = GetComponentInChildren<Rigidbody>();
        state = State.Normal;
        cameraShake = FindObjectOfType<CameraShake>();
        leftHandParent = leftHandTransform.parent.transform;
        rightHandParent = rightHandTransform.parent.transform;
        canDash = true;
        grabbing = false;
        releasedLeft = true;
        releasedRight = true;
        if (animatorUpdated != null)
        {
            SetAnimatorToIdle();
        }
        bodyCollider.enabled = false;
        canWaveDash = true;
        topSpeed = topSpeedSetter;
    }

    protected override void Look()
    {

        playerRing.right = lastLookedPosition;
    }
    protected override void FixedHandleMovement()
    {
        Vector3 newVelocity = new Vector3(movement.x * moveSpeed, rb.linearVelocity.y, movement.z * moveSpeed);

        rb.AddForce(movement.normalized * moveSpeed * 15);
        if (rb.linearVelocity.magnitude > topSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * topSpeed;
        }
        

    }

    protected override void HandleThrowingHands()
    {
        if (state == State.Dashing)
        {
            returnSpeed = 10f;
            moveSpeed = moveSpeedSetter + 15f;
            if (punchedLeft || punchedRight || returningLeft || returningRight)
            {
                moveSpeed = moveSpeedSetter + 2f;
            }
            return;
        }
        if (punchedLeft || punchedRight || returningLeft || returningRight)
        {
            moveSpeed = moveSpeedSetter - 8f;
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
            returnSpeed = 10f;
        }
        if (returningLeft && returningRight)
        {

            returnSpeed = 4f;
        }
    }
    protected override void HandleKnockback()
    {

        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        bodyCollider.enabled = false;
        if (rb.linearVelocity.magnitude < 20f && !hasChangedFromKnockbackToFallingAnimation)
        {
            if (animatorUpdated != null)
            {
                animatorUpdated.SetBool("Landing", true);

                hasChangedFromKnockbackToFallingAnimation = true;
            }
            landingTime = 0f;
            hasLanded = false;
        }

        if (rb.linearVelocity.magnitude < 20f)
        {
            landingTime += Time.deltaTime;
            if (landingTime > .4f)
            {
                hasLanded = true;
            }
        }
        if (hasLanded)
        {
            if (animatorUpdated != null)
            {
                SetAnimatorToKnockback();
                SetAnimatorToIdle();
                animatorUpdated.SetBool("Knockback", false);
                animatorUpdated.SetBool("Landing", false);
            }
            rb.linearVelocity = new Vector2(0, 0);
            rb.linearDamping = 0;

            this.transform.GetComponentInChildren<SphereCollider>().material.dynamicFriction = .6f;

            this.transform.GetComponentInChildren<SphereCollider>().material.frictionCombine = PhysicsMaterialCombine.Maximum;
            state = State.Normal;
        }

        if (rb.linearVelocity.magnitude > 0)
        {
            this.transform.GetComponentInChildren<SphereCollider>().material.dynamicFriction = 0f;
            this.transform.GetComponentInChildren<SphereCollider>().material.frictionCombine = PhysicsMaterialCombine.Minimum;
            
            oppositeForce = -rb.linearVelocity;
            //brakeSpeed = brakeSpeed + (100f * Time.deltaTime);
            //rb.AddForce(oppositeForce * Time.deltaTime * brakeSpeed);
            //rb.AddForce(movement * .05f); //DI*/
            rb.linearDamping = 1.3f;
        }

        Vector3 knockbackLook = new Vector3(oppositeForce.x, 0, oppositeForce.z);
        //transform.right = knockbackLook;
    }

    public override void Knockback(float damage, Vector3 direction, PlayerController playerSent)
    {

        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }

        this.transform.GetComponentInChildren<SphereCollider>().material.frictionCombine = PhysicsMaterialCombine.Minimum;

        this.transform.GetComponentInChildren<SphereCollider>().material.dynamicFriction = 0f;
        bodyCollider.enabled = false;
        if (grabbing)
        {
            EndGrab();
        }
        EndPunchRight();
        EndPunchLeft();
        //if (state == State.WaveDahsing && rb.velocity.magnitude > 20f) return;
        if (animatorUpdated != null)
        {
            SetAnimatorToKnockback();
        }
        hasLanded = false;

        //this if is for if the opponent is grabbed
        if (opponent != null)
        {
            if (state == State.Grabbing)
            {
                opponent.Throw(transform.right);
            }
        }
        canAirDodgeTimer = 0f;

        currentPercentage += damage;
        brakeSpeed = 0f;
        // Debug.Log(damage + " damage");
        //Vector2 direction = new Vector2(rb.position.x - handLocation.x, rb.position.y - handLocation.y); //distance between explosion position and rigidbody(bluePlayer)
        //direction = direction.normalized;
        float knockbackValue = (20 * ((currentPercentage + damage) * (damage / 2)) / 150) + 14; //knockback that scales
        rb.linearVelocity = new Vector3(direction.x * knockbackValue, 0, direction.z * knockbackValue);
        if (GameConfigurationManager.Instance != null)
        {
            GameConfigurationManager.Instance.DisplayDamageText((int)damage, this.transform, playerSent);
        }
        HitImpact(direction);
        state = State.Knockback;
    }

    public Vector3 GetMovement()
    {
        return movement;
    }

    protected override void HandleAButton()
    {
        if (waveDashBool)
        {
            WaveDash(lastMoveDir, 80f);
            if (animatorUpdated != null)
            {
                animatorUpdated.SetBool("Rolling", true);
            }
            waveDashBool = false;
            Instantiate(ballDashParticle, transform.position, transform.rotation);
            canWaveDash = false;
            StartCoroutine(waveDashRoutine(.75f));
        }
    }

    protected override void Dash(Vector3 dashDirection)
    {

        rb.AddForce(movement.normalized * moveSpeed * 5);
    }

    
    private IEnumerator waveDashRoutine(float grabtime)
    {
        yield return new WaitForSecondsRealtime(grabtime);
        canWaveDash = true;
    }
    protected override void HandleWaveDashing()
    {
        Look();
        bodyCollider.enabled = true;
        transform.right = lastMoveDir;
        Time.timeScale = 1;
        float powerDashSpeedMulti = 4f;
        powerDashSpeed -= powerDashSpeed * powerDashSpeedMulti * Time.deltaTime;

        float powerDashMinSpeed = 10f;
        if (powerDashSpeed < powerDashMinSpeed)
        {
            if (animatorUpdated != null)
            {
                animatorUpdated.SetBool("Rolling", false);
            }
            bodyCollider.enabled = false;
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
        

        //when dash button is released subtract time from dashbuffer so it only goes down when youre not pressing dash
        if (releasedDash)
        {

            topSpeed = topSpeedSetter;
            dashBuffer -= Time.deltaTime;
        }

        //return area
        if (state != State.Normal) return;
        if (state == State.Dashing) return;
        if (state == State.Stunned) return;
        if (!canDash) return;
        if (grabbing) return;
        if (state == State.Grabbed) return;
        if (pressedDash)
        {

            topSpeed = topSpeedSetter;
            Dash(movement.normalized);
        }
        //check if hasdashedtimer is good to go if not return

        //then if dash buffer is greater than 0 dash
        
    }
    protected override void FaceLookDirection()
    {

        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0f)
        {
            lastLookedPosition = lookTowards;
        }

        Look();
    }
    protected override void CheckForWaveDash()
    {
        if (releasedWaveDash)
        {
            waveDashTimer -= Time.deltaTime;
        }
        if (pressedWaveDash)
        {
            waveDashTimer = inputBuffer;
            pressedWaveDash = false;
        }
        if (!canWaveDash) return;
        if (lastMoveDir.magnitude == 0f) return;
        if (state == State.WaveDahsing) return;
        if (state == State.Stunned) return;
        if (state == State.Dashing) return;
        if (state == State.Grabbed) return;
        if (state == State.Knockback) return;
        if (grabbing) return;
        if (waveDashTimer > 0)
        {
            
            waveDashBool = true;
            waveDashTimer = 0f;
        }
    }
}
