using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamID : MonoBehaviour
{
    public int team;
    GameObject playerRing;
    public Color teamColor;
    // Start is called before the first frame update
    void Awake()
    {
        team = -1;
        playerRing = this.transform.GetComponentInChildren<PlayerRing>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Material color)
    {
        playerRing.GetComponent<MeshRenderer>().material = color;
    }
    public void SetColorOnMat(Color color)
    {
        teamColor = color;
        playerRing.GetComponent<MeshRenderer>().material.color = color;
    }

    public void SetTeamID(int sentTeam)
    {
        team = sentTeam;
    }
}
