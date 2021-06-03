using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SoccerCanvasBehaviour : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI redText, blueText; 
    // Start is called before the first frame update
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText(int red, int blue)
    {
        redText.text = red.ToString();
        blueText.text = blue.ToString();
    }
}
