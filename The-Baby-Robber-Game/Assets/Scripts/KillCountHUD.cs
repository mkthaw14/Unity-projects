using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillCountHUD : MonoBehaviour
{
    [SerializeField]
    private Text prefabText;
    private Text[] killCountTexts;
    private HUDHandler hUDHandler;

    public void SetUp()
    {
        hUDHandler = GetComponentInParent<HUDHandler>();
        killCountTexts = GetComponentsInChildren<Text>();
        CreateTextFieldForKillCount();
    }

    public void Tick()
    {
        UpdateKillCount();
    }

    private void CreateTextFieldForKillCount()
    {
        killCountTexts = new Text[GameManager.instance.allTeams.Count];

        for (int x = 0; x < GameManager.instance.allTeams.Count; x++)
        {
            Text newTxt = Instantiate(prefabText, gameObject.transform);
            newTxt.fontSize = 20;
            newTxt.resizeTextForBestFit = true;
            newTxt.GetComponent<RectTransform>().localScale = Vector3.one;
            killCountTexts[x] = newTxt;

            killCountTexts[x].color = hUDHandler.GetTextColor(GameManager.instance.allTeams[x].teamName);
        }
    }

    private void UpdateKillCount()
    {
        for (int x = 0; x < GameManager.instance.allTeams.Count; x++)
        {
            killCountTexts[x].text = GameManager.instance.allTeams[x].teamName + '\n' + "Kills" + '\n' + GameManager.instance.allTeams[x].killCount;
        }
    }
}
