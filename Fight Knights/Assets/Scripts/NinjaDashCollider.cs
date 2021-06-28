using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaDashCollider : MonoBehaviour
{
    Collider hitBox;
    PlayerController opponent, player;
    NinjaScript ninjaScript;
    List<Collider> opponents = new List<Collider>();
    Vector3 punchTowards;
    float damage = 5;
    bool ignorningCollider;
    // Start is called before the first frame update
    void Start()
    {
        hitBox = this.GetComponent<Collider>();
        ninjaScript = this.transform.parent.GetComponent<NinjaScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        opponent = other.transform.parent.GetComponent<PlayerController>();
        if (opponent != null)
        {
            if (opponent.isParrying)
            {
                opponent.Parry();
                player.ParryStun();

                return;
            }

            if (opponent.shielding)
            {
                return;
            }
            punchTowards = new Vector3(-this.transform.forward.normalized.x, 0, -this.transform.forward.normalized.z);
            opponent.Knockback(damage, punchTowards, player);
            Physics.IgnoreCollision(hitBox, other, true);
            opponents.Add(other);
            ignorningCollider = true;
        }
        if (other.transform.GetComponent<Environment>() != null)
        {
            ninjaScript.EndDash();
        }
    }

    public void TurnOnCollisions()
    {
        foreach (Collider col in opponents)
        {
            Physics.IgnoreCollision(hitBox, col, false);
        }
        ignorningCollider = false; 
    }

    public bool hasNDCed()
    {
        return ignorningCollider;
    }
}
