using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownHammer : MonoBehaviour
{
    PlayerController opponent;
    PlayerController player;
    LightningBall lightningBall;
    [SerializeField] GameObject lightning;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetPlayer(PlayerController playerSent)
    {
        player = playerSent;
    }

    void OnTriggerEnter(Collider other)
    {
        lightningBall = other.transform.parent.GetComponent<LightningBall>();
        if (lightningBall != null)
        {
            player.HitImpact(this.transform.right);
            Instantiate(lightning, new Vector3(lightningBall.transform.position.x, 0, lightningBall.transform.position.z), Quaternion.identity);
            lightningBall.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            if (this.transform.GetComponentInChildren<Rigidbody>().velocity.magnitude != 0f)
            {
                lightningBall.gameObject.GetComponent<Rigidbody>().AddForce((this.transform.right) * (80f), ForceMode.Impulse);
            }
            if (this.transform.GetComponentInChildren<Rigidbody>().velocity.magnitude == 0f)
            {
                lightningBall.gameObject.GetComponent<Rigidbody>().AddForce((-this.transform.right) * (80f), ForceMode.Impulse);
            }
            Physics.IgnoreCollision(this.transform.GetComponent<Collider>(), other);
            player.EndPunchRight();
            return;
        }
        opponent = other.transform.parent.GetComponent<PlayerController>();
        if (opponent != null && opponent != player)
        {
            if (opponent.isParrying)
            {
                opponent.Parry();
                player.ParryStun();
                player.EndPunchRight();
                Physics.IgnoreCollision(this.transform.GetComponent<Collider>(), other);
                Collider[] colliders = opponent.transform.GetComponentsInChildren<Collider>();
                Collider[] collidersInColliderParents = this.transform.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    foreach (Collider collidersInParent in collidersInColliderParents)
                    {
                        Physics.IgnoreCollision(collider, collidersInParent);
                    }
                }
                return;
            }
            if (opponent.shielding)
            {
                player.EndPunchRight();
                Physics.IgnoreCollision(this.transform.GetComponent<Collider>(), other);
                return;
            }

            Vector3 punchTowards = new Vector3(this.transform.right.normalized.x, 0, this.transform.right.normalized.z);
            if (this.transform.GetComponentInChildren<Rigidbody>().velocity.magnitude == 0f)
            {
                punchTowards = -punchTowards;
            }
            float damage = 9;
            if (player.isDashing)
            {
                damage = 10f;
            }
            opponent.Knockback(damage, punchTowards, player);
            Physics.IgnoreCollision(this.transform.GetComponent<Collider>(), other);
            Instantiate(lightning, new Vector3(opponent.transform.position.x, 0, opponent.transform.position.z), Quaternion.identity);
            player.EndPunchRight();
        }
        if (other.transform.GetComponent<Environment>() != null && this.gameObject.GetComponentInChildren<Rigidbody>().velocity.magnitude != 0f)
        {
            player.EndPunchRight();
        }
    }
}
