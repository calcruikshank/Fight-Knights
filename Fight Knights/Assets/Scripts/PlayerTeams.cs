using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTeams : MonoBehaviour
{
    public bool teamsIsOn = false;
    PlayerInputManager playerInputManager;
    TeamID teamID;
    [SerializeField] public List<Material> mats = new List<Material>();
    // Start is called before the first frame update
    void Start()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInputManager.onPlayerJoined += SetPlayerTeam;
    }
    void SetPlayerTeam(PlayerInput playerInputSent)
    {
        if (!teamsIsOn)
        {
            if (playerInputSent.gameObject.GetComponentInChildren<TeamID>() == null)
            {
                teamID = playerInputSent.gameObject.AddComponent<TeamID>();
                teamID.team = playerInputSent.playerIndex;
                teamID.SetColor(mats[playerInputSent.playerIndex]);
                
            }
        }
        if (teamsIsOn)
        {
            if (playerInputSent.gameObject.GetComponentInChildren<TeamID>() == null)
            {
                teamID = playerInputSent.gameObject.AddComponent<TeamID>();
                teamID.team = playerInputSent.playerIndex % 2;
                teamID.SetColor(mats[playerInputSent.playerIndex % 2]);
            }
        }
        
    }

    public void ToggleTeamsOn()
    {
        teamsIsOn = true;
        PlayerInput[] players = FindObjectsOfType<PlayerInput>();

        foreach(PlayerInput player in players){
            if (player.gameObject.GetComponent<TeamID>() != null)
            {
                teamID = player.gameObject.GetComponent<TeamID>();
                teamID.team = player.playerIndex % 2;
                teamID.SetColor(mats[player.playerIndex % 2]);
            }
            
        }
    }

    public void SetTeam(int teamToAssign, PlayerController player)
    {
        teamID = player.gameObject.GetComponent<TeamID>();
        teamID.SetColor(mats[teamToAssign]);
    }

    public void ToggleTeamsOff()
    {
        teamsIsOn = false;
        PlayerInput[] players = FindObjectsOfType<PlayerInput>();

        foreach (PlayerInput player in players)
        {
            teamID = player.gameObject.GetComponent<TeamID>();
            teamID.team = player.playerIndex;
            teamID.SetColor(mats[player.playerIndex]);
        }
    }

}
