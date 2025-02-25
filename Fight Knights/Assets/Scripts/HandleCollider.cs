using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HandleCollider : NetworkBehaviour
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

        // Cache the Rigidbody
        colRb = GetComponent<Rigidbody>();
        if (colRb == null)
        {
            colRb = GetComponentInChildren<Rigidbody>();
        }
    }
    private Rigidbody colRb;

    // We'll only use these if we're doing manual server->client sync
    // (If offline, we won't use them, but they won't hurt being here)
    private NetworkVariable<Vector3> colliderpos = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> colliderrot = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Owner);
    private bool IsOffline()
    {
        // If there's no NetworkManager or it's not running/hosting/connected, treat as offline
        return NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;
    }
    
    private void FixedUpdate()
    {
        // ----------------------------------------
        // OFFLINE: local physics simulation
        // ----------------------------------------
        if (IsOffline())
        {
            // If offline, just let normal local physics run.
            // No server, no client. So do nothing special here.
            return;
        }

        // ----------------------------------------
        // ONLINE: server -> updates, client -> follows
        // ----------------------------------------
        if (IsServer)
        {
            // SERVER simulates normal physics (rigidbody) and writes transform to NetworkVariables
            if (colRb != null)
            {
                // Let the server do its usual physics
                // Then store the transform each physics step
                colliderpos.Value = transform.position;
                colliderrot.Value = transform.rotation;
            }
        }
        else
        {
            if (colliderpos.Value != Vector3.zero)
            {
                // CLIENT just follows server’s authoritative transform
                if (colRb != null)
                {
                    colRb.isKinematic = true;
                }
                transform.position = colliderpos.Value;
                transform.rotation = colliderrot.Value;
            }
        }
    }

    public void SetPlayer(PlayerController player, Transform handSent)
    {
        
        this.player = player;
    }

    public void HandleCollision(float hitId, float damage, PlayerController sentOpponent)
    {
        if (NetworkManager.Singleton != null && !IsServer)
        {
            return;
        }
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
                    this.gameObject.GetComponentInChildren<Rigidbody>().linearVelocity = Vector3.zero;

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
                    if (!IsOffline())
                    {
                        if (IsServer)
                        {
                            GetComponent<NetworkObject>().Despawn();
                        }
                    }
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
                    this.gameObject.GetComponentInChildren<Rigidbody>().linearVelocity = Vector3.zero;

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
                    if (!IsOffline())
                    {
                        if (IsServer)
                        {
                            GetComponent<NetworkObject>().Despawn();
                        }
                    }
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
                    this.gameObject.GetComponentInChildren<Rigidbody>().linearVelocity = Vector3.zero;

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
                    if (!IsOffline())
                    {
                        if (IsServer)
                        {
                            GetComponent<NetworkObject>().Despawn();
                        }
                    }
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
                if (!IsOffline())
                {
                    if (IsServer)
                    {
                        GetComponent<NetworkObject>().Despawn();
                    }
                }
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
                this.gameObject.GetComponentInChildren<Rigidbody>().linearVelocity = Vector3.zero;
                
                ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Stop();
                    }
                }
                if (!IsOffline())
                {
                    if (IsServer)
                    {
                        GetComponent<NetworkObject>().Despawn();
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
