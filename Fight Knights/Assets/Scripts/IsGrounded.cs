using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGrounded : MonoBehaviour
{
    Floor floor;
    public bool isGrounded = false;
    Collider boxyCollider;
    float hasBeenLongEnoughToNotBeJumping;
    // Start is called before the first frame update
    void Start()
    {
        isGrounded = true;
        boxyCollider = this.transform.GetComponentInChildren<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hitColliders = Physics.OverlapBox(boxyCollider.bounds.center, boxyCollider.bounds.size / 2, Quaternion.identity);
        hasBeenLongEnoughToNotBeJumping += Time.deltaTime;
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].transform.GetComponent<Floor>() != null && hasBeenLongEnoughToNotBeJumping > .1f)
            {
                isGrounded = true;
                hasBeenLongEnoughToNotBeJumping = 0;
            }
        }
        
    }

    /*private void OnTriggerEnter(Collider other)
    {
        floor = other.transform.GetComponent<Floor>();
        if (floor != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }*/
    private void OnTriggerExit(Collider other)
    {
        floor = other.GetComponent<Floor>();
        if (floor != null)
        {
            isGrounded = false;
        }
        
    }
}
