using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlayer : PlayerController
{
    [SerializeField] GameObject fireballInHand;
    [SerializeField] GameObject fireballPrefab, explosionPrefab, bigFireball;
    GameObject fireballInstantiated, fireballInstantiatedLeft, bigInstantiated;
    float punchLeftAnimationTimer;
    [SerializeField] GameObject swingLeftParticle, swingRightParticle;
    bool spawnedLeft, spawnedRight, spawnedBigFireball, dashBallSpawned = false;
    GameObject swingLeftInstantiated, swingRightInstantiated;
    float fireballSpeed = 60f;
    float startDashTimer;

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
                FixedHandleMovement();
                break;
        }
    }
    protected override void HandleThrowingHands()
    {

        animatorUpdated.SetBool("punchingLeft", (punchedLeft));
        animatorUpdated.SetBool("punchingRight", (punchedRight));


        animatorUpdated.SetBool("Grabbing", false);
        if (releasedLeft && fireballInstantiatedLeft != null && spawnedLeft == false && fireballInstantiatedLeft.GetComponent<Rigidbody>().velocity != Vector3.zero)
        {
            spawnedLeft = true;
            GameObject newExplosion = Instantiate(explosionPrefab, fireballInstantiatedLeft.transform.position, Quaternion.identity);
            newExplosion.GetComponent<HandleCollider>().SetPlayer(this, leftHandTransform);
            ParticleSystem[] particles = fireballInstantiatedLeft.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                particle.Stop();
            }
            fireballInstantiatedLeft.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        if (releasedRight && fireballInstantiated != null && spawnedRight == false && fireballInstantiated.GetComponent<Rigidbody>().velocity != Vector3.zero)
        {
            spawnedRight = true;
            GameObject newExplosion = Instantiate(explosionPrefab, fireballInstantiated.transform.position, Quaternion.identity);
            newExplosion.GetComponent<HandleCollider>().SetPlayer(this, rightHandTransform);
            ParticleSystem[] particles = fireballInstantiated.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                particle.Stop();
            }
            fireballInstantiated.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        if (punchedLeft)
        {
            punchedLeftTimer = 0;
            spawnedLeft = false;

            punchLeftAnimationTimer += Time.deltaTime;
            if (punchLeftAnimationTimer > .025f && swingLeftInstantiated == null)
            {
                swingLeftInstantiated = Instantiate(swingLeftParticle, GrabPosition.position, transform.rotation);
                swingLeftInstantiated.GetComponent<HandleColliderShieldBreak>().SetPlayer(this, leftHandTransform);
            }
            if (punchLeftAnimationTimer > .5f && swingRightInstantiated == null)
            {
                swingRightInstantiated = Instantiate(swingRightParticle, GrabPosition.position, transform.rotation);

                swingRightInstantiated.GetComponent<HandleColliderShieldBreak>().SetPlayer(this, rightHandTransform);
            }
            if (punchLeftAnimationTimer > 1f && fireballInstantiatedLeft == null)
            {
                fireballInstantiatedLeft = Instantiate(fireballPrefab, rightHandParent.transform.position, transform.rotation);
                fireballInstantiatedLeft.GetComponent<Rigidbody>().AddForce((transform.right) * (fireballSpeed), ForceMode.Impulse);

                punchedLeft = false;
            }

            if (punchLeftAnimationTimer > 1.18f)
            {
            }
            
        }

        if (returningLeft)
        {
            punchedLeft = false;
            returningLeft = false;
        }

        if (punchedRight && returningRight == false)
        {
            spawnedRight = false;
            punchedRightTimer = 0;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                fireballInstantiated = Instantiate(fireballPrefab, rightHandParent.transform.position, transform.rotation);
                fireballInHand.SetActive(false);

                returningRight = true;
                fireballInstantiated.GetComponent<Rigidbody>().AddForce((transform.right) * (fireballSpeed), ForceMode.Impulse);
            }
        }
        if (returningRight)
        {
            punchedRight = false;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);


            if (rightHandTransform.localPosition.x <= 0f)
            {
                fireballInHand.SetActive(true);
                returningRight = false;
            }
        }

        if (state == State.Dashing)
        {
            returnSpeed = 10f;
            if (startDashTimer > 0f)
            {
                moveSpeed = 0f;
                return;
            }
            moveSpeed = moveSpeedSetter + 15f;
            if (punchedLeft || punchedRight || returningLeft || returningRight)
            {
                moveSpeed = moveSpeedSetter + 2f;
            }
            return;
        }
        if (punchedLeft || punchedRight || returningLeft || returningRight)
        {
            moveSpeed = 0;
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
            returnSpeed = 8f;
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
        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("Dashing", true);
        }
        canDash = false;
        StartCoroutine(StartDashCooldown(12f));
        isDashing = true;
        dashTimer = 3f;
        startDashTimer = .75f;
        dashBallSpawned = false;
        state = State.Dashing;
        if (bigInstantiated != null) Destroy(bigInstantiated);
    }
    IEnumerator StartDashCooldown(float dashcd)
    {
        canDash = false;
        yield return new WaitForSecondsRealtime(.1f);
        canDash = true;
    }
    protected override void HandleDash()
    {

        startDashTimer -= Time.deltaTime;

        if (startDashTimer <= 0f)
        {
            animatorUpdated.SetBool("Dashing", false);
            dashTimer -= Time.deltaTime;
            moveSpeed = moveSpeedSetter + 10f;
            if (dashTimer <= 0)
            {
                fireballInHand.SetActive(true);

                animatorUpdated.SetBool("Dashing", false);
                isDashing = false;
                bigInstantiated.GetComponentInChildren<Collider>().enabled = false;
                ParticleSystem[] particles = bigInstantiated.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particle in particles)
                {
                    particle.Stop();
                }
                state = State.Normal;
            }
        }
        if (startDashTimer < .5f && !dashBallSpawned)
        {

            fireballInHand.SetActive(false);
            bigInstantiated = Instantiate(bigFireball, new Vector3(this.transform.position.x, this.transform.position.y + 3.5f, this.transform.position.z), Quaternion.identity);
            bigInstantiated.GetComponent<Rigidbody>().velocity = Vector3.zero;
            bigInstantiated.GetComponent<HandleColliderShieldBreak>().SetPlayer(this, rightHandTransform);
            bigInstantiated.GetComponentInChildren<Collider>().enabled = true;
            dashBallSpawned = true;
        }

        if (bigInstantiated != null)
        {
            bigInstantiated.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z);
            bigInstantiated.transform.right = this.transform.right;
            if (!bigInstantiated.GetComponentInChildren<ParticleSystem>().isPlaying)
            {
                fireballInHand.SetActive(true);
                animatorUpdated.SetBool("Dashing", false);
                isDashing = false;
                bigInstantiated.GetComponentInChildren<Collider>().enabled = false;
                ParticleSystem[] particles = bigInstantiated.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particle in particles)
                {
                    particle.Stop();
                }
                state = State.Normal;
            }
        }

        if (startDashTimer > 0f)
        {
            moveSpeed = moveSpeedSetter + 10f;
        }

    }
    public override void Knockback(float damage, Vector3 direction, PlayerController playerSent)
    {
        if (bigInstantiated != null)
        {
            bigInstantiated.GetComponentInChildren<Collider>().enabled = false;
            ParticleSystem[] particles = bigInstantiated.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                particle.Stop();
            }
            Destroy(bigInstantiated);
        }
        if (grabbing)
        {
            EndGrab();
        }
        hasLanded = false;
        EndPunchRight();
        EndPunchLeft();
        //if (state == State.WaveDahsing && rb.velocity.magnitude > 20f) return;
        if (animatorUpdated != null)
        {
            SetAnimatorToKnockback();
        }

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
        float knockbackValue = (20 * ((currentPercentage + damage) * (damage / 2)) / 150) + 25; //knockback that scales
        rb.velocity = new Vector3(direction.x * knockbackValue, 0, direction.z * knockbackValue);
        if (GameConfigurationManager.Instance != null)
        {
            GameConfigurationManager.Instance.DisplayDamageText((int)damage, this.transform, playerSent);
        }
        HitImpact(direction);
        state = State.Knockback;
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
        if (!canDash) return;
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

        if (returningLeft || punchedLeft) return;
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;

        if (state == State.Knockback) return;
        if (state == State.Stunned) return;
        if (grabbing) return;
        if (state == State.Grabbed) return;
        if (state == State.Dashing) return;

        if (punchedLeftTimer > 0)
        {
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }

            if (swingLeftInstantiated != null) Destroy(swingLeftInstantiated);
            if (swingRightInstantiated != null) Destroy(swingRightInstantiated);
            if (fireballInstantiatedLeft != null) Destroy(fireballInstantiatedLeft);
            if (shielding) shielding = false;
            punchedLeft = true;
            punchLeftAnimationTimer = 0f;
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

        if (returningRight || punchedRight) return;
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;

        if (state == State.Knockback) return;
        if (state == State.Stunned) return;
        if (grabbing) return;
        if (state == State.Grabbed) return;
        if (state == State.Dashing) return;

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

    protected override void FaceLookDirection()
    {
        if (punchedLeft || punchedRight || leftHandTransform.localPosition.x > 1f && returningLeft || rightHandTransform.localPosition.x > 1f && returningRight) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;
        if (grabbing) return;
        if (startDashTimer > 0f && state == State.Dashing) return;

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0f)
        {
            lastLookedPosition = lookTowards;
        }

        Look();
    }
}
