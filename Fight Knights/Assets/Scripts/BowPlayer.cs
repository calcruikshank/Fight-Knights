using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BowPlayer : PlayerController
{
    float heldArrowTime;
    bool canShoot, maxCharged;
    [SerializeField] GameObject arrowPrefab, forcePushPrefab, arrowIndicator, glowParentPrefab, diagonalArrowPrefab, explosionPrefab;
    GameObject arrowInstantiated, directionArrow, glowParentInst, diagArrowInst, explosionInst;
    bool canShootUtil = true;
    float arrowSpeed = 80f;
    bool hasDashed;
    [SerializeField] Transform bowTransformUtility;
    protected override void HandleThrowingHands()
    {
        if (directionArrow != null && !punchedLeft)
        {
            Destroy(directionArrow);
        }
        if (directionArrow != null && punchedLeft)
        {
            directionArrow.transform.position = this.transform.position;
            directionArrow.transform.rotation = this.transform.rotation;
            if (heldArrowTime < 1.25f)
            {
                heldArrowTime += Time.deltaTime;
            }

        }
        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            animatorUpdated.SetBool("punchingLeft", (punchedLeft));

            animatorUpdated.SetBool("returningLeft", (returningLeft));
        }
        if (punchedLeft && returningLeft == false)
        {
            if (directionArrow == null)
            {
                maxCharged = false;
                directionArrow = Instantiate(arrowIndicator, transform.position, transform.rotation);
            }
            animatorUpdated.SetBool("Rolling", false);
            punchedLeftTimer = 0;
            //leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange && returningLeft == false)
            {
                canShoot = true;
            }
            if((heldArrowTime + 1f) * 6f >= 12 && maxCharged == false)
            {
                maxCharged = true;
                glowParentInst = Instantiate(glowParentPrefab, GrabPosition.position, transform.rotation);
            }
            if (glowParentInst != null)
            {
                glowParentInst.transform.position = GrabPosition.position;
                glowParentInst.transform.right = GrabPosition.right;
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


        if (punchedRight)
        {
            if (!canShoot && punchedLeft) return;

            if (canShoot)
            {
                if (glowParentInst != null)
                {
                    Destroy(glowParentInst);
                }
                arrowInstantiated = Instantiate(arrowPrefab, GrabPosition.position, transform.rotation);
                arrowInstantiated.GetComponent<Rigidbody>().AddForce((transform.right) * (arrowSpeed * (heldArrowTime + .8f)), ForceMode.Impulse);
                arrowInstantiated.GetComponent<HandleCollider>().SetPlayer(this, rightHandTransform);

                arrowInstantiated.GetComponent<HandleCollider>().greatestDamage = (int)((heldArrowTime + 1f) * 6f);


                if (maxCharged)
                {
                    arrowInstantiated.GetComponent<HandleCollider>().breaksShield = true;
                    Debug.Log("Setting break shield to true");

                    maxCharged = false;
                }
                
                //Debug.Log((arrowInstantiated.GetComponent<HandleCollider>().greatestDamage));
                heldArrowTime = 0f;
                canShoot = false;
            }

            
            animatorUpdated.SetBool("Rolling", false);
            punchedRightTimer = 0;
            //rightHandCollider.enabled = true;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, .4f), punchSpeed * 1.5f * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                if (!punchedLeft && !returningLeft && !punchedLeft)
                {
                    GameObject forcePushInst = Instantiate(forcePushPrefab, GrabPosition.position, transform.rotation);
                    forcePushInst.GetComponent<HandleCollider>().SetPlayer(this, rightHandTransform);
                }
                returningRight = true;
            }

            returningLeft = true;
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

        if (diagArrowInst != null)
        {
            if (diagArrowInst.transform.position.y <= bowTransformUtility.position.y)
            {
                diagArrowInst.GetComponentInChildren<Rigidbody>().linearVelocity = Vector3.zero;
                //diagArrowInst.GetComponent<BowUtilityArrow>().ReleaseOpponent();
                Collider[] collidersInDiagArrow = diagArrowInst.gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider collider in collidersInDiagArrow)
                {
                    collider.enabled = false;
                }
            }
        }


        if (grabbing) return;
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
            moveSpeed = moveSpeedSetter - 12f;
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
            returnSpeed = 12f;
        }

    }





    public override void EndPunchRight()
    {

        punchedRight = false;
        returningRight = true;
        rightHandTransform.gameObject.GetComponent<Collider>().enabled = false;
    }
    protected override void EndPunchLeft()
    {
        //Debug.Log((arrowInstantiated.GetComponent<HandleCollider>().greatestDamage));
        heldArrowTime = 0f;
        canShoot = false;
        punchedLeft = false;
        returningLeft = true;
        leftHandTransform.gameObject.GetComponent<Collider>().enabled = false;
    }

    protected override void WaveDash(Vector3 powerDashDirection, float sentSpeed)
    {
        EndPunchRight();
        shielding = false;
        transform.right = lastMoveDir;
        shielding = false;
        canShieldAgainTimer = 0f;
        powerDashSpeed = sentSpeed;
        powerDashTowards = new Vector3(powerDashDirection.normalized.x, rb.linearVelocity.y, powerDashDirection.normalized.z);
        state = State.WaveDahsing;
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
    protected override void Dash(Vector3 dashDirection)
    {
        if (diagArrowInst != null)
        {
            this.transform.position = diagArrowInst.transform.position;
            explosionInst = Instantiate(explosionPrefab, this.transform.position + movement, this.transform.rotation);
            explosionInst.GetComponent<HandleCollider>().SetPlayer(this, leftHandTransform);
            Destroy(diagArrowInst);
            return;
        }
        if (canShoot)
        {
            if (!canShootUtil) return;
            animatorUpdated.SetBool("punchingRight", (true));
            if (glowParentInst != null)
            {
                Destroy(glowParentInst);
            }
            diagArrowInst = Instantiate(diagonalArrowPrefab, GrabPosition.position, transform.rotation);
            diagArrowInst.GetComponent<Rigidbody>().AddForce((bowTransformUtility.right) * (arrowSpeed * (heldArrowTime + .8f)), ForceMode.Impulse);
            diagArrowInst.GetComponent<BowUtilityArrow>().SetPlayer(this);
            
            if (maxCharged)
            {

                maxCharged = false;
            }
            //Debug.Log((arrowInstantiated.GetComponent<HandleCollider>().greatestDamage));
            heldArrowTime = 0f;
            canShoot = false;

            returningLeft = true;

            StartCoroutine(shotUtilityArrowRoutine(1.25f));
        }
        else
        {
            canShoot = true;
            WaveDash(lastMoveDir, 60f);
            punchedLeft = true;
            returningLeft = false;
        }
    }
    private IEnumerator shotUtilityArrowRoutine(float grabtime)
    {
        canShootUtil = false;
        yield return new WaitForSecondsRealtime(grabtime);
        canShootUtil = true;
    }
    protected override void HandleDash()
    {
        dashTimer -= Time.deltaTime;
        moveSpeed = moveSpeedSetter + 10f;
        if (dashTimer <= 0)
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
        if (!canDash) return;

        if (grabbing) return;
        if (state == State.Grabbed) return;

        //check if hasdashedtimer is good to go if not return

        //then if dash buffer is greater than 0 dash
        if (dashBuffer > 0)
        {
            Debug.Log("PresedDash" + dashBuffer);
            hasDashed = true;
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }
            
            dashBuffer = 0;
        }

        if (punchedLeft && !canShoot) return;
        if (hasDashed)
        {
            Dash(transform.right.normalized);
            hasDashed = false;
        }
    }

    protected override void FaceLookDirection()
    {
        if (state == State.WaveDahsing) return;
        if (grabbing) return;
        if (punchedRight || returningRight && rightHandTransform.localPosition.x > punchRange - 1f) return;
        if (hasDashed) return;
        if (returningLeft) return;
        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.magnitude != 0f)
        {
            lastLookedPosition = lookTowards;
        }

        Look();
    }
}
