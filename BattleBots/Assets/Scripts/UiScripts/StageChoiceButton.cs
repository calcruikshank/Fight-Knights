using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageChoiceButton : MonoBehaviour
{
    public int stageChoice;
    public GameObject selectedText;
    public GameObject selectedIndicator;
    public bool selectedOnStart = false;
    public bool previousSelected = false;
    // Start is called before the first frame update
    void Start()
    {
        
        SetInactive();
        if (selectedOnStart)
        {
            SetEnabled();
            GameConfigurationManager.Instance.SetStage(this.stageChoice);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEnabled()
    {
        selectedText.SetActive(true);
        selectedIndicator.SetActive(true);
        previousSelected = true;
    }

    public void SetInactive()
    {
        selectedText.SetActive(false);
        selectedIndicator.SetActive(false);
        previousSelected = false;
    }
}
