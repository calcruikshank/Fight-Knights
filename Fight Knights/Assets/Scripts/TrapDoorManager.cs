using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDoorManager : MonoBehaviour
{
    TrapDoorBehaviour[] trapDoors;
    bool hasChosenDoorsToOpen = false;
    float timeBetween = 0;
    float timeAfterClosing = 0f;
    // Start is called before the first frame update
    void Start()
    {
        trapDoors = GetComponentsInChildren<TrapDoorBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasChosenDoorsToOpen)
        {
            timeBetween += Time.deltaTime;
            if (timeBetween > 6f)
            {
                CloseAllDoors();
            }
        }
        if (!hasChosenDoorsToOpen)
        {
            timeAfterClosing += Time.deltaTime;
            if (timeAfterClosing > 1f)
            {
                SelectRandomDoorsToOpen();
                timeAfterClosing = 0f;
            }
        }
        
    }

    void SelectRandomDoorsToOpen()
    {
        foreach (TrapDoorBehaviour trapDoor in trapDoors)
        {
            if (Random.Range(0, 100) > 50)
            {
                trapDoor.SetToBeOpen();
            }
            
        }
        hasChosenDoorsToOpen = true;
        timeBetween = 0f;
    }

    public void CloseAllDoors()
    {
        foreach (TrapDoorBehaviour trapDoor in trapDoors)
        {
            
             trapDoor.CloseTrapDoor();

        }

        timeAfterClosing = 0f;
        hasChosenDoorsToOpen = false;
        
    }
}
