using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBall : MonoBehaviour
{
    float lifeTimer;
    float speed = 20f;
    PlayerController opponent;
    PlayerController player;
    [SerializeField] Transform arrow;
    Transform instantiatedArrow;
    bool connected = false;
    float throwAfterSecs = 0f;
    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        lifeTimer = 0f;
        this.gameObject.GetComponent<Rigidbody>().AddForce((player.transform.right) * (speed), ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer > 1f && !connected)
        {
            Destroy(this.gameObject);
        }

        if (connected)
        {
            instantiatedArrow.right = player.transform.right;
            instantiatedArrow.position = opponent.transform.position;
            this.transform.position = opponent.transform.position;
            throwAfterSecs += Time.deltaTime;
            if (throwAfterSecs >= .25f && opponent != null)
            {

                opponent.Throw(player.transform.right);
                Destroy(instantiatedArrow.gameObject);
                Destroy(this.gameObject);
            }
        }
    }
    public void SetPlayer(PlayerController playerSent)
    {
        player = playerSent;
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
                player.EndPunchRight();
                Destroy(this.gameObject);
                return;
            }
            else
            {
                connected = true;
                opponent.Stunned(1f, 0f);
                instantiatedArrow = Instantiate(arrow, new Vector3(opponent.transform.position.x, 0, opponent.transform.position.z), Quaternion.identity);
            }
            Vector3 punchTowards = new Vector3(player.transform.right.normalized.x, 0, player.transform.right.normalized.z);
            Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            this.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            this.transform.position = opponent.transform.position;
        }
    }
}
