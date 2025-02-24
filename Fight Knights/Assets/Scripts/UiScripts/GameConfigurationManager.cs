using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameConfigurationManager : MonoBehaviour
{
    public static GameConfigurationManager Instance { get; private set; }
    public int stage, gameMode, numOfStocks;
    [SerializeField] GameObject[] gameModes;
    public float timeForGame = 4;
    public float timeToWinKoth = 60;
    public int[] numOfPlayersInEachTeam = new int[6];
    public int teamsRemaining = 0;
    public int indexOfRemainingTeam = 0;
    public RectTransform damageCanvas;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject ControlsMenu;
    [SerializeField] Button resumeButton;
    [SerializeField] EventSystem pauseMenuEventSystem;
    [SerializeField] TextMeshProUGUI stocksText;
    [SerializeField] GameObject DamagePopupText;
    [SerializeField] GameObject LeftStockButton;
    [SerializeField] GameObject LeftArrowButton;
    [SerializeField] GameObject RightArrowButton;
    [SerializeField] GameObject RightStockButton;
    public bool isPaused = false;

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        SetStocks(4);
        SetTimeToWinKoth(60);
        if (Instance != null)
        {
            Debug.Log("Singleton - Trying to create another instance of a singleton");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }
    
    public void SetStocks(int sentStocks)
    {
        numOfStocks = sentStocks;
    }

    public void ChangeStocks(bool wayToChange)
    {
        if (wayToChange == false && numOfStocks > 1)
        {
            numOfStocks--;

        }
        if (wayToChange == true)
        {
            numOfStocks++;
        }
        stocksText.text = numOfStocks.ToString();
    }
    public void SetTimeToWinKoth(float sentTime)
    {
        timeToWinKoth = sentTime;
    }
    public void SetStage(int sentStage)
    {
        Debug.Log(sentStage + "Sent Stage");
        stage = sentStage;
    }
    public void SetGameMode(bool sentGameMode)
    {
        if (sentGameMode == false)
        {
            gameMode--;
            
        }
        if (sentGameMode == true)
        {
            gameMode++;
        }
        if (gameMode < 0)
        {
            gameMode = gameModes.Length - 1;
        }
        if (gameMode > gameModes.Length - 1)
        {
            gameMode = 0;
        }
        if (gameMode == 0)
        {
            LeftStockButton.SetActive(true);
            RightStockButton.SetActive(true);

            LeftArrowButton.SetActive(true);
            RightArrowButton.SetActive(true);
        }
        else
        {

            LeftStockButton.SetActive(false);
            RightStockButton.SetActive(false);

        }
        SetActivePanel(gameMode);
    }

    public void SetActivePanel(int gameModeSent)
    {
        StageChoiceButton[] stages = FindObjectsOfType<StageChoiceButton>();
        foreach (StageChoiceButton stageToDeselect in stages)
        {
            stageToDeselect.selectedIndicator.SetActive(false);
            stageToDeselect.selectedText.SetActive(false);
        }
        Debug.Log(gameModeSent);
        foreach (GameObject mode in gameModes)
        {
            mode.SetActive(false);
        }
        gameModes[gameModeSent].SetActive(true);
        StageChoiceButton[] stages2 = FindObjectsOfType<StageChoiceButton>();
        foreach (StageChoiceButton stageToDeselect in stages2)
        {
            if (stageToDeselect.selectedOnStart)
            {
                stageToDeselect.selectedIndicator.SetActive(true);
                stageToDeselect.selectedText.SetActive(true);
                SetStage(stageToDeselect.stageChoice);
            }
        }
    }

    public void LoadVictoryScene(int winningTeam)
    {
        Debug.Log(winningTeam + " Team Won");

        StartCoroutine(WaitTime(1f));
        
    }
    private IEnumerator WaitTime(float freezeTime)
    {
        yield return new WaitForSecondsRealtime(freezeTime);
        SceneManager.LoadScene("Victory Screen");
        
    }
    public void AddPlayerToTeamArray(int index)
    {
        numOfPlayersInEachTeam[index]++;
    }
    public void RemovePlayerFromTeamArray(int index)
    {
        numOfPlayersInEachTeam[index]--;
    }

    public void CheckIfWon()
    {
        teamsRemaining = 0;
        for (int i = 0; i < numOfPlayersInEachTeam.Length; i++)
        {
            if (numOfPlayersInEachTeam[i] > 0)
            {
                teamsRemaining++;
                indexOfRemainingTeam = i;
            }
        }
        if (teamsRemaining <= 1)
        {
            LoadVictoryScene(indexOfRemainingTeam);
        }
    }

    public void DisplayDamageText(int damage, Transform playerTakingDamage, PlayerController playerSent)
    {
        if (FindObjectOfType<DamageCanvas>() != null)
        {
            Debug.Log("Damage Text");
            GameObject txt = Instantiate(DamagePopupText, new Vector3(playerTakingDamage.transform.position.x, playerTakingDamage.transform.position.y + 3, playerTakingDamage.transform.position.z + 3), FindObjectOfType<DamageCanvas>().transform.rotation, FindObjectOfType<DamageCanvas>().transform);
            txt.GetComponent<MoveTextBehaviour>().Setup(damage);
            
                //txt.GetComponent<TextMeshProUGUI>().color = playerTakingDamage.gameObject.GetComponent<TeamID>().teamColor;
            
        }
        
    }

    public void ResetToGameModeSelect()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            playerConfigs[i].IsReady = false;
        }
        DontDestroyOnLoad[] ddols = FindObjectsOfType<DontDestroyOnLoad>();
        foreach (DontDestroyOnLoad ddol in ddols)
        {
            Destroy(ddol.gameObject);
        }
        FindObjectOfType<PlayerInputManager>().joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        Destroy(GameConfigurationManager.Instance.gameObject);
        Destroy(PlayerConfigurationManager.Instance.gameObject);


        SceneManager.LoadScene(0);
    }


    public void Pause()
    {
        ControlsMenu.SetActive(false);
        PauseMenu.SetActive(!PauseMenu.activeSelf);
        if (PauseMenu.activeSelf)
        {
            resumeButton.Select();
            isPaused = true;
            Time.timeScale = 0;
        }
        else
        {
            isPaused = false;

            Time.timeScale = 1;
        }
        
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
