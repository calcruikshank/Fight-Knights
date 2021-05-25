using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CursorGameSetup : MonoBehaviour
{
    Vector2 inputMovement;
    Vector3 movement, lastMoveDir;
    float moveSpeed = 35f;
    Button button;
    [SerializeField] GameObject multiEventSystem;
    GameObject myEventSystem;
    bool isReadied = false; //this is for player configs
    GameObject currentSelectedPrefab;
    int currentStageChoice;
    int currentColor;
    [SerializeField] public Color[] colors = new Color[6];
    private int PlayerIndex;
    bool isReady = false; //this is for cursor game setup only
    string thisControlScheme;
    bool moveGameModeRight = false; 
    bool hoveringOverGameMode = false;
    bool hoveringOverStageChoice = false;
    GameObject hoveredButton;
    GameObject previousSelectedButton;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.parent = FindObjectOfType<MainCanvas>().transform;
        this.transform.localScale = Vector3.one;
        myEventSystem = Instantiate(multiEventSystem);
        PlayerIndex = this.gameObject.GetComponent<PlayerInput>().playerIndex;
        currentColor = PlayerIndex;
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
            Debug.Log("gamepad ");

            SetDevice(Gamepad.current);
        }
        else
        {
            Debug.Log("keyboard ");
            SetDevice(Keyboard.current);
        }
    }
    void SetDevice(InputDevice currentDevice)
    {

        //PlayerConfigurationManager.Instance.SetDevice(PlayerIndex, currentDevice);


    }

    void SetStage(int stage)
    {
        Debug.Log("Setting Stage from cursor " + stage);
        GameConfigurationManager.Instance.SetStage(stage);
        StageChoiceButton[] stages2 = FindObjectsOfType<StageChoiceButton>();
        foreach (StageChoiceButton stageToDeselect in stages2)
        {
            if (stageToDeselect.isActiveAndEnabled)
            {
                previousSelectedButton = stageToDeselect.gameObject;
                if (previousSelectedButton != null)
                {
                    previousSelectedButton.GetComponent<StageChoiceButton>().SetInactive();
                }
            }
        }
        
        if (hoveredButton != null)
        {
            hoveredButton.GetComponent<StageChoiceButton>().SetEnabled();
        }

        previousSelectedButton = hoveredButton;
    }

    void SetGameMode(bool wayToMove)
    {
        GameConfigurationManager.Instance.SetGameMode(moveGameModeRight);
    }

    private void OnTriggerStay(Collider other)
    {
        button = other.gameObject.GetComponent<Button>();
        if (other.gameObject.GetComponent<StageChoiceButton>() != null)
        {
            currentStageChoice = other.gameObject.GetComponent<StageChoiceButton>().stageChoice;

            hoveredButton = other.gameObject;
            hoveringOverStageChoice = true;
        }
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(button.gameObject);
        if (other.gameObject.GetComponent<ReadyButton>() != null)
        {
            isReady = true;
        }
        if (other.gameObject.GetComponent<GameModeButton>() != null)
        {
            moveGameModeRight = other.gameObject.GetComponent<GameModeButton>().changeGameModeToTheRight;
            hoveringOverGameMode = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        currentStageChoice = -1;
        button = other.gameObject.GetComponent<Button>();
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        isReady = false;
        hoveredButton = null;
        hoveringOverGameMode = false;
        hoveringOverStageChoice = false; 
    }

    public void SetPlayerIndex(int pi)
    {
        PlayerIndex = pi;
    }

    public void SetTeam(int teamID)
    {
        

    }
    public void SetCharacterChoice(GameObject character)
    {
        PlayerConfigurationManager.Instance.SetPlayerPrefab(PlayerIndex, character);
    }
    public void SetColor(Color charColor)
    {

        this.gameObject.GetComponentInChildren<Image>().color = charColor;

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
        if (hoveringOverStageChoice && isReady == false)
        {
            SetStage(currentStageChoice);
        }
        if (isReady)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        if (hoveringOverGameMode)
        {
            SetGameMode(moveGameModeRight);
        }
        
    }


    void OnStartButton()
    {
        GameConfigurationManager.Instance.Pause();
    }

    void OnAltPunchRight()
    {
        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.isPaused) return;
        }
        UnReadyPlayer();
    }

    void OnShield()
    {
        if (GameConfigurationManager.Instance != null)
        {
            if (GameConfigurationManager.Instance.isPaused) return;
        }
        currentColor++;
        if (currentColor > 5)
        {
            currentColor = 0;
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
        if (currentColor < 0)
        {
            currentColor = 5;
        }
        SetColor(colors[currentColor]);
        SetTeam(currentColor);
    }
}
