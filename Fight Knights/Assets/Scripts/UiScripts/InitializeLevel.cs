using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InitializeLevel : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform[] playerSpawns;
    GameObject percentText;
    GameObject stockText;
    [SerializeField] GameObject percentTextPrefab;
    [SerializeField] GameObject stockTextPrefab;
    [SerializeField] GameObject soccerScorePrefab;
    [SerializeField] GameObject billiardsScorePrefab;
    [SerializeField] GameObject canvasMain;
    GameObject soccerScore;
    GameObject billiardsScore;
    public int gameMode = 0;
    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var player = PlayerInput.Instantiate(playerConfigs[i].PlayerPrefab, playerConfigs[i].PlayerIndex, playerConfigs[i].ControlScheme, -1, playerConfigs[i].CurrentDevice);

            player.gameObject.AddComponent<TeamID>().enabled = true;
            player.GetComponent<TeamID>().SetColorOnMat(playerConfigs[i].PlayerColor);
            player.GetComponent<TeamID>().SetTeamID(playerConfigs[i].PlayerTeam);
            GameConfigurationManager.Instance.AddPlayerToTeamArray(playerConfigs[i].PlayerTeam);
            if (GameConfigurationManager.Instance.gameMode == 0 || GameConfigurationManager.Instance.gameMode == 2) LoadClassicPlayer(player);
            if (GameConfigurationManager.Instance.gameMode == 1) LoadSoccerPlayer(player);
            if (GameConfigurationManager.Instance.gameMode == 3) LoadKingOfTheHill(player);
            if (playerSpawns[i] != null)
            {
                player.transform.position = playerSpawns[i].position;
                player.transform.rotation = playerSpawns[i].rotation;
            }
        }
        if (GameConfigurationManager.Instance.gameMode == 0) LoadClassic();
        if (GameConfigurationManager.Instance.gameMode == 1) LoadSoccer();
        if (GameConfigurationManager.Instance.gameMode == 2) LoadClassic();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadClassicPlayer(PlayerInput player)
    {

        percentText = Instantiate(percentTextPrefab);
        percentText.gameObject.GetComponent<PercentTextBehaviour>().SetPlayer(player.gameObject.GetComponent<PlayerController>());
        percentText.transform.parent = FindObjectOfType<PercentageParent>().transform;
        player.gameObject.GetComponent<PlayerController>().stocks = GameConfigurationManager.Instance.numOfStocks;
        stockText = Instantiate(stockTextPrefab);
        stockText.gameObject.GetComponent<StockTextBehaviour>().SetPlayer(player.gameObject.GetComponent<PlayerController>());
        stockText.transform.parent = percentText.transform;
        stockText.transform.localPosition = new Vector3(0f, 100f, 0f);
    }
    void LoadSoccerPlayer(PlayerInput player)
    {
        percentText = Instantiate(percentTextPrefab);
        percentText.gameObject.GetComponent<PercentTextBehaviour>().SetPlayer(player.gameObject.GetComponent<PlayerController>());
        percentText.transform.parent = FindObjectOfType<PercentageParent>().transform;
    }
    void LoadClassic()
    {

        Debug.Log("Loading Classic");
    }

    void LoadSoccer()
    {
        soccerScore = Instantiate(soccerScorePrefab);
        soccerScore.transform.parent = canvasMain.transform;
        soccerScore.transform.localPosition = new Vector3(0, 426, 0);

        Debug.Log("Loading soccer");
    }

    void LoadBilliards()
    {
        billiardsScore = Instantiate(billiardsScorePrefab);
        billiardsScore.transform.parent = canvasMain.transform;
        billiardsScore.transform.localPosition = new Vector3(0, 426, 0);
    }

    void LoadKingOfTheHill(PlayerInput player)
    {

        percentText = Instantiate(percentTextPrefab);
        percentText.gameObject.GetComponent<PercentTextBehaviour>().SetPlayer(player.gameObject.GetComponent<PlayerController>());
        percentText.transform.parent = FindObjectOfType<PercentageParent>().transform;
    }
}
