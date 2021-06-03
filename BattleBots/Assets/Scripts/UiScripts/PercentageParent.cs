using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PercentageParent : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    public PlayerInput[] players;
    public List<PlayerController> playerList = new List<PlayerController>();
    [SerializeField] GameObject playerPercentText;
    public GameObject percentText;
    // Start is called before the first frame update
    void Start()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //playerInputManager.onPlayerJoined += AddText;
    }


    public void AddPercentageText(PlayerController player)
    {
        percentText = Instantiate(playerPercentText);
        percentText.transform.parent = this.transform;
        percentText.GetComponent<PercentTextBehaviour>().SetPlayer(player);
        
    }

    public void RemovePercentageText()
    {
        Destroy(percentText);
    }

    void AddText(PlayerInput playerInputSent)
    {
        if (!playerList.Contains(playerInputSent.gameObject.GetComponent<PlayerController>()))
        {
            playerList.Add(playerInputSent.gameObject.GetComponent<PlayerController>());
            percentText = Instantiate(playerPercentText);
            percentText.transform.SetParent(this.transform);
            percentText.GetComponent<PercentTextBehaviour>().SetPlayer(playerInputSent.gameObject.GetComponent<PlayerController>());
        }
        
       
    }
}
