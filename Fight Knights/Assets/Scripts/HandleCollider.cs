using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleCollider : MonoBehaviour
{
    public PlayerController player;
    PlayerController opponent;
    PlayerController opponentHit;
    public float greatestDamage = 0f;
    Vector3 punchTowards;
    bool setDirection = false;
    [SerializeField] bool destroyedOnImpact = false;
    [SerializeField] bool gameObjectDestroyedOnImpact = false;
    public bool breaksShield = false;
    [SerializeField] bool throws = false;
    // Start is called before the first frame update
    void Start()
    {
        setDirection = false;
    }



    public void SetPlayer(PlayerController player, Transform handSent)
    {
        
        this.player = player;
    }

    public void HandleCollision(float hitId, float damage, PlayerController sentOpponent)
    {
        if (greatestDamage < damage)
        {
            //Debug.Log(greatestDamage + " greatest before changing");
            greatestDamage = damage;
        }
        opponent = sentOpponent;
        if (sentOpponent != player && opponent != opponentHit)
        {
            if (opponent.isParrying)
            {
                opponent.Parry();
                player.ParryStun();
                if (destroyedOnImpact)
                {
                    this.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                    Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = false;
                    }
                    this.gameObject.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

                    ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem particle in particles)
                    {
                        if (particle != null)
                        {
                            particle.Stop();
                        }
                    }
                }
                if (gameObjectDestroyedOnImpact)
                {
                    Debug.Log("destroy");
                    Destroy(this.gameObject);
                }
                return;
            }



            if (opponent.shielding && !breaksShield)
            {
                if (destroyedOnImpact)
                {
                    this.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                    Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = false;
                    }
                    this.gameObject.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

                    ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem particle in particles)
                    {
                        if (particle != null)
                        {
                            particle.Stop();
                        }
                    }
                }
                if (gameObjectDestroyedOnImpact)
                {
                    Debug.Log("destroy");
                    Destroy(this.gameObject);
                }
                return;
            }
            if (opponent.shielding && breaksShield)
            {
                if (destroyedOnImpact)
                {
                    this.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                    Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = false;
                    }
                    this.gameObject.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

                    ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem particle in particles)
                    {
                        if (particle != null)
                        {
                            particle.Stop();
                        }
                    }
                }
                if (gameObjectDestroyedOnImpact)
                {
                    Debug.Log("destroy");
                    Destroy(this.gameObject);
                }
                opponent.Stunned(.1f, 0);
                return;
            }
            if (setDirection == false)
            {
                punchTowards = new Vector3(this.transform.right.normalized.x, 0, this.transform.right.normalized.z);
            }
            if (player != null)
            {
                if (player.isDashing)
                {
                    damage = 20f;
                }
            }

            Debug.Log("Shield Poke" + greatestDamage);
            //Debug.Log("Knockback" + greatestDamage);
            if (!throws)
            {
                opponent.Knockback(greatestDamage, punchTowards, player);
            }
            if (throws)
            {
                opponent.Throw(punchTowards);
            }
            opponentHit = sentOpponent;
            if (gameObjectDestroyedOnImpact)
            {
                Debug.Log("destroy");
                Destroy(this.gameObject);
            }
            if (destroyedOnImpact)
            {
                this.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }
                this.gameObject.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
                
                ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Stop();
                    }
                }
            }
        }
    }

    public void SetKnockbackDirection(Vector3 direction)
    {
        
        
        punchTowards = direction;
        setDirection = true;

    }
}
