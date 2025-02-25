using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class SoccerBall : PlayerController
{
    [SerializeField]public int billiardBallColor;
    public override void Awake()
    {
        if (!IsOffline())
        {
            var netTransform = gameObject.AddComponent<NetworkTransform>();
            var netRigidbody = gameObject.AddComponent<NetworkRigidbody>();
        }
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
        rb.linearVelocity = Vector3.zero;
    }
    public bool canBeScored = true;
    // Start is called before the first frame update
    void Start()
    {
        canBeScored = true;
    }
    protected override void Update()
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }
        base.Update();
        if (state != State.Grabbed && state != State.Stunned)
        {
            state = State.Knockback;
        }
    }
    protected override void Look()
    {
        
    }

    protected override void HandleThrowingHands()
    {
    }
    public override void Knockback(float damage, Vector3 direction, PlayerController playerSent)
    {
        if (!IsOffline()) // means we are online
        {
            if (!IsServer) return;
        }

        canAirDodgeTimer = 0f;

        currentPercentage += damage;
        brakeSpeed = 10f;
        // Debug.Log(damage + " damage");
        //Vector2 direction = new Vector2(rb.position.x - handLocation.x, rb.position.y - handLocation.y); //distance between explosion position and rigidbody(bluePlayer)
        //direction = direction.normalized;
        float knockbackValue = (20 * ((50 + damage) * (damage / 2)) / 150) + 14; //knockback that scales
        rb.linearVelocity = new Vector3(direction.x * knockbackValue, 10f, direction.z * knockbackValue);

        HitImpact(direction);
        state = State.Knockback;
    }
    protected override void HandleKnockback()
    {

    }

    protected override void Respawn()
    {

        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.gameMode == 1)
            {
                canBeScored = false;
                StartCoroutine(RespawnWait(1f));
            }
            else
            {
                AddToBilliardsScore();
            }
        }
        
    }
    private IEnumerator RespawnWait(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        rb.linearVelocity = Vector3.zero;
        state = State.Normal;
        transform.position = Vector3.zero;
        currentPercentage = 0f;
        canBeScored = true;
    }

    public void AddToBilliardsScore()
    {
        Destroy(this.gameObject);
    }
}
