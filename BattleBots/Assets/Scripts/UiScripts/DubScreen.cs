using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DubScreen : MonoBehaviour
{
    [SerializeField] GameObject teamThatWonTextPrefab;
    [SerializeField] GameObject PlayerThatWonCardPrefab;
    float reloadSceneTimer;
    // Start is called before the first frame update
    void Start()
    {
        DisplayWhoWon();
        SpawnVictoryCards();
        reloadSceneTimer = 0f;
        
    }

    // Update is called once per frame
    void Update()
    {
        reloadSceneTimer += Time.deltaTime;
        if (reloadSceneTimer > 3f)
        {
            if (GameConfigurationManager.Instance != null)
            {
                GameConfigurationManager.Instance.ResetToGameModeSelect();
            }
        }
    }

    void DisplayWhoWon()
    {
        if (GameConfigurationManager.Instance.indexOfRemainingTeam == 0)
        {
            teamThatWonTextPrefab.gameObject.GetComponent<TextMeshProUGUI>().text = "Blue Team Won";
        }
        if (GameConfigurationManager.Instance.indexOfRemainingTeam == 1)
        {
            teamThatWonTextPrefab.gameObject.GetComponent<TextMeshProUGUI>().text = "Red Team Won";
        }
        if (GameConfigurationManager.Instance.indexOfRemainingTeam == 2)
        {
            teamThatWonTextPrefab.gameObject.GetComponent<TextMeshProUGUI>().text = "Yellow Team Won";
        }
        if (GameConfigurationManager.Instance.indexOfRemainingTeam == 3)
        {
            teamThatWonTextPrefab.gameObject.GetComponent<TextMeshProUGUI>().text = "Green Team Won";
        }
        if (GameConfigurationManager.Instance.indexOfRemainingTeam == 4)
        {
            teamThatWonTextPrefab.gameObject.GetComponent<TextMeshProUGUI>().text = "White Team Won";
        }
        if (GameConfigurationManager.Instance.indexOfRemainingTeam == 5)
        {
            teamThatWonTextPrefab.gameObject.GetComponent<TextMeshProUGUI>().text = "Black Team Won";
        }
    }

    void SpawnVictoryCards()
    {
        Instantiate(PlayerThatWonCardPrefab, this.transform);
    }
}
