using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CursorBehaviour : MonoBehaviour
{
    Vector2 inputMovement;
    Vector3 movement, lastMoveDir;
    float moveSpeed = 35f;
    Button button;
    [SerializeField] GameObject multiEventSystem, playerInfo;
    GameObject myEventSystem;
    bool isReadied = false;
    GameObject currentSelectedPrefab, playerInfoInstantiated;
    int currentColor;
    Transform playerInfoParent;
    [SerializeField] public Color[] colors = new Color[6];
    private int PlayerIndex;
    string thisControlScheme;
    bool backButtonSelected;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {

        playerInfoInstantiated = Instantiate(playerInfo, Vector3.zero, Quaternion.identity);
        this.transform.parent = FindObjectOfType<MainCanvas>().transform;
        this.transform.localScale = Vector3.one;
        myEventSystem = Instantiate(multiEventSystem);
        PlayerIndex = this.gameObject.GetComponent<PlayerInput>().playerIndex;
        currentColor = PlayerIndex;
        if (GameConfigurationManager.Instance.gameMode != 0)
        {

            SetColor(colors[currentColor % 2]);
            Debug.Log("game mode isnt classic");
            SetTeam(PlayerIndex % 2);
        }
        else
        {

            SetColor(colors[currentColor]);
            SetTeam(PlayerIndex);
        }
        playerInfoParent = FindObjectOfType<PlayerInfoParent>().transform;
        
        playerInfoInstantiated.transform.parent = playerInfoParent;
        playerInfoInstantiated.transform.localPosition = Vector3.zero;
        thisControlScheme = this.gameObject.GetComponent<PlayerInput>().currentControlScheme;
        SetControlScheme(thisControlScheme);
        backButtonSelected = false;


        rb = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = inputMovement.x;
        movement.y = inputMovement.y;
        if (movement.x != 0 || movement.z != 0)
        {
            lastMoveDir = movement;
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
    }

    private void FixedUpdate()
    {
        if (!isReadied)
        {
            rb.velocity = (movement * moveSpeed);
        }
    }
    

    void SetControlScheme(string controlScheme)
    {
        PlayerConfigurationManager.Instance.SetControlScheme(PlayerIndex, controlScheme);
        if (controlScheme == "Gamepad")
        {
            SetDevice(this.gameObject.GetComponent<PlayerInput>().devices[0]);
            
        }
        else
        {
            SetDevice(Keyboard.current);
        }
    }
    void SetDevice(InputDevice currentDevice)
    {

        PlayerConfigurationManager.Instance.SetDevice(PlayerIndex, currentDevice);
        
        
    }
    
    private void OnTriggerStay(Collider other)
    {
        button = other.gameObject.GetComponent<Button>();
        if (other.gameObject.GetComponent<CharChoiceButton>() != null)
        {

            currentSelectedPrefab = other.gameObject.GetComponent<CharChoiceButton>().prefabToSpawn;

            playerInfoInstantiated.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = other.gameObject.GetComponent<CharChoiceButton>().charName;
        }
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(button.gameObject);
        if (other.gameObject.GetComponent<ResetButton>() != null)
        {
            backButtonSelected = true;
        }
        if (other.gameObject.GetComponent<ResetButton>() == null)
        {
            backButtonSelected = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        currentSelectedPrefab = null;
        button = other.gameObject.GetComponent<Button>();
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        playerInfoInstantiated.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = " ";
        backButtonSelected = false;
    }

    public void SetPlayerIndex(int pi)
    {
        PlayerIndex = pi;
    }

    public void SetTeam(int teamID)
    {
        PlayerConfigurationManager.Instance.SetPlayerTeam(PlayerIndex, teamID);
        playerInfoInstantiated.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Player " + (PlayerIndex + 1);
        if (teamID == 0)
        {
            playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Blue Team";
        }
        if (teamID == 1)
        {
            playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Red Team";
        }
        if (teamID == 2)
        {
            playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Yellow Team";
        }
        if (teamID == 3)
        {
            playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Green Team";
        }
        if (teamID == 4)
        {
            playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "White Team";
        }
        if (teamID == 5)
        {
            playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Black Team";
        }

    }
    public void SetCharacterChoice(GameObject character)
    {
        PlayerConfigurationManager.Instance.SetPlayerPrefab(PlayerIndex, character);
    }
    public void SetColor(Color charColor)
    {

        this.gameObject.GetComponentInChildren<Image>().color = charColor;
        PlayerConfigurationManager.Instance.SetPlayerColor(PlayerIndex, charColor);
        playerInfoInstantiated.transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = charColor;
        playerInfoInstantiated.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = charColor;
        
    }

    

    public void ReadyPlayer()
    {
        //call this when you press a over a button
        PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex);
        isReadied = true;
    }
    public void UnReadyPlayer()
    {
        //call this when you press b
        PlayerConfigurationManager.Instance.UnReadyPlayer(PlayerIndex);
        isReadied = false;
    }



    void OnMove(InputValue value)
    {
        
        inputMovement = value.Get<Vector2>();
    }

    void OnAButtonDown()
    {
        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.isPaused) return;
        }
        if (currentSelectedPrefab != null)
        {
            SetCharacterChoice(currentSelectedPrefab);
            ReadyPlayer();
        }
        if (backButtonSelected)
        {
            GameConfigurationManager.Instance.ResetToGameModeSelect();
        }
    }

    void OnAltPunchRight()
    {
        UnReadyPlayer();
    }
    
    void OnShield()
    {
        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.isPaused) return;
        }
        currentColor++;
        if (GameConfigurationManager.Instance.gameMode == 0)
        {
            if (currentColor > 5)
            {
                currentColor = 0;
            }
        }
        if (GameConfigurationManager.Instance.gameMode == 1 || GameConfigurationManager.Instance.gameMode == 2)
        {
            if (currentColor > 1)
            {
                currentColor = 0;
            }
        }

        SetColor(colors[currentColor]);
        SetTeam(currentColor);
    }
    void OnLeftBumper()
    {
        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.isPaused) return;
        }
        currentColor--;
        if (GameConfigurationManager.Instance.gameMode == 0 || GameConfigurationManager.Instance.gameMode == 2)
        {
            if (currentColor < 0)
            {
                currentColor = 5;
            }
        }
        if (GameConfigurationManager.Instance.gameMode == 1)
        {
            if (currentColor < 0)
            {
                currentColor = 1;
            }
        }
        SetColor(colors[currentColor]);
        SetTeam(currentColor);
    }

    void OnStartButton()
    {
        GameConfigurationManager.Instance.Pause();
    }
}
