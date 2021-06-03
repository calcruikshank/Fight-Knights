using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StockTextBehaviour : MonoBehaviour
{
    PlayerController player;
    [SerializeField] TextMeshProUGUI textObject;
    [SerializeField] GameObject textObjectPrefab;
    [SerializeField] List<Color> colors = new List<Color>();

    PlayerTeams playerTeams;
    // Start is called before the first frame update
    void Awake()
    {
        playerTeams = FindObjectOfType<PlayerTeams>();

    }

    private void Start()
    {
        this.textObject.color = player.gameObject.GetComponent<TeamID>().teamColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            textObject.text = player.stocks.ToString();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetPlayer(PlayerController playerSent)
    {
        player = playerSent;
    }
}
