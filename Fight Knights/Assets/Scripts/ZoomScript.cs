using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomScript : MonoBehaviour
{
    PointBetweenPlayers pointBetweenPlayers;
    // Start is called before the first frame update
    void Start()
    {
        pointBetweenPlayers = this.transform.parent.GetComponent<PointBetweenPlayers>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, new Vector3(0f, 0f, pointBetweenPlayers.furthestDistanceBetweenPlayer * 3 / -10f), 25f * Time.deltaTime);
        
    }
}
