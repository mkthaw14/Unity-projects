using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject pauseMenuUI, winningMenuUI, losingMenuUI;
    public UIMenuHandler uIMenuHandler;
    public HUDHandler HUD_handler;
    public OffScreenIndicator offscreenIndicator;

    string levelUnlockedText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        SetUp();     
    }

    void Update()
    {
        PauseGame();
        HUD_handler.Tick();
    }

    void SetUp()
    {
        offscreenIndicator = GetComponentInChildren<OffScreenIndicator>();
        uIMenuHandler = GetComponentInChildren<UIMenuHandler>();
        HUD_handler = GetComponentInChildren<HUDHandler>();
        HUD_handler.SetUp();
    }

    void PauseGame()
    {
        if (!pauseMenuUI.activeSelf && !GameManager.instance.isGameFreeze())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                pauseMenuUI.SetActive(true);
                DisplayHintDuringPauseScreen();
                GameManager.instance.SetGameSpeed(true);
            }
        }
    }

    void CongradulatePlayer()
    {
        Text congradulateText = winningMenuUI.transform.GetChild(0).GetComponent<Text>();
        switch (GameManager.instance.gameMode)
        {
            case GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan:
                congradulateText.text = "You are winner!" + "\n" + "Your team was able to kill more than other teams while keeping the little boy with you." + "\n" + levelUnlockedText;
                break;
            case GameManager.GameMode.CatchBabyAndDefendYourself:
                congradulateText.text = "You are winner!" + "\n" + "Your team was able to hold the little boy for certain period of time." + "\n" + levelUnlockedText;
                break;
        }
    }

    void CommentingPlayer()
    {
        Text commentText = losingMenuUI.transform.GetChild(0).GetComponent<Text>();
        switch (GameManager.instance.gameMode)
        {
            case GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan:
                if (GameManager.instance.KillCountNotEnough() && GameManager.instance.PlayerHasLittleGuy())
                {
                    commentText.text = "You are loser!" + "\n" + "You were able to get the little boy but did not kill enough.";
                }
                else if (GameManager.instance.KillCountNotEnough())
                {
                    commentText.text = "You are loser!" + "\n" + "You did not kill enough and could not get the little boy";
                }
                else
                {
                    commentText.text = "You are loser!" + "\n" + "You did killing more but could not get the little boy.";
                }
                break;
            case GameManager.GameMode.CatchBabyAndDefendYourself:
                commentText.text = "You are loser!" + "\n" + "Your team could not keep the little boy.";
                break;
        }
    }

    void DisplayHintDuringPauseScreen()
    {
        Text hint = pauseMenuUI.transform.GetChild(0).GetComponent<Text>();
        hint.text = GetGameRuleText() + "\n" + "Hold Alt key and press Q to order teammates to attack";
    }



    public void ShowWinningOrLosingScreen(bool enable, int index)
    {
        switch(index)
        {
            case 1:
                winningMenuUI.SetActive(enable);
                CongradulatePlayer();
                GameManager.instance.SetGameSpeed(true);
                break;
            case 2:
                losingMenuUI.SetActive(enable);
                CommentingPlayer();
                GameManager.instance.SetGameSpeed(true);
                break;
        }    
    }

    public string GetGameRuleText()
    {
        string text = "";
        switch (GameManager.instance.gameMode)
        {
            case GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan:
                text = "Get higher kill count and the little boy before the time runs out";
                break;
            case GameManager.GameMode.CatchBabyAndDefendYourself:
                text = "Get the little boy and survive";
                break;
        }

        return text;
    }

    public void LevelUnlockedText(bool newLevelUnlocked)
    {
        if (GameManager.instance.sceneIndex == 12 && newLevelUnlocked)
            levelUnlockedText = "Congratulation! You have beaten my game";
        else
            levelUnlockedText = newLevelUnlocked ? "A new level has been unlocked" : "";
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        GameManager.instance.SetGameSpeed(false);
    }
}
