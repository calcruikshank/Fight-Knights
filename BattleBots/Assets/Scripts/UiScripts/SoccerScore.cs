using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoccerScore : MonoBehaviour
{
    public static SoccerScore Instance { get; private set; }
    int redScore, blueScore;
    float time;
    int greaterScore;
    int teamThatWon;
    [SerializeField] GameObject RedScorePrefab, BlueScorePrefab, TimePrefab;
    bool finishedGame = false;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Singleton - Trying to create another instance of a singleton");
        }
        else
        {
            Instance = this;
        }
        finishedGame = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        redScore = 0;
        blueScore = 0;
        RedScorePrefab.GetComponent<TextMeshProUGUI>().text = redScore.ToString();
        SetTime(GameConfigurationManager.Instance.timeForGame);
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        
        TimePrefab.GetComponent<TextMeshProUGUI>().text = time.ToString("F0");
        if (time <= 0 && finishedGame == false)
        {
            finishedGame = true;
            GameConfigurationManager.Instance.LoadVictoryScene(GetWinningTeam());
        }
    }
    public void AddToRed()
    {
        redScore++;
        RedScorePrefab.GetComponent<TextMeshProUGUI>().text = redScore.ToString();
    }
    public void SetTime(float sentTime)
    {
        time = sentTime * 60;
    }

    public void AddToBlue()
    {
        blueScore++;
        BlueScorePrefab.GetComponent<TextMeshProUGUI>().text = blueScore.ToString();
    }

    public int GetWinningTeam()
    {
        if (blueScore > redScore)
        {
            greaterScore = blueScore;
            teamThatWon = 0;
        }
        if (redScore > blueScore)
        {
            greaterScore = blueScore;
            teamThatWon = 1;
        }
        return teamThatWon;
    }
}
