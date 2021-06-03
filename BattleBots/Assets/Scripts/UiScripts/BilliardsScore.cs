using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BilliardsScore : MonoBehaviour
{
    public static BilliardsScore Instance { get; private set; }
    int redScore, blueScore;
    int greaterScore;
    int teamThatWon;
    [SerializeField] GameObject RedScorePrefab, BlueScorePrefab;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddToRed()
    {
        redScore++;
        RedScorePrefab.GetComponent<TextMeshProUGUI>().text = redScore.ToString() + "/5";
        if (redScore >= 5)
        {
            GameConfigurationManager.Instance.LoadVictoryScene(1);
        }
    }
    

    public void AddToBlue()
    {
        blueScore++;
        BlueScorePrefab.GetComponent<TextMeshProUGUI>().text = blueScore.ToString() + "/5";
        if (blueScore >= 5)
        {
            GameConfigurationManager.Instance.LoadVictoryScene(0);
        }
    }
}
