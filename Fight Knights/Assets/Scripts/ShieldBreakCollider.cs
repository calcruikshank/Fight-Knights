using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBreakCollider : MonoBehaviour
{
    PlayerController opponent;
    [SerializeField] int hitID;
    [SerializeField] float damage;
    [SerializeField] float stunTime = 1f;
    [SerializeField] float colliderThreshold = .2f;

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

            this.transform.parent.transform.parent.GetComponent<HandleColliderShieldBreak>().HandleCollision(hitID, damage, opponent, stunTime);
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
