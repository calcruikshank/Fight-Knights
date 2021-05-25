using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinballCollider : MonoBehaviour
{
    [SerializeField] float damage = 8f;
    PlayerController opponent;
    // Start is called before the first frame update
    void Start()
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
                return;
            }
            Vector3 knockTowards = new Vector3(opponent.transform.position.x - this.transform.parent.transform.parent.position.x, 0, opponent.transform.position.z - this.transform.parent.transform.parent.position.z).normalized;
            Debug.Log(opponent);
            opponent.Bounce(knockTowards);
        }

    }
    //Debug.Log("Knockback" + greatestDamage);
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
