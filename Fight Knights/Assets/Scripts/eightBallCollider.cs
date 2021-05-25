using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eightBallCollider : MonoBehaviour
{
    PlayerController opponent;
    float timeBetweenCollisions = 0f;
    // Start is called before the first frame update
    void Start()
    {
        timeBetweenCollisions = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        timeBetweenCollisions += Time.deltaTime;
    }
    void OnTriggerEnter(Collider collision)
    {
        
        opponent = collision.transform.parent.GetComponent<PlayerController>();
        if (opponent != null)
        {
            if (opponent.isParrying)
            {
                opponent.Parry();
                this.transform.parent.transform.parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
                this.transform.parent.transform.parent.GetComponent<PlayerController>().state = PlayerController.State.Normal;
                this.transform.parent.transform.parent.GetComponent<BallCharacter>().bodyCollider.enabled = false;
                timeBetweenCollisions = 0f;
                this.transform.parent.transform.parent.GetComponent<PlayerController>().ParryStun();
                this.transform.parent.transform.parent.GetComponent<PlayerController>().EndPunchRight();
                
                return;
            }
            if (timeBetweenCollisions > .1f)
            {
                if (this.transform.parent.transform.parent.GetComponent<PlayerController>().state != PlayerController.State.Knockback)
                {
                    int damage = (int)this.transform.parent.transform.parent.GetComponent<Rigidbody>().velocity.magnitude / 3;
                    if (damage > 12)
                    {
                        damage = 12;
                    }
                    if (damage < 8)
                    {
                        damage = 8;
                    }
                    opponent.Knockback(damage, this.transform.parent.right, this.transform.parent.GetComponent<PlayerController>());
                    this.transform.parent.transform.parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    this.transform.parent.transform.parent.GetComponent<PlayerController>().state = PlayerController.State.Normal;
                    this.transform.parent.transform.parent.GetComponent<BallCharacter>().bodyCollider.enabled = false;
                    timeBetweenCollisions = 0f;
                }
                
            }
            
        }
    }
}
