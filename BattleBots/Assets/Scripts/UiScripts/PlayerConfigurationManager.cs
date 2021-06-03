using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;
    private int maxPlayers = 6;
    [SerializeField] Transform canvasInScene;
    public static PlayerConfigurationManager Instance { get; private set; }
    public PlayerInputManager pim;
    int stage, gameMode = 0;
    private void Awake()
    {
        
        if (Instance != null)
        {
            Debug.Log("Singleton - Trying to create another instance of a singleton");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
            pim = this.gameObject.GetComponent<PlayerInputManager>();
        }
    }
    

    public void SetPlayerPrefab(int index, GameObject characterChoice)
    {
        playerConfigs[index].PlayerPrefab = characterChoice;
    }
    public void SetPlayerColor(int index, Color charColor)
    {
        playerConfigs[index].PlayerColor = charColor;
    }
    public void SetPlayerTeam(int index, int playerTeam)
    {
        playerConfigs[index].PlayerTeam = playerTeam;
    }

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = true;

        if (playerConfigs.Count >= 2 && playerConfigs.All(p => p.IsReady == true))
        {
            pim.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
            SceneManager.LoadScene(GameConfigurationManager.Instance.stage);
        }
    }

    public void SetStage(int sentStage)
    {
        Debug.Log(sentStage);
        stage = sentStage;
    }
    public void SetGameMode(int sentGameMode)
    {
        gameMode = sentGameMode;
    }
    public void SetControlScheme(int index, string thisControlScheme)
    {
        playerConfigs[index].ControlScheme = thisControlScheme;
    }
    public void SetDevice(int index, InputDevice current)
    {
        
        playerConfigs[index].CurrentDevice = current;
        Debug.Log("setting currrent device " + current);
    }

    public void UnReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = false;
        playerConfigs[index].PlayerPrefab = null;
    }

    
    public void HandlePlayerJoin(PlayerInput pi)
    {

        if (!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.parent = (canvasInScene);
            pi.transform.localScale = Vector3.one;
            playerConfigs.Add(new PlayerConfiguration(pi));
        }
    }


    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }
}


public class PlayerConfiguration
{

    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }
    public PlayerInput Input { get; set; }
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    
    public GameObject PlayerPrefab { get; set; }

    public Color PlayerColor { get; set; }
    public int PlayerTeam { get; set; }

    public string ControlScheme { get; set; }
    public InputDevice CurrentDevice { get; set; }
}
