using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KOTHTextBehaviour : MonoBehaviour
{

    PlayerController player;
    [SerializeField] TextMeshProUGUI textObject;
    KingOfTheHillScore kothScore;
    // Start is called before the first frame update
    void Start()
    {

        textObject = this.transform.GetComponentInChildren<TextMeshProUGUI>();
        this.textObject.color = player.gameObject.GetComponent<TeamID>().teamColor;
        kothScore = player.gameObject.AddComponent<KingOfTheHillScore>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayer(PlayerController player)
    {
        this.player = player;
    }
}
