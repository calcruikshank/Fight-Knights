using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterChoice : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Spawn(PlayerController playerSent)
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        PlayerInput pi = playerSent.gameObject.GetComponent<PlayerInput>();
        playerSent.Awake();
        DontDestroyOnLoad(pi.gameObject);
        

    }
}
