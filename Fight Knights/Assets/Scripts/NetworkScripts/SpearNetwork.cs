using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpearNetwork : NetworkPlayerController
{
    public List<string> animations;
    [SerializeField] GameObject spearSlashPrefab, spearThrustPrefab;
    [SerializeField] Transform spearPoint;
    GameObject spearSlashInstantiated, spearThrustInstantiated;
    public override void Awake()
    {
        animations = new List<string>()
            {
                "Attack",
                "Active",
                "Passive"
            };
        Application.targetFrameRate = 600;

        rb = GetComponent<Rigidbody>();
        state = State.Normal;
        cameraShake = FindObjectOfType<CameraShake>();
        canDash = true;
        grabbing = false;
        releasedLeft = true;
        releasedRight = true;
        if (animatorUpdated != null)
        {
            SetAnimatorToIdle();
        }

    }

    public override void EndPunchRight()
    {

        punchedRight = false;
        returningRight = false;
        //rightHandTransform.gameObject.GetComponent<Collider>().enabled = false;
    }
    protected override void EndPunchLeft()
    {
        punchedLeft = false;
        returningLeft = false;
        //leftHandTransform.gameObject.GetComponent<Collider>().enabled = false;
    }
    protected override void HandleMovement()
    {
        if (punchedRight)
        {
            return;
        }
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
                animatorUpdated.SetFloat("speedv", (movement.magnitude));

            }
            else
            {
                animatorUpdated.SetFloat("speedv", (0));
            }
        }
    }

    protected override void HandleThrowingHands()
    {
        
        if (punchedLeft && returningLeft == false)
        {
        }
        if (punchedRight && returningRight == false)
        {
        }
        if (returningRight)
        {
        }
        
        if (returningLeft || returningRight)
        {
            moveSpeed = 0;
            return;
        }
        if (punchedLeft)
        {
            moveSpeed = moveSpeedSetter / 2;
            return;
        }
        if (state == State.Dashing)
        {

            moveSpeed = moveSpeedSetter + 10;
            return;
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

    IEnumerator PunchLeftRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        punchedLeft = false;
    }
    IEnumerator StartHitboxRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        StartCoroutine(StartHitboxRoutineTwo(.18f));
        SpawnLeftParticleServerRpc(spearPoint.position, spearPoint.right);
    }
    IEnumerator StartHitboxRoutineTwo(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        SpawnLeftParticleServerRpc(spearPoint.position, spearPoint.right);
        StartCoroutine(StartHitboxRoutineThree(.192f));
    }
    IEnumerator StartHitboxRoutineThree(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        SpawnLeftParticleServerRpc(spearPoint.position, spearPoint.right);
    }
    IEnumerator PunchRightRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);
        returningRight = true;
        SpawnRightParticleServerRpc(GrabPosition.position, transform.right);
        StartCoroutine(RecoverRightRoutine(.525f));
    }
    IEnumerator RecoverRightRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);

        punchedRight = false;
        returningRight = false;
    }

    IEnumerator WaveDashRoutine(float timeSent)
    {
        yield return new WaitForSeconds(timeSent);

        state = State.Normal;
    }
    protected override void FaceLookDirection()
    {
        if (grabbing) return;

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0f)
        {

            lastLookedPosition = lookTowards;
        }

        if (punchedLeft || punchedRight || returningRight) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;
        Look();
    }

    protected override void WaveDash(Vector3 powerDashDirection, float sentSpeed)
    {
        EndPunchLeft();
        EndPunchRight();
        shielding = false;
        transform.right = lastMoveDir;
        shielding = false;
        canShieldAgainTimer = 0f;
        powerDashSpeed = sentSpeed;
        powerDashTowards = new Vector3(powerDashDirection.normalized.x, rb.velocity.y, powerDashDirection.normalized.z);
        state = State.WaveDahsing;
        animatorUpdated.SetTrigger("Roll");
    }

    protected override void HandleWaveDashing()
    {
        transform.right = lastMoveDir;
        Time.timeScale = 1;
        float powerDashSpeedMulti = 4f;
        powerDashSpeed -= powerDashSpeed * powerDashSpeedMulti * Time.deltaTime;

        float powerDashMinSpeed = 10f;
        if (powerDashSpeed < powerDashMinSpeed)
        {
            powerDashSpeed = 10f;
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

        if (returningRight || punchedRight) return;
        if (returningLeft || punchedLeft) return;
        if (state == State.WaveDahsing) return;

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
            animatorUpdated.SetTrigger("Attack");
            punchedLeft = true;
            punchedLeftTimer = 0;
            StartCoroutine(PunchLeftRoutine(.6f));
            StartCoroutine(StartHitboxRoutine(.12f));
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
        if (returningLeft || punchedLeft) return;
        if (state == State.WaveDahsing) return;

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
            StartCoroutine(PunchRightRoutine(.525f));
            animatorUpdated.SetTrigger("Passive");
            punchedRightTimer = 0;
        }
    }
    protected override void HandleDash()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0)
        {

            isDashing = false;
            state = State.Normal;
        }
    }

    protected override void OnMove(InputValue value)
    {
        inputMovement = value.Get<Vector2>();

        lookDirection = value.Get<Vector2>();
        
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
        if (lastMoveDir.magnitude == 0f) return;
        if (punchedLeft || returningLeft || returningRight || punchedRight) return;
        if (state == State.WaveDahsing) return;
        if (state == State.Stunned) return;
        if (state == State.Dashing) return;
        if (state == State.Grabbed) return;
        if (state == State.Knockback) return;
        if (grabbing) return;
        if (waveDashTimer > 0)
        {
            StartCoroutine(WaveDashRoutine(.6f));
            waveDashBool = true;
            waveDashTimer = 0f;
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
        if (!canDash) return;
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
            Dash(transform.right.normalized);
            canDash = false;
        }
    }
    protected override void Dash(Vector3 dashDirection)
    {
        animatorUpdated.SetTrigger("Active");
        StartCoroutine(DashRoutine(1f));
        canDash = false;
        isDashing = true;
        dashTimer = 3f;
        state = State.Dashing;

    }

    IEnumerator DashRoutine(float timeSent)
    {
        returningRight = true;
        yield return new WaitForSeconds(timeSent);
        returningRight = false;
        canDash = true;
    }
    [ServerRpc]
    private void SpawnRightParticleServerRpc(Vector3 dir, Vector3 rot)
    {
        SpawnRightParticleClientRpc(dir, rot);
    }
    [ClientRpc]
    private void SpawnRightParticleClientRpc(Vector3 dir, Vector3 rot)
    {

        spearSlashInstantiated = Instantiate(spearSlashPrefab, dir, transform.rotation);
        spearSlashInstantiated.transform.right = rot;
        HandleColliderNetwork handleCollider = spearSlashInstantiated.GetComponent<HandleColliderNetwork>();
        handleCollider.SetPlayer(this, leftHandParent);
    }
    [ServerRpc]
    private void SpawnLeftParticleServerRpc(Vector3 dir, Vector3 rot)
    {
        SpawnLeftParticleClientRpc(dir, rot);
    }
    [ClientRpc]
    private void SpawnLeftParticleClientRpc(Vector3 dir, Vector3 rot)
    {

        spearThrustInstantiated = Instantiate(spearThrustPrefab, dir, Quaternion.identity);
        spearThrustInstantiated.transform.right = rot;
        HandleColliderNetwork handleCollider = spearThrustInstantiated.GetComponent<HandleColliderNetwork>();
        handleCollider.SetPlayer(this, leftHandParent);
    }

}
