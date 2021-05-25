using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchRockCollider : MonoBehaviour
{
    RockLaunch rockLaunch;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        rockLaunch = other.transform.GetComponent<RockLaunch>();
        if (rockLaunch != null)
        {
            Debug.Log("HitRock");
            rockLaunch.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            rockLaunch.gameObject.GetComponent<Rigidbody>().AddForce((this.transform.parent.transform.parent.GetComponent<HandleCollider>().player.transform.right) * (40f), ForceMode.Impulse);
            Physics.IgnoreCollision(this.transform.GetComponent<Collider>(), other);
            
        }
    }
}
