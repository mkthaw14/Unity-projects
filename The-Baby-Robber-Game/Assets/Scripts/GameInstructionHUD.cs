using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInstructionHUD : TimeEventTextDisplayer
{
    float timer;
    public override void SetUp()
    {
        base.SetUp();
        timeLimitToDisappearText = 6;
        DisplayGameRuleText();
    }

    public override void Tick()
    {
        base.Tick();
        if(timer > 60)
        {
            DisplayGameHint();
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    public void DisplayGameRuleText()
    {
        timeLimitToDisappearText = 3.5f;
        SetActive(true);
        ResetFadeOutTimer();
        displayText.text = UIManager.instance.GetGameRuleText();
    }

    public void HelpPlayerWhatToDoNext(int type, string teamName)
    {
        timeLimitToDisappearText = 3.5f;
        SetActive(true);
        ResetFadeOutTimer();
        displayText.text = ShowHint(type, teamName);
    }

    string ShowHint(int type, string teamName)
    {
        string text = "";
        switch (type)
        {
            case 1:
                text = "You have lost him! Catch him again!";
                break;
            case 2:
                text = "Now, avoid being killed";
                break;
            case 3:
                text = "Good! Now kill more or run";
                break;
            case 4:
                text = teamName + " team has got the little boy";
                break;
            case 5:
                text = teamName + " team currently has higher kill count";
                break;
        }

        return text;
    }

    void DisplayGameHint()
    {
        switch (GameManager.instance.gameMode)
        {
            case GameManager.GameMode.CatchBabyAndDefendYourself:
                if (!GameManager.instance.PlayerHasLittleGuy())
                    DisplayGameRuleText();
                break;
            case GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan:
                DisplayGameRuleText();
                break;
        }
    }
}
