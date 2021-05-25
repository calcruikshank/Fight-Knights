using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointBetweenPlayers : MonoBehaviour
{
    public PlayerInput[] players;
    Vector3 pointToFollow;
    public float furthestDistanceBetweenPlayer;
    public float[] numsToChooseFrom;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        players = FindObjectsOfType<PlayerInput>();
        if (players.Length == 1)
        {
            pointToFollow = players[0].transform.position;
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(pointToFollow.x, pointToFollow.y + 25, pointToFollow.z - 15), 50 * Time.deltaTime);
            return;
        }
        foreach (PlayerInput player in players)
        {
            pointToFollow += player.transform.position;
            if (players.Length - 1 != numsToChooseFrom.Length)
            {
                numsToChooseFrom = new float[players.Length - 1];
            }
        }
        
        for (int i = 0; i < players.Length - 1; i++)
        {
            float distanceBetweenPlayer = Vector3.Distance(players[i].transform.position, players[i + 1].transform.position);
            numsToChooseFrom[i] = distanceBetweenPlayer;
            furthestDistanceBetweenPlayer = distanceBetweenPlayer;
            for (int j = 0; j < numsToChooseFrom.Length; j++)
            {
                if (numsToChooseFrom[j] > furthestDistanceBetweenPlayer)
                {
                    furthestDistanceBetweenPlayer = numsToChooseFrom[j];
                }
            }

        }
        

        if (players.Length > 1)
        {
            pointToFollow = pointToFollow / (players.Length + 1);

        }


        this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(pointToFollow.x, pointToFollow.y + 25, pointToFollow.z - 15), 50 * Time.deltaTime);
    }
}
