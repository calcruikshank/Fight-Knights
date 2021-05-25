using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPlayer : PlayerController
{

    [SerializeField] GameObject rockPrefab, dustPrefab, rockExplosion;
    Rigidbody rockRB;
    RockLaunch rockLaunch;
    GameObject kickDust;
    GameObject rock;
    protected override void HandleThrowingHands()
    {
        if (animatorUpdated != null)
        {
            animatorUpdated.SetBool("punchingRight", (punchedRight));
            animatorUpdated.SetBool("punchingLeft", (punchedLeft));

        }
        if (punchedLeft && returningLeft == false)
        {
            animatorUpdated.SetBool("Rolling", false);
            punchedLeftTimer = 0;
            //leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector3(punchRange, -.4f, -.4f), punchSpeed * 2 * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange)
            {
                kickDust = Instantiate(dustPrefab, GrabPosition.position, Quaternion.identity);
                kickDust.transform.right = transform.right;
                HandleCollider handleCollider = kickDust.GetComponent<HandleCollider>();
                handleCollider.SetPlayer(this, leftHandParent);
                rockRB = kickDust.GetComponent<Rigidbody>();
                rockRB.gameObject.GetComponent<Rigidbody>().AddForce((rockRB.transform.up) * (30f), ForceMode.Impulse);
                rockLaunch = kickDust.GetComponent<RockLaunch>();
                rockLaunch.SetPlayer(this);
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
        }



        if (punchedRight && returningRight == false)
        {


            animatorUpdated.SetBool("Rolling", false);
            punchedRightTimer = 0;
            //rightHandCollider.enabled = true;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector3(punchRange, -.4f, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                rock = Instantiate(rockPrefab, GrabPosition.position, Quaternion.identity);
                rock.transform.right = transform.right;
                HandleCollider handleCollider = rock.GetComponent<HandleCollider>();
                handleCollider.SetPlayer(this, leftHandParent);

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
            moveSpeed = 0f;
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
            returnSpeed = 10f;
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
    }
    protected override void FaceLookDirection()
    {
        if (punchedRight || punchedLeft || leftHandTransform.localPosition.x > 2f && returningLeft || rightHandTransform.localPosition.x > 2f && returningRight) if (state != State.Grabbing) return;
        if (state == State.WaveDahsing) return;

        Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
        if (lookTowards.x != 0 || lookTowards.y != 0)
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

        if (state == State.Grabbed) return;
        if (returningLeft || punchedLeft) return;
        if (punchedRight) return;
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;
        if (shielding) return;
        if (state == State.Knockback) return;

        if (punchedLeftTimer > 0)
        {
            //if (rockLaunch != null) Destroy(rockLaunch.gameObject);
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }


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

        if (state == State.Grabbed) return;
        if (returningRight || punchedRight) return;
        if (state == State.WaveDahsing && rb.velocity.magnitude > 10f) return;
        if (shielding) return;
        if (state == State.Knockback) return;
        if (state == State.Dashing) return;
        if (punchedLeft) return;

        if (punchedRightTimer > 0)
        {
            if (rock != null) Destroy(rock);
            if (lookDirection.magnitude != 0)
            {
                Vector3 lookTowards = new Vector3(lookDirection.x, 0, lookDirection.y);
                transform.right = lookTowards;
            }

            punchedRight = true;
            punchedRightTimer = 0;
        }
    }
}
