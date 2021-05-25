using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTeam : PlayerController
{
    TeamID teamID;
    [SerializeField] int teamToAssign = -1;
    PlayerTeams playerTeams;
    void Start()
    {
        playerTeams = FindObjectOfType<PlayerTeams>();
    }
    // Update is called once per frame
    protected override void Update()
    {
        
    }
    public override void Knockback(float damage, Vector3 direction, PlayerController playerSent)
    {
        if (teamToAssign != -1)
        {
            playerTeams.ToggleTeamsOn();

            teamID = playerSent.gameObject.GetComponent<TeamID>();
            teamID.team = teamToAssign;
            playerTeams.SetTeam(teamToAssign, playerSent);
        }
        if (teamToAssign <= -1)
        {
            playerTeams.ToggleTeamsOff();

            
        }

    }

    protected override void Look()
    {

    }
    
}
