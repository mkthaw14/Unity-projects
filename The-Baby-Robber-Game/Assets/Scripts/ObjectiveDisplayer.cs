using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveDisplayer : MonoBehaviour
{
    [SerializeField]
    GameManager.GameMode gameMode;
    [SerializeField]
    Text[] objectiveTexts;

    // Start is called before the first frame update
    void Start()
    {
        if(gameMode != GameManager.instance.gameMode)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameMode == GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan)
        {
            objectiveTexts[0].text = HigherKillCount();
            objectiveTexts[1].text = HasLittleBoy();
        }
        else if(gameMode == GameManager.GameMode.CatchBabyAndDefendYourself)
        {
            objectiveTexts[0].text = HasLittleBoy();
        }
    }

    string HigherKillCount()
    {
        string s = "";

        if (GameManager.instance.KillCountNotEnough())
            s = "No";
        else
            s = "Yes";
        return s;
    }
    
    string HasLittleBoy()
    {
        string s = "No";

        if (GameManager.instance.PlayerHasLittleGuy())
            s = "Yes";

        return s;
    }
}
