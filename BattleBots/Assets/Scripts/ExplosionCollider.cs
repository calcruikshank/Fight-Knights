using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCollider : MonoBehaviour
{
    PlayerController opponent;
    [SerializeField] int hitID;
    [SerializeField] float damage;
    [SerializeField] float stunDamage;
    [SerializeField] float colliderThreshold = .2f;
    [SerializeField] bool moreDamageIfStunned = false;
    float collideTimer;

    private void Update()
    {
        collideTimer += Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {

        opponent = other.transform.parent.GetComponent<PlayerController>();
        if (opponent != null && collideTimer <= colliderThreshold)
        {
            if (!moreDamageIfStunned || opponent.state != PlayerController.State.Stunned)
            {

                this.transform.parent.transform.parent.GetComponent<HandleCollider>().SetKnockbackDirection(new Vector3(opponent.transform.position.x - this.transform.parent.transform.parent.position.x, 0, opponent.transform.position.z - this.transform.parent.transform.parent.position.z).normalized);
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
