using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public PlayerController player;
    public SoccerBall soccerBall;
    [SerializeField] int goalColor = -1; //if goal color is -1 its a neutral goal

    
    void OnTriggerEnter(Collider other)
    {
        soccerBall = other.transform.parent.GetComponent<SoccerBall>();
        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.gameMode == 1 && soccerBall != null)
            {
                if (goalColor == 0 && soccerBall.canBeScored)
                {
                    SoccerScore.Instance.AddToBlue();

                }
                if (goalColor == 1 && soccerBall.canBeScored)
                {


                    SoccerScore.Instance.AddToRed();
                }
                soccerBall.LoseStock();
                return;
            }
            if (GameConfigurationManager.Instance.gameMode == 0 || GameConfigurationManager.Instance.gameMode == 2)
            {
                player = other.transform.parent.GetComponent<PlayerController>();
                if (player != null)
                {
                    if (!player.CheckIfColliderIsShield(other))
                    {
                        player.lostStock = true;
                        player.LoseStock();
                    }
                }
            }
            if (GameConfigurationManager.Instance.gameMode == 2 && soccerBall != null)
            {
                if (soccerBall.billiardBallColor == 0)
                {
                    BilliardsScore.Instance.AddToBlue();

                }
                if (soccerBall.billiardBallColor == 1)
                {


                    BilliardsScore.Instance.AddToRed();
                }
            }
           
        }
        else
        {
            NetworkPlayerController playerNetwork = other.transform.parent.GetComponent<NetworkPlayerController>();
            if (playerNetwork != null)
            {
                if (!playerNetwork.CheckIfColliderIsShield(other))
                {
                    playerNetwork.lostStock = true;
                    playerNetwork.LoseStock();
                }
            }
        }
        

        
        
        
    }
}
