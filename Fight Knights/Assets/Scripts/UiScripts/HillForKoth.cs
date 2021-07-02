using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillForKoth : MonoBehaviour
{
    KingOfTheHillScore koth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        koth = other.transform.parent.GetComponent<KingOfTheHillScore>();
        if (koth != null)
        {
            koth.SetInsideOfHillBounds();
        }
    }
}
