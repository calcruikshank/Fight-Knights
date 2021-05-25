using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSwirl : MonoBehaviour
{
    PlayerController opponent;
    PlayerController player;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
            opponent.Stunned(.25f, 0f);
            player.Grab(opponent, this.transform);
            Debug.Log("Grab");
            return;

            this.gameObject.GetComponent<Collider>().enabled = false;


        }


    }

    

    public void SetPlayer(PlayerController player)
    {
        this.player = player;
    }
}
