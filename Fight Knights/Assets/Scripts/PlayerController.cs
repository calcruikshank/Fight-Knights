﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Multiplayer;
using static UnityEngine.Rendering.DebugUI;
using Unity.Netcode.Components;
using System;

public class PlayerController : NetworkBehaviour
{
   


    protected Rigidbody rb;
    protected Vector3 inputMovement, movement, lastMoveDir, lastLookedPosition, lookDirection, oppositeForce, powerDashTowards;
    protected bool pressedRight, pressedLeft, pressedShield, releasedShield, pressedDash, releasedDash, airDodged, releasedAirDodged, pressedWaveDash, releasedWaveDash, waveDashBool, canDash, grabbing, hasLanded = false;
    public bool punchedRight, punchedLeft, shielding, isParrying, returningLeft, returningRight, releasedLeft, releasedRight, isDashing, hasChangedFromKnockbackToFallingAnimation = false;
    float parryTimerThreshold = .25f;
    [SerializeField] protected float moveSpeed, moveSpeedSetter = 18f;
    protected float punchedLeftTimer, punchedRightTimer, brakeSpeed, canShieldAgainTimer, parryTimer, parryStunnedTimer, isParryingTimer, powerDashSpeed, dashBuffer, canAirDodgeTimer, airShieldTimer, waveDashTimer, stunTimerThreshold, stunTimer, dashTimer, landingTime;
    public float currentPercentage;
    protected float inputBuffer = .15f;
    [SerializeField] protected Transform leftHandTransform, rightHandTransform, GrabPosition, grabbedPositionTransform;
    [SerializeField] GameObject splatterPrefab, fistIndicator, parryIndicator, initialStunParticle, continuedStunParticle, continuedStunSpawned, leftJabParticle, grabSwirl, swirlInstantiated, dashStartParticle, respawnParticlePrefab;
    [SerializeField] ParticleSystem knockbackSmoke;
    protected float punchRange = 3f;
    protected float punchRangeRight = 4f;
    [SerializeField]protected float punchSpeed = 40f;
    protected float returnSpeed = 15f;

    [SerializeField] private LayerMask layerMask;
    public int stocks = 4;
    protected PlayerController opponent;

    protected CameraShake cameraShake;
    protected Transform rightHandParent, leftHandParent;
    [SerializeField] protected Animator animatorUpdated;
    [SerializeField] Transform shield;
    [SerializeField] GameObject arrow, deathEffect;
    public bool lostStock = false;
    string currentControlScheme;

