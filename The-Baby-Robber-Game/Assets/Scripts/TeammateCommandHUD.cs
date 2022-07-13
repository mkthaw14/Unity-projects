using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeammateCommandHUD : TimeEventTextDisplayer
{
    public override void SetUp()
    {
        base.SetUp();
        timeLimitToDisappearText = 3;
        SetActive(false);
    }

    public void TeammateCommandText(string command)
    {
        SetActive(true);
        ResetFadeOutTimer();
        displayText.text = command;
    }
}
