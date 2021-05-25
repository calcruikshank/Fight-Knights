using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class ModeSelect : MonoBehaviour
{
    PlayerController player;
    [SerializeField] int sceneToSelect;
    SelectCharacter selectCharacter;
    CharacterChoice characterChoice;
    // Start is called before the first frame update

    
    void Start()
    {
        
    }

    private void Awake()
    {
        characterChoice = FindObjectOfType<CharacterChoice>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        player = other.transform.parent.GetComponent<PlayerController>();
        if (player != null)
        {
            SceneManager.LoadScene(sceneToSelect);
            ResetTransformAndSpawn();
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    void ResetTransformAndSpawn()
    {
        PlayerInput[] playersInScene = FindObjectsOfType<PlayerInput>();
            foreach (PlayerInput players in playersInScene)
            {
                players.transform.position = Vector3.zero;
                PlayerController playerController = players.gameObject.GetComponent<PlayerController>();
                
                characterChoice.Spawn(playerController);
            }
    }

    
}
