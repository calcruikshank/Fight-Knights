using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleColliderShieldBreak : MonoBehaviour
{
    public PlayerController player;
    PlayerController opponent;
    public PlayerController opponentHit;
    float greatestDamage = 0f;
    Vector3 punchTowards;
    [SerializeField] bool explodeAfterColliding = false;
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] bool stunsIfOpponentIsntShielding = true;
    // Start is called before the first frame update
    void Start()
    {

    }




    public void SetPlayer(PlayerController player, Transform handSent)
    {

        this.player = player;
    }

    public void HandleCollision(float hitId, float damage, PlayerController sentOpponent, float stunTime)
    {

        if (greatestDamage < damage)
        {
            greatestDamage = damage;
        }
        opponent = sentOpponent;

        if (sentOpponent != player && opponent != opponentHit)
        {
            if (explodeAfterColliding)
            {
                Explode();
            }
            if (opponent.isParrying)
            {
                opponent.Parry();
                player.ParryStun();
                return;
            }
            if (opponent.shielding)
            {
                opponent.Stunned(stunTime, damage);
                opponentHit = sentOpponent;
                return;
            }
            if (stunsIfOpponentIsntShielding)
            {
                opponent.Stunned(stunTime, damage);
                opponentHit = sentOpponent;
                return;
            }


            punchTowards = new Vector3(this.transform.right.normalized.x, 0, this.transform.right.normalized.z);
            if (punchTowards == null || punchTowards == Vector3.zero)
            {
            }

            if (player.isDashing)
            {
                damage = 20f;
            }

            Debug.Log(punchTowards);
            opponent.Knockback(greatestDamage, punchTowards, player);
            opponentHit = sentOpponent;
        }
        greatestDamage = 0f;
        
    }

    public void SetKnockbackDirection(Vector3 direction)
    {


        punchTowards = direction;

    }

    void Explode()
    {
        ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop();
        }
        this.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        if (explosionPrefab != null)
        {
           Instantiate(explosionPrefab, this.transform.position, Quaternion.identity).GetComponent<HandleCollider>().SetPlayer(this.player, this.transform);
        }
    }
}
