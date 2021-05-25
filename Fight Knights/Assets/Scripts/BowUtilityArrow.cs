using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowUtilityArrow : MonoBehaviour
{
    float lifeTimer;
    float speed = 20f;
    PlayerController opponent;
    PlayerController player;
    bool connected = false;
    float throwAfterSecs = 0f;
    [SerializeField]LayerMask environment;
    Rigidbody rb;
    // Start is called before the first frame update
    public void SetPlayer(PlayerController playerSent)
    {
        player = playerSent;
    }

    private void Start()
    {
        rb = this.gameObject.GetComponentInChildren<Rigidbody>();
    }
    private void Update()
    {
        if (rb.velocity == Vector3.zero)
        {
            
        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 9)
        {
            Debug.Log("Collided with environment");
            this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        if (other.transform.parent.GetComponent<PlayerController>() != null) opponent = other.transform.parent.GetComponent<PlayerController>();
        if (opponent != null && opponent != player)
        {
            if (opponent.isParrying)
            {
                opponent.Parry();
                player.ParryStun();
                player.EndPunchRight();
                Destroy(this.gameObject);
                return;
            }
            else
            {
                connected = true;
                opponent.Grabbed(player, this.transform);
                this.gameObject.GetComponentInChildren<Rigidbody>().velocity *= .5f;
            }
            Vector3 punchTowards = new Vector3(player.transform.right.normalized.x, 0, player.transform.right.normalized.z);
            
            
        }
    }

    public void ReleaseOpponent()
    {
        if (opponent != null)
        {
            Debug.Log("Release opponent");
            opponent.state = PlayerController.State.Normal;   
        }
    }
}
