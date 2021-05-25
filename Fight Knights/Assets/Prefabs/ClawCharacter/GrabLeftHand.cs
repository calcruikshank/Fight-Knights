using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabLeftHand : MonoBehaviour
{
    PlayerController opponent;
    [SerializeField] PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.localPosition.x < 4f)
        {
            this.gameObject.GetComponent<Collider>().enabled = false;
            
        }
    }

    void OnTriggerEnter(Collider other)
    {

        opponent = other.transform.parent.GetComponent<PlayerController>();


        if (opponent != null && opponent != player)
        {

            if (opponent.isParrying)
            {
                opponent.Parry();
                player.ParryStun();
                return;
            }
            player.Grab(opponent, this.transform);
            Debug.Log("Grab");
            return;

            this.gameObject.GetComponent<Collider>().enabled = false;


        }


    }
}
