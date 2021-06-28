using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeClone : MonoBehaviour
{
    PlayerController player;
    [SerializeField] GameObject explosionPrefab;
    GameObject explosion;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExplodeTheClone()
    {
        GameObject explosion = Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
        HandleCollider handleCollider = explosion.GetComponent<HandleCollider>();
        handleCollider.SetPlayer(this.player, this.transform);
        Destroy(this.gameObject);
    }

    public void SetPlayer(PlayerController playersent)
    {
        player = playersent;
    }
}
