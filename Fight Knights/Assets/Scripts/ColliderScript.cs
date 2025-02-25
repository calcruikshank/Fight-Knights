using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderScript : MonoBehaviour
{
    PlayerController opponent;
    [SerializeField] int hitID;
    [SerializeField] float damage;
    [SerializeField] float stunDamage;
    [SerializeField] float colliderThreshold = .2f;
    [SerializeField] bool moreDamageIfStunned = false;
    [SerializeField] float activateAfterSeconds = 0f;
    bool hasSetActive = false;
    float collideTimer;

    private void Update()
    {
        collideTimer += Time.deltaTime;
        if (collideTimer > activateAfterSeconds && !hasSetActive)
        {
            this.GetComponent<Collider>().enabled = true;
            hasSetActive = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent)
        {

            opponent = other.transform.parent.GetComponent<PlayerController>();
            if (opponent != null && collideTimer <= colliderThreshold)
            {
                if (!moreDamageIfStunned || opponent.state != PlayerController.State.Stunned)
                {
                    this.transform.parent.transform.parent.GetComponent<HandleCollider>().HandleCollision(hitID, damage, opponent);
                }
                if (moreDamageIfStunned && opponent.state == PlayerController.State.Stunned)
                {
                    this.transform.parent.transform.parent.GetComponent<HandleCollider>().HandleCollision(hitID, stunDamage, opponent);
                }
                Collider[] colliders = opponent.transform.GetComponentsInChildren<Collider>();
                Collider[] collidersInColliderParents = this.transform.parent.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    foreach (Collider collidersInParent in collidersInColliderParents)
                    {
                        Physics.IgnoreCollision(collider, collidersInParent);
                    }
                }

            }
        }
        
    }
}
