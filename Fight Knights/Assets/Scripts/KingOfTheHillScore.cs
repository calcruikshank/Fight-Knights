using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingOfTheHillScore : MonoBehaviour
{
    public float kothScore;
    bool insideBounds;
    // Start is called before the first frame update
    void Start()
    {
        insideBounds = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (insideBounds)
        {
            kothScore += Time.deltaTime;
        }
    }

    public void SetInsideOfHillBounds()
    {
        insideBounds = true;
    }
}
