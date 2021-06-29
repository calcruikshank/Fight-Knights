using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDoorBehaviour : MonoBehaviour
{
    Transform leftDoor, rightDoor, leftTargetDoor, rightTargetDoor;
    Quaternion rightClosedPosition, leftClosedPosition;
    MeshCollider meshCollider;
    bool toBeOpened;
    float toBeOpenedTimer;
    Material originalMaterial;
    [SerializeField]Material toBeOpenedMaterial;
    // Start is called before the first frame update
    [SerializeField]bool open = false;
    void Start()
    {
        leftDoor = transform.GetChild(0);
        rightDoor = transform.GetChild(1);
        leftTargetDoor = transform.GetChild(2);
        rightTargetDoor = transform.GetChild(3);
        leftClosedPosition = leftDoor.rotation;
        rightClosedPosition = rightDoor.rotation;
        meshCollider = GetComponent<MeshCollider>();
        toBeOpenedTimer = 0f;
        originalMaterial = leftDoor.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (open)
        {
            leftDoor.rotation = Quaternion.RotateTowards(leftDoor.rotation, leftTargetDoor.rotation, 1000 * Time.deltaTime);
            rightDoor.rotation = Quaternion.RotateTowards(rightDoor.rotation, rightTargetDoor.rotation, 1000 * Time.deltaTime);
            meshCollider.enabled = false;
        }
        if (!open) 
        {
            leftDoor.rotation = Quaternion.RotateTowards(leftDoor.rotation, leftClosedPosition, 1000 * Time.deltaTime);
            rightDoor.rotation = Quaternion.RotateTowards(rightDoor.rotation, rightClosedPosition, 1000 * Time.deltaTime);
            meshCollider.enabled = true;

            leftDoor.GetComponent<MeshRenderer>().material = originalMaterial;
            rightDoor.GetComponent<MeshRenderer>().material = originalMaterial;
        }
        if (toBeOpened)
        {
            
            toBeOpenedTimer += Time.deltaTime;
            if (toBeOpenedTimer < .25f)
            {
                leftDoor.GetComponent<MeshRenderer>().material = toBeOpenedMaterial;
                rightDoor.GetComponent<MeshRenderer>().material = toBeOpenedMaterial;
                return;
            }
            if (toBeOpenedTimer > .25f && toBeOpenedTimer < .5f)
            {
                leftDoor.GetComponent<MeshRenderer>().material = originalMaterial;
                rightDoor.GetComponent<MeshRenderer>().material = originalMaterial;
            }
            if (toBeOpenedTimer > .5f && toBeOpenedTimer < .75f)
            {
                leftDoor.GetComponent<MeshRenderer>().material = toBeOpenedMaterial;
                rightDoor.GetComponent<MeshRenderer>().material = toBeOpenedMaterial;
            }
            if (toBeOpenedTimer > .75f && toBeOpenedTimer < 1f)
            {
                leftDoor.GetComponent<MeshRenderer>().material = originalMaterial;
                rightDoor.GetComponent<MeshRenderer>().material = originalMaterial;
            }
            if (toBeOpenedTimer > 1f && toBeOpenedTimer < 1.25f)
            {
                leftDoor.GetComponent<MeshRenderer>().material = toBeOpenedMaterial;
                rightDoor.GetComponent<MeshRenderer>().material = toBeOpenedMaterial;
            }
            if (toBeOpenedTimer > 1.25f)
            {
                leftDoor.GetComponent<MeshRenderer>().material = originalMaterial;
                rightDoor.GetComponent<MeshRenderer>().material = originalMaterial;
            }
            if (toBeOpenedTimer > 3f)
            {
                toBeOpened = false;
                OpenTrapDoor();
            }
        }
    }

    public void OpenTrapDoor()
    {
        open = true;
    }

    public void CloseTrapDoor()
    {
        toBeOpenedTimer = 0f;
        open = false;
        toBeOpened = false;
    }

    public void SetToBeOpen()
    {
        toBeOpened = true;
    }
   
}