    private float lastNetUpdateTime = 0f;
    // Position, Velocity, and Rotation
    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Owner,
        readPerm: NetworkVariableReadPermission.Everyone
    );
    private NetworkVariable<Vector3> netVelocity = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Owner,
        readPerm: NetworkVariableReadPermission.Everyone
    );
    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Owner,
        readPerm: NetworkVariableReadPermission.Everyone
    );



    public State state;
    public enum State
    {
        Normal,
        Knockback,
        Diving,
        Grabbed,
        Grabbing,
        Stunned,
        Dashing,
        ShockGrabbed,
        FireGrabbed,
        UltimateState,
        TakingUltimate,
        ParryState,
        PowerDashing,
        WaveDahsing,
        ParryStunned,
        AirDodging
    }

    
    public virtual void Awake()
    {
        Application.targetFrameRate = 600;
        rb = GetComponent<Rigidbody>();
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

    }
    // Start is called before the first frame update
    void Start()
    {
        currentControlScheme = this.gameObject.GetComponent<PlayerInput>().currentControlScheme;
    }
    private PlayerInput _playerInput;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            // For non-owners, subscribe to position changes (or velocity) to record the time
            netPosition.OnValueChanged += (oldPos, newPos) =>
            {
                lastNetUpdateTime = Time.time;
            };
            // If you want, also attach to netVelocity.OnValueChanged
            // netVelocity.OnValueChanged += (oldVel, newVel) => { lastNetUpdateTime = Time.time; };

            // Initialize
            lastNetUpdateTime = Time.time;
        }
        else
        {
            // The owner sets position/velocity in Update() or FixedUpdate().
        }
        // Only apply the local device/scheme if we own this object
        if (!IsOwner) return;
        _playerInput = GetComponent<PlayerInput>();
        // Retrieve the local player's config from PlayerConfigurationManager

        var configs = PlayerConfigurationManager.Instance.GetPlayerConfigs();
        if (configs.Count == 0)
        {
            Debug.LogWarning("No local config found. Did you set one offline?");
            return;
        }

        // If there's only 1 local player, just grab index [0]
        var localConfig = configs[0];

        // Switch the local PlayerInput to the correct scheme and device
        // so that it picks up exactly what the user was using offline
        if (_playerInput != null)
        {
            var controlScheme = localConfig.ControlScheme; // e.g. “Gamepad” or “Keyboard & Mouse”

            // Build up the list of InputDevices we want to pass
            var deviceList = new List<InputDevice>();

            // If your localConfig.CurrentDevice is the only device you stored, that might be just “Keyboard”
            // but for keyboard + mouse, you need both:
            if (controlScheme == "Keyboard and Mouse")
            {
                if (Keyboard.current != null)
                    deviceList.Add(Keyboard.current);
                if (Mouse.current != null)
                    deviceList.Add(Mouse.current);
            }
            else if (controlScheme == "Gamepad")
            {
                // Example if you only want to set Gamepad
                if (Gamepad.current != null)
                    deviceList.Add(Gamepad.current);
            }

            // Finally, switch to that scheme with all relevant devices
            _playerInput.SwitchCurrentControlScheme(
                controlScheme,
                devices: deviceList.ToArray()
            );

            _playerInput.ActivateInput();
        }
    }
    private Vector3 lastNetPos;                 // last position we received from netPosition
    private Vector3 lastNetPosBeforeUpdate;     // previous position from the last update
    private float lastNetPosUpdateTime;         // local time when we last received an update
    private Vector3 remoteVelocity;
    private void OnNetPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        // 1) How long since our last update?
        float dt = Time.time - lastNetPosUpdateTime;
        lastNetPosUpdateTime = Time.time;

        // 2) Compute velocity = (newPos - oldPos) / deltaTime
        if (dt > 0f)
        {
            remoteVelocity = (newValue - oldValue) / dt;
        }

        // 3) Update the local references
        lastNetPosBeforeUpdate = oldValue;
        lastNetPos = newValue;
    }
    private void ExtrapolateRemoteMovement()
    {
        // 1) How long since the last update arrived?
        float timeSinceUpdate = Time.time - lastNetUpdateTime;

        // 2) Predict the new position:
        //    last known position plus velocity * time
        Vector3 predictedPos = netPosition.Value + new Vector3( netVelocity.Value.x, netPosition.Value.y, netVelocity.Value.z ) * timeSinceUpdate;

        // 3) Smoothly move (LERP) toward that predicted position to avoid snapping
        float lerpSpeed = 15f; // tweak to taste
        transform.position = Vector3.Lerp(transform.position, new Vector3(predictedPos.x, transform.position.y, predictedPos.z) , Time.deltaTime * lerpSpeed);

        // 4) For rotation, we can just snap or slerp, depending on your preference:
        transform.rotation = netRotation.Value;
    }

    protected virtual void Update()
    {
        if (IsOwner)
        {
            netPosition.Value = transform.position;
            netVelocity.Value = rb.linearVelocity; // or however you track velocity
            netRotation.Value = transform.rotation;
        }
        else
        {
            // Non-owner logic: we do extrapolation
            ExtrapolateRemoteMovement();
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
                HandleThrowingHands();
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

    protected virtual void FixedUpdate()
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


    protected virtual void HandleMovement()
    {
        movement.x = inputMovement.x;
        movement.z = inputMovement.y;
        if (movement.x != 0 || movement.z != 0)
        {
            lastMoveDir = movement;
        }
        if (animatorUpdated != null)
        {
            if (!shielding && state == State.Normal || !shielding && state == State.Dashing)
            {
                
               animatorUpdated.SetFloat("MoveSpeed", (movement.magnitude));

                
            }
            else
            {
                animatorUpdated.SetFloat("MoveSpeed", (0));
            }
        }
    }
    protected virtual void FixedHandleMovement()
    {
        float yVelo;
        if (rb.linearVelocity.y < 0)
        {
            yVelo = rb.linearVelocity.y;
        }
        else
        {
            yVelo = 0f;
        }
        Vector3 newVelocity = new Vector3(movement.x * moveSpeed, yVelo, movement.z * moveSpeed);
        rb.linearVelocity = newVelocity;

    }


    protected virtual void HandleThrowingHands()
    {
        if (!grabbing && swirlInstantiated != null)
        {
            Destroy(swirlInstantiated);
        }
        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }

        #region GrabRegion
        if (punchedLeft && !releasedRight && grabbing == false || punchedRight && !releasedLeft && grabbing == false)
        {
            animatorUpdated.SetBool("Grabbing", true);
            StartCoroutine(GrabTimer(.5f));
            grabbing = true;
            swirlInstantiated = Instantiate(grabSwirl, GrabPosition.position, Quaternion.identity);
            swirlInstantiated.GetComponent<GrabSwirl>().SetPlayer(this);
            swirlInstantiated.GetComponent<Collider>().enabled = false;
        }
        
        if (grabbing)
        {
            swirlInstantiated.transform.position = GrabPosition.position;
            rb.linearVelocity = Vector3.zero;
            moveSpeed = 0f;
        }
        if (grabbing && !returningRight)
        {
            rb.linearVelocity = Vector3.zero;
            moveSpeed = 0f;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * Time.deltaTime);
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                swirlInstantiated.GetComponent<Collider>().enabled = true;
                returningLeft = true;
                returningRight = true;
                punchedRight = false;
                punchedLeft = false;
                returnSpeed = 10f;
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
                }
            }
            return;
        }
        #endregion

        if (punchedLeft && returningLeft == false)
        {
            animatorUpdated.SetBool("Rolling", false);
            punchedLeftTimer = 0;
            //leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * 2 * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange / 1.5f)
            {
                if (splatterPrefab != null)
                {
                    GameObject leftJab = Instantiate(leftJabParticle, leftHandParent.position, Quaternion.identity);
                    leftJab.transform.right = transform.right;
                    HandleCollider handleCollider = leftJab.GetComponent<HandleCollider>();
                    handleCollider.SetPlayer(this, leftHandParent);
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

                animatorUpdated.SetBool("Grabbing", false);
            }
        }



        if (punchedRight && returningRight == false)
        {
            

            animatorUpdated.SetBool("Rolling", false);
            punchedRightTimer = 0;
            //rightHandCollider.enabled = true;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, .4f), punchSpeed * 1.5f * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                if (splatterPrefab != null)
                {
                    GameObject splatter = Instantiate(splatterPrefab, rightHandParent.position, Quaternion.identity);
                    splatter.transform.right = transform.right;
                    HandleCollider handleCollider = splatter.GetComponent<HandleCollider>();
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

                animatorUpdated.SetBool("Grabbing", false);
            }
        }
        if (grabbing) return;
        if (state == State.Dashing)
        {
            returnSpeed = 8f;
            moveSpeed = moveSpeedSetter + 15f;
            if (punchedLeft || punchedRight || returningLeft || returningRight)
            {
                moveSpeed = moveSpeedSetter + 2f;
            }
            return;
        }
        if (punchedLeft || punchedRight)
        {
            moveSpeed = 0f;
            return;
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
        
    }

    private IEnumerator GrabTimer(float grabtime)
    {
        yield return new WaitForSecondsRealtime(grabtime);
        grabbing = false;
        returningLeft = true;
        returningRight = true;
        punchedRight = false;
        punchedLeft = false;
        if (opponent.state == State.Grabbed && opponent.transform.position == GrabPosition.position)
        {
            opponent.Throw(GrabPosition.right.normalized);
        }
        if (swirlInstantiated != null)
        {
            Destroy(swirlInstantiated);
        }
        animatorUpdated.SetBool("Grabbing", false);
    }



    protected virtual void HandleAButton()
    {
        if (waveDashBool)
        {
            WaveDash(lastMoveDir, 70f);
            if (animatorUpdated != null)
            {
                animatorUpdated.SetBool("Rolling", true);
            }
            waveDashBool = false;
            
        }
    }

    public virtual void Knockback(float damage, Vector3 direction, PlayerController playerSent)
    {
        HitImpact(direction);

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
        float knockbackValue = (20 * ((currentPercentage + damage) * (damage / 2)) / 150) + 25; //knockback that scales
        rb.linearVelocity = new Vector3(direction.x * knockbackValue, 0, direction.z * knockbackValue);
        if (GameConfigurationManager.Instance != null)
        {
            GameConfigurationManager.Instance.DisplayDamageText((int)damage, this.transform, playerSent);
        }
        state = State.Knockback;
    }
    protected virtual void HandleKnockback()
    {

        if (knockbackSmoke != null)
        {
            knockbackSmoke.gameObject.SetActive(true);
            
        }
        if (rb.linearVelocity.magnitude < 25f && !hasChangedFromKnockbackToFallingAnimation)
        {
            if (animatorUpdated != null)
            {
                animatorUpdated.SetBool("Landing", true);

                hasChangedFromKnockbackToFallingAnimation = true;
            }
            landingTime = 0f;
            hasLanded = false;
        }

        if (rb.linearVelocity.magnitude < 25f)
        {
            landingTime += Time.deltaTime;
            if (landingTime > .4f)
            {
                hasLanded = true;
            }
        }
        if (hasLanded)
        {
            if (knockbackSmoke != null) knockbackSmoke.Stop();
            if (animatorUpdated != null)
            {
                SetAnimatorToKnockback();
                SetAnimatorToIdle();
                animatorUpdated.SetBool("Knockback", false);
                animatorUpdated.SetBool("Landing", false);
            }
            rb.linearVelocity = new Vector2(0, 0);
            rb.linearDamping = 0;
            state = State.Normal;
        }

        if (rb.linearVelocity.magnitude > 0)
        {
            oppositeForce = -rb.linearVelocity;
            //brakeSpeed = brakeSpeed + (100f * Time.deltaTime);
            //rb.AddForce(oppositeForce * Time.deltaTime * brakeSpeed);
            //rb.AddForce(movement * .05f); //DI*/
            rb.linearDamping = .8f;
        }
        float yVelo;
        if (rb.linearVelocity.y < 0)
        {
            yVelo = rb.linearVelocity.y;
        }
        else
        {
            yVelo = 0f;
        }
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVelo, rb.linearVelocity.z);
        Vector3 knockbackLook = new Vector3(oppositeForce.x, 0, oppositeForce.z);
        transform.right = knockbackLook;
    }
    

    public void EndGrab()
    {
        if (opponent != null)
        {
            if (opponent.state == State.Grabbed)
            {
                Debug.Log("normal state");
                opponent.state = State.Normal;
            }
        }
        
        
        grabbing = false;
        returningLeft = true;
        returningRight = true;
        punchedRight = false;
        punchedLeft = false;
        
        if (swirlInstantiated != null)
        {
            Destroy(swirlInstantiated);
        }
        animatorUpdated.SetBool("Grabbing", false);
    }
    public virtual void EndPunchRight()
    {
        
        punchedRight = false;
        returningRight = true;
        rightHandTransform.gameObject.GetComponent<Collider>().enabled = false;
    }
    protected virtual void EndPunchLeft()
    {
        punchedLeft = false;
        returningLeft = true;
        leftHandTransform.gameObject.GetComponent<Collider>().enabled = false;
    }

    protected void HandleShielding()
    {
        if (shielding)
        {
            parryTimer += Time.deltaTime;
            if (parryTimer <= parryTimerThreshold)
            {
                isParrying = true;
            }
            if (parryTimer > parryTimerThreshold)
            {
                isParrying = false;
            }

            shield.gameObject.SetActive(true);

        }
        if (!shielding && state != State.ParryState)
        {
            isParrying = false;
            shield.gameObject.SetActive(false);
        }
    }
   

    public void Parry()
    {
        rb.linearDamping = 0f;
        punchedRight = false;
        punchedLeft = false;
        returningRight = false;
        returningLeft = false;
        leftHandTransform.localPosition = Vector3.zero;
        rightHandTransform.localPosition = Vector3.zero;
        GameObject parryParticle = Instantiate(parryIndicator, transform.position, Quaternion.identity);
        if (state == State.ParryState) return;
        rb.linearVelocity = Vector3.zero;
        isParryingTimer = 0;
        if (animatorUpdated != null)
        {
            SetAnimatorToIdle();
        }
        state = State.ParryState;
        
    }
    protected void HandleParry()
    {
        shield.gameObject.SetActive(true);
        Time.timeScale = .2f;
        isParryingTimer += Time.deltaTime;

        if (inputMovement.magnitude > .8f)
        {
            arrow.SetActive(true);
        }
        if (inputMovement.magnitude <= .8f)
        {
            arrow.SetActive(false);
        }
        if (isParryingTimer > .2f && inputMovement.magnitude > .8f)
        {
            shielding = false;
            Time.timeScale = 1;

            arrow.SetActive(false);
            PowerDash(inputMovement, 80f);
            return;
        }
        if (isParryingTimer > .2f && inputMovement.magnitude <= .8f)
        {
            Time.timeScale = 1;

            arrow.SetActive(false);
            state = State.Normal;
        }
        
        if (punchedLeft || punchedRight && inputMovement.magnitude > .8f)
        {
            Time.timeScale = 1;
            shielding = false;
            arrow.SetActive(false);
            PowerDash(inputMovement, 80f);
            return;
        }
        if (punchedLeft || punchedRight)
        {

            Time.timeScale = 1;
            arrow.SetActive(false);
            state = State.Normal;
            return;
        }
        if (inputMovement.magnitude > .8f && releasedShield)
        {

            Time.timeScale = 1;
            PowerDash(inputMovement, 80f);

            arrow.SetActive(false);
            return;
        }
        if (releasedShield)
        {

            Time.timeScale = 1;
            arrow.SetActive(false);
            state = State.Normal;
        }

        
    }

    protected void PowerDash(Vector3 powerDashDirection, float sentSpeed)
    {
        shielding = false;
        canShieldAgainTimer = 0f;
        powerDashSpeed = sentSpeed;
        powerDashTowards = new Vector3(powerDashDirection.normalized.x, rb.linearVelocity.y, powerDashDirection.normalized.y);
        state = State.PowerDashing;
        if (animatorUpdated != null)
        {
            SetAnimatorToIdle();
        }
    }

    protected void HandlePowerDashing()
    {
        Time.timeScale = 1;
        float powerDashSpeedMulti = 6f;
        powerDashSpeed -= powerDashSpeed * powerDashSpeedMulti * Time.deltaTime;

        float powerDashMinSpeed = 10f;
        if (powerDashSpeed < powerDashMinSpeed)
        {
            state = State.Normal;
        }
    }
    protected void FixedHandlePowerDashing()
    {
        rb.linearVelocity = new Vector3(powerDashTowards.x * powerDashSpeed, 0f, powerDashTowards.z * powerDashSpeed);
    }

    public void Stunned(float stunTime, float damage)
    {
        Debug.Log("Initial parry stun");
        if(animatorUpdated != null)
        {
            SetAnimatorToStunned();
        }
        stunTimerThreshold = stunTime;
        stunTimer = 0f;
        Debug.Log("Stunned");
        rb.linearVelocity = Vector3.zero;
        GameObject initialStun = Instantiate(initialStunParticle, transform.position, Quaternion.identity);
        continuedStunSpawned = Instantiate(continuedStunParticle, transform.position, Quaternion.identity);
        EndPunchLeft();
        EndPunchRight();
        shielding = false;
        HitImpact(Vector3.zero);
        state = State.Stunned;
    }


    protected void HandleStunned()
    {
        rb.linearVelocity = Vector3.zero;
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunTimerThreshold)
        {
            if (animatorUpdated != null)
            {
                SetAnimatorToIdle();
            }
            state = State.Normal;
            if (continuedStunSpawned != null) Destroy(continuedStunSpawned);
        }
    }

    protected virtual void WaveDash(Vector3 powerDashDirection, float sentSpeed)
    {
        EndPunchLeft();
        EndPunchRight();
        animatorUpdated.SetBool("Rolling", true);
        shielding = false;
        transform.right = lastMoveDir;
        shielding = false;
        canShieldAgainTimer = 0f;
        powerDashSpeed = sentSpeed;
        powerDashTowards = new Vector3(powerDashDirection.normalized.x, rb.linearVelocity.y, powerDashDirection.normalized.z);
        state = State.WaveDahsing;
    }
    protected virtual void HandleWaveDashing()
    {
        transform.right = lastMoveDir;
        Time.timeScale = 1;

        float powerDashSpeedMulti = 3f;
        powerDashSpeed -= powerDashSpeed * powerDashSpeedMulti * Time.deltaTime;

        float powerDashMinSpeed = 20f;
        if (powerDashSpeed < powerDashMinSpeed)
        {
            if (animatorUpdated != null)
            {
                animatorUpdated.SetBool("Rolling", false);
            }
            state = State.Normal;
        }
    }
    protected virtual void FixedHandleWaveDashing()
    {
        float yVelo;
        if (rb.linearVelocity.y < 0)
        {
            yVelo = rb.linearVelocity.y;   
        }
        else
        {
            yVelo = 0f;
        }
        rb.linearVelocity = new Vector3(powerDashTowards.x * powerDashSpeed, yVelo, powerDashTowards.z * powerDashSpeed);
    }

    public void ParryStun()
    {
        parryStunnedTimer = 0f;
        state = State.ParryStunned;
    }

    protected void HandleParryStunned()
    {
        rb.linearVelocity = Vector3.zero;
        if (punchedRight)
        {
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, 0, .4f), punchSpeed * Time.deltaTime);
        }
        if (punchedLeft)
        {
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, 0, -.4f), punchSpeed * Time.deltaTime);
        }
        parryStunnedTimer += Time.deltaTime / Time.timeScale;
        if (parryStunnedTimer >= .5f)
        {
            returningLeft = true;
            returningRight = true;
            punchedRight = false;
            punchedLeft = false;
            state = State.Normal;
        }
    }

    public bool CheckIfColliderIsShield(Collider collider)
    {
        return collider == shield.GetComponent<Collider>();
    }

    public void LoseStock()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        Respawn();
        stocks--;
        if (stocks >= 1)
        {
        }
        else
        {
            if (GameConfigurationManager.Instance != null)
            {
                if (GameConfigurationManager.Instance.gameMode == 0 || GameConfigurationManager.Instance.gameMode == 2)
                {
                    GameConfigurationManager.Instance.RemovePlayerFromTeamArray(this.GetComponent<TeamID>().team);
                    GameConfigurationManager.Instance.CheckIfWon();
                    Destroy(this.gameObject);
                }
            }
        }
    }
    protected virtual void Respawn()
    {
        SetAnimatorToKnockback();
        SetAnimatorToIdle();
        if (animatorUpdated != null)
        {
            animatorUpdated.Play("Idle");
        }
        state = State.Normal;
        
        transform.position = Vector3.zero;
        if (stocks > 1)
        {
            if (respawnParticlePrefab != null)
            {
                Instantiate(respawnParticlePrefab, this.transform.position, transform.rotation);
            }
        }
       
        currentPercentage = 0f;

        lostStock = false;
    }
    public void HitImpact(Vector3 impactDirection)
    {
        StartCoroutine(cameraShake.Shake(.1f, .3f));
        StartCoroutine(FreezeFrames(.05f));
    }
    private IEnumerator FreezeFrames(float freezeTime)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeTime);
        Time.timeScale = 1f;
    }
    protected virtual void Dash(Vector3 dashDirection)
    {
        Instantiate(dashStartParticle, this.transform.position, this.transform.rotation);
        canDash = false;
        StartCoroutine(StartDashCooldown(12f));
        isDashing = true;
        dashTimer = 3f;
        state = State.Dashing;
        
    }

    IEnumerator StartDashCooldown(float dashcd)
    {
        canDash = false;
        yield return new WaitForSecondsRealtime(dashcd);
        canDash = true;
    }
    protected virtual void HandleDash()
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        dashTimer -= Time.deltaTime;
        moveSpeed = moveSpeedSetter + 10f;
        if (dashTimer <= 0)
        {
           
            isDashing = false;
            state = State.Normal;
        }
    }

    public void Grab(PlayerController opponent, Transform grabbedTransform)
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        if (state == State.Grabbed) return;
        opponent.Grabbed(this, grabbedTransform);
        this.opponent = opponent;
        
    }

    public void Grabbed(PlayerController playerGrabbing, Transform grabbedTransform)
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        shielding = false;
        EndPunchLeft();
        EndPunchRight();
        grabbedPositionTransform = grabbedTransform;
        this.transform.position = grabbedPositionTransform.position;
        this.state = State.Grabbed;
    }
    protected void HandleGrabbed()
    {

        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        shielding = false;
        rb.linearVelocity = Vector3.zero;
        if (grabbedPositionTransform != null)
            this.transform.position = grabbedPositionTransform.position;
        else
        {
            state = State.Normal;
        }
    }
    protected void HandleGrabbing()
    {

        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        returnSpeed = 15f;

        returningLeft = false;
        returningRight = false;
        rb.linearVelocity = Vector3.zero;
        if (punchedLeft && !releasedLeft)
        {
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * Time.deltaTime);
        }
        if (punchedRight && !releasedRight)
        {
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * Time.deltaTime);
        }
        if (releasedLeft)
        {
            punchedLeft = false;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);
        }
        if (releasedRight)
        {
            punchedLeft = false;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(0, 0, 0), returnSpeed * Time.deltaTime);
        }
        if (releasedLeft && releasedRight)
        {
            returningLeft = true;
            returningRight = true;
            state = State.Normal;
            opponent.Throw(GrabPosition.right.normalized);
        }

    }
    public void Throw(Vector3 direction)
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        if (animatorUpdated != null)
        {
            SetAnimatorToKnockback();
        }
        
        hasLanded = false;
        brakeSpeed = 20f;
        // Debug.Log(damage + " damage");
        //Vector2 direction = new Vector2(rb.position.x - handLocation.x, rb.position.y - handLocation.y); //distance between explosion position and rigidbody(bluePlayer)
        //direction = direction.normalized;
        float throwValue = (25 * ((120) * (3 / 2)) / 100) + 14;
        currentPercentage += 8f;
        rb.linearVelocity = (direction * throwValue);
        HitImpact(direction);
        state = State.Knockback;
    }
    public void Bounce(Vector3 direction)
    {
        if (animatorUpdated != null)
        {
            SetAnimatorToKnockback();
        }

        hasLanded = false;
        brakeSpeed = 20f;
        // Debug.Log(damage + " damage");
        //Vector2 direction = new Vector2(rb.position.x - handLocation.x, rb.position.y - handLocation.y); //distance between explosion position and rigidbody(bluePlayer)
        //direction = direction.normalized;
        float throwValue = (14 * ((120) * (3 / 2)) / 100) + 50;
        currentPercentage += 0f;
        rb.linearVelocity = (direction * throwValue);
        state = State.Knockback;
    }
    protected void AirDodge()
    {
        isParrying = true;
        shield.gameObject.SetActive(true);
        parryTimer = 0f;
        state = State.AirDodging;
    }
    protected void HandleAirDodge()
    {
        parryTimer += Time.deltaTime;
        if (parryTimer > parryTimerThreshold && isParrying == true)
        {
            isParrying = false;
            shield.gameObject.SetActive(false);
            shielding = false;
        }
    }

    protected void SetAnimatorToKnockback()
    {
        animatorUpdated.SetBool("punchingLeft", false);
        animatorUpdated.SetBool("punchingRight", false);
        animatorUpdated.SetBool("Rolling", false);
        animatorUpdated.SetFloat("MoveSpeed", 0f);
        animatorUpdated.SetBool("Landing", false);
        animatorUpdated.SetBool("Knockback", true);

        animatorUpdated.SetBool("Dashing", false);
        hasChangedFromKnockbackToFallingAnimation = false;
        if (continuedStunSpawned != null) Destroy(continuedStunSpawned);
    }
    protected void SetAnimatorToIdle()
    {
        animatorUpdated.SetBool("punchingLeft", false);
        animatorUpdated.SetBool("punchingRight", false);
        animatorUpdated.SetBool("Rolling", false);
        animatorUpdated.SetFloat("MoveSpeed", 0f);
        animatorUpdated.SetBool("Knockback", false);

        animatorUpdated.SetBool("Landing", true);
        animatorUpdated.SetBool("Landing", false);

        animatorUpdated.SetBool("Stunned", false);
        animatorUpdated.SetBool("Dashing", false);
        hasChangedFromKnockbackToFallingAnimation = false;
        if (knockbackSmoke != null) knockbackSmoke.Stop();
    }
    protected void SetAnimatorToStunned()
    {
        animatorUpdated.SetBool("punchingLeft", false);
        animatorUpdated.SetBool("punchingRight", false);
        animatorUpdated.SetBool("Rolling", false);
        animatorUpdated.SetFloat("MoveSpeed", 0f);
        animatorUpdated.SetBool("Knockback", false);

        animatorUpdated.SetBool("Landing", true);
        animatorUpdated.SetBool("Landing", false);

        animatorUpdated.SetBool("Dashing", false);
        hasChangedFromKnockbackToFallingAnimation = false;
        if (knockbackSmoke != null) knockbackSmoke.Stop();
        Debug.Log("Parry stunned animator");
        animatorUpdated.SetTrigger("Stunned");
    }




    #region inputRegion
    
    protected virtual void FaceLookDirection()
    {
        if (punchedLeft || punchedRight || returningRight || returningLeft) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;
        if (grabbing) return;
        
        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0f)
        {
            lastLookedPosition = lookTowards;
        }
        
        Look();
    }

    protected virtual void Look()
    {
        if (state == State.Knockback) return;
        transform.right = Vector3.MoveTowards(transform.right, lastLookedPosition, 100 * Time.deltaTime);
    }

    protected void CheckForInputs()
    {
        CheckForPunchRight();
        CheckForPunchLeft();
        CheckForShield();
        CheckForDash();
        CheckForAirDodge();
        CheckForWaveDash();

    }

    protected virtual void CheckForPunchLeft()
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
        if (returningLeft || punchedLeft) return;
        if (state == State.WaveDahsing && rb.linearVelocity.magnitude > 10f) return;
        
        if (state == State.Knockback) return;
        if (state == State.Stunned) return;
        if (grabbing) return;
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
    protected virtual void CheckForPunchRight()
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
        if (state != State.Normal && state != State.PowerDashing) return;
        if (returningRight || punchedRight) return;
        if (state == State.WaveDahsing && rb.linearVelocity.magnitude > 10f) return;
        
        if (state == State.Knockback) return;
        if (state == State.Stunned) return;
        if (grabbing) return;
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

    void CheckForShield()
    {
        if (!shielding)
        {
            canShieldAgainTimer -= Time.deltaTime;
        }
        if (releasedShield)
        {
            if (shielding)
            {
                canShieldAgainTimer = inputBuffer;

            }
            shielding = false;


        }
        if (punchedRight || punchedLeft || returningRight || returningLeft)
        {
            shielding = false;
            return;
        }
        if (state == State.Stunned) return;
        if (state == State.Dashing) return;
        if (state == State.WaveDahsing) return;
        if (grabbing) return;
        if (state == State.Grabbed) return;
        if (state == State.Knockback) return;
        if (pressedShield)
        {
            if (canShieldAgainTimer > 0f) return;
            pressedShield = false;
            parryTimer = 0;
            shielding = true;
        }
    }
    protected virtual void CheckForDash()
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
        if (punchedRight || punchedLeft || returningLeft || returningRight) return;


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
            Dash(transform.right);
        }
    }

    void CheckForAirDodge()
    {
        if (state == State.Knockback)
        {
            canAirDodgeTimer += Time.deltaTime;
            if (canAirDodgeTimer < .25f) return; //if the player has only been in knockback for x seconds return
            if (airDodged)
            {
                
                airDodged = false;
                AirDodge();
            }
        }
    }

    protected virtual void CheckForWaveDash()
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
        if (lastMoveDir.magnitude == 0f) return;
        if (state == State.WaveDahsing) return;
        if (state == State.Stunned) return;
        if (state == State.Dashing) return;
        if (state == State.Grabbed) return;
        if (state == State.Knockback) return;
        if (punchedRight || punchedLeft) return;
        if (grabbing) return;
        if (waveDashTimer > 0)
        {

            waveDashBool = true;
            waveDashTimer = 0f;
        }
    }





    #endregion

    #region Input Callbacks

    public bool IsOffline()
    {
        return NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;
    }

    // -----------------------------------------------------
    // OnMove
    // -----------------------------------------------------
    void OnMove(InputValue value)
    {
        inputMovement = value.Get<Vector2>();
        if (currentControlScheme == "Gamepad")
        {
            lookDirection = value.Get<Vector2>();
        }
    }

    // -----------------------------------------------------
    // OnMouseMove
    // -----------------------------------------------------
    void OnMouseMove(InputValue value)
    {
        Debug.Log("mouse movement " + " no raycast though ");
        Vector2 mousePosition = value.Get<Vector2>();

        // Offline logic
        Plane groundPlane = new Plane(Vector3.up, 0f);

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            // Get the point along the ray that intersects the plane
            Vector3 pointOnPlane = ray.GetPoint(distance);

            // Now you have a position at y = 0 
            // exactly below or above the mouse pointer, ignoring scene colliders.
            lookDirection = new Vector2(
                pointOnPlane.x - transform.position.x,
                pointOnPlane.z - transform.position.z
            );

            Debug.Log("mouse movement " + lookDirection);
        }
    }

    // -----------------------------------------------------
    // OnPunchRight
    // -----------------------------------------------------
    void OnPunchRight()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedRight = true;
            releasedRight = false;
        }
        else
        {
            if (!IsOwner) return;
            OnPunchRightServerRpc();
        }
    }

    [ServerRpc]
    private void OnPunchRightServerRpc()
    {
        OnPunchRightClientRpc();
    }

    [ClientRpc]
    private void OnPunchRightClientRpc()
    {
            pressedRight = true;
            releasedRight = false;
      
    }

    // -----------------------------------------------------
    // OnPunchLeft
    // -----------------------------------------------------
    void OnPunchLeft()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedLeft = true;
            releasedLeft = false;
        }
        else
        {
            if (!IsOwner) return;
            OnPunchLeftServerRpc();
        }
    }

    [ServerRpc]
    private void OnPunchLeftServerRpc()
    {
        OnPunchLeftClientRpc();
    }

    [ClientRpc]
    private void OnPunchLeftClientRpc()
    {
            pressedLeft = true;
            releasedLeft = false;
        
    }

    // -----------------------------------------------------
    // OnReleaseRight
    // -----------------------------------------------------
    void OnReleaseRight()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedRight = false;
            releasedRight = true;
        }
        else
        {
            if (!IsOwner) return;
            OnReleaseRightServerRpc();
        }
    }

    [ServerRpc]
    private void OnReleaseRightServerRpc()
    {
        OnReleaseRightClientRpc();
    }

    [ClientRpc]
    private void OnReleaseRightClientRpc()
    {
            pressedRight = false;
            releasedRight = true;
      
    }

    // -----------------------------------------------------
    // OnReleaseLeft
    // -----------------------------------------------------
    void OnReleaseLeft()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedLeft = false;
            releasedLeft = true;
        }
        else
        {
            if (!IsOwner) return;
            OnReleaseLeftServerRpc();
        }
    }

    [ServerRpc]
    private void OnReleaseLeftServerRpc()
    {
        OnReleaseLeftClientRpc();
    }

    [ClientRpc]
    private void OnReleaseLeftClientRpc()
    {
            pressedLeft = false;
            releasedLeft = true;
      
    }

    // -----------------------------------------------------
    // AltPunchRight / AltPunchLeft / AltReleaseRight / AltReleaseLeft
    // (They call the same ServerRpcs or new ones. Example reuses the same.)
    // -----------------------------------------------------
    void OnAltPunchRight()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedRight = true;
            releasedRight = false;
        }
        else
        {
            if (!IsOwner) return;
            OnPunchRightServerRpc();
        }
    }

    void OnAltPunchLeft()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedLeft = true;
            releasedLeft = false;
        }
        else
        {
            if (!IsOwner) return;
            OnPunchLeftServerRpc();
        }
    }

    void OnAltReleaseRight()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedRight = false;
            releasedRight = true;
        }
        else
        {
            if (!IsOwner) return;
            OnReleaseRightServerRpc();
        }
    }

    void OnAltReleaseLeft()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedLeft = false;
            releasedLeft = true;
        }
        else
        {
            if (!IsOwner) return;
            OnReleaseLeftServerRpc();
        }
    }

    // -----------------------------------------------------
    // OnShield
    // -----------------------------------------------------
    void OnShield()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedShield = true;
            releasedShield = false;
            airDodged = true;
            releasedAirDodged = false;
        }
        else
        {
            if (!IsOwner) return;
            OnShieldServerRpc();
        }
    }

    [ServerRpc]
    private void OnShieldServerRpc()
    {
        OnShieldClientRpc();
    }

    [ClientRpc]
    private void OnShieldClientRpc()
    {
            pressedShield = true;
            releasedShield = false;
            airDodged = true;
            releasedAirDodged = false;
       
    }

    // -----------------------------------------------------
    // OnReleaseShield
    // -----------------------------------------------------
    void OnReleaseShield()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            releasedShield = true;
            pressedShield = false;
            airDodged = false;
            releasedAirDodged = true;
        }
        else
        {
            if (!IsOwner) return;
            OnReleaseShieldServerRpc();
        }
    }

    [ServerRpc]
    private void OnReleaseShieldServerRpc()
    {
        OnReleaseShieldClientRpc();
    }

    [ClientRpc]
    private void OnReleaseShieldClientRpc()
    {
            releasedShield = true;
            pressedShield = false;
            airDodged = false;
            releasedAirDodged = true;
       
    }

    // -----------------------------------------------------
    // OnDash
    // -----------------------------------------------------
    void OnDash()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedDash = true;
            releasedDash = false;
        }
        else
        {
            if (!IsOwner) return;
            OnDashServerRpc();
        }
    }

    [ServerRpc]
    private void OnDashServerRpc()
    {
        OnDashClientRpc();
    }

    [ClientRpc]
    private void OnDashClientRpc()
    {
            pressedDash = true;
            releasedDash = false;
        
    }

    // -----------------------------------------------------
    // OnReleaseDash
    // -----------------------------------------------------
    void OnReleaseDash()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedDash = false;
            releasedDash = true;
        }
        else
        {
            if (!IsOwner) return;
            OnReleaseDashServerRpc();
        }
    }

    [ServerRpc]
    private void OnReleaseDashServerRpc()
    {
        OnReleaseDashClientRpc();
    }

    [ClientRpc]
    private void OnReleaseDashClientRpc()
    {
            pressedDash = false;
            releasedDash = true;
        
    }

    // -----------------------------------------------------
    // OnAButtonDown
    // -----------------------------------------------------
    void OnAButtonDown()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedWaveDash = true;
            releasedWaveDash = false;
        }
        else
        {
            if (!IsOwner) return;
            OnAButtonDownServerRpc();
        }
    }

    [ServerRpc]
    private void OnAButtonDownServerRpc()
    {
        OnAButtonDownClientRpc();
    }

    [ClientRpc]
    private void OnAButtonDownClientRpc()
    {
            pressedWaveDash = true;
            releasedWaveDash = false;
       
    }

    // -----------------------------------------------------
    // OnAButtonUp
    // -----------------------------------------------------
    void OnAButtonUp()
    {
        if (GameConfigurationManager.Instance != null && GameConfigurationManager.Instance.isPaused) return;

        if (IsOffline())
        {
            pressedWaveDash = false;
            releasedWaveDash = true;
        }
        else
        {
            if (!IsOwner) return;
            OnAButtonUpServerRpc();
        }
    }

    [ServerRpc]
    private void OnAButtonUpServerRpc()
    {
        OnAButtonUpClientRpc();
    }

    [ClientRpc]
    private void OnAButtonUpClientRpc()
    {
            pressedWaveDash = false;
            releasedWaveDash = true;
        
    }

    // -----------------------------------------------------
    // OnReset (Local only)
    // -----------------------------------------------------
    void OnReset()
    {
        if (GameConfigurationManager.Instance != null)
        {
            GameConfigurationManager.Instance.ResetToGameModeSelect();
        }
    }

    // -----------------------------------------------------
    // OnStartButton (Local only)
    // -----------------------------------------------------
    void OnStartButton()
    {
        if (GameConfigurationManager.Instance != null)
        {
            GameConfigurationManager.Instance.Pause();
        }
    }

    #endregion



}
