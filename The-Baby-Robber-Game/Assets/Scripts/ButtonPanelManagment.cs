using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPanelManagment : MonoBehaviour
{
    private Button[] buttons;
    private Text[] button_txt;
    void SetUpAllButtons()
    {
        buttons = GetComponentsInChildren<Button>();
        button_txt = new Text[buttons.Length];

        for(int y = 0; y < button_txt.Length; y++)
        {
            button_txt[y] = buttons[y].GetComponentInChildren<Text>();
        }

        for(int x = 0; x < buttons.Length; x++)
        {
            if (button_txt[x].text == "Restart")
                buttons[x].onClick.AddListener(() => { GameManager.instance.ReloadCurrentScene(); });
            else if (button_txt[x].text == "Back To Main Menu")
                buttons[x].onClick.AddListener(() => { GameManager.instance.SceneLoader(0); });
            else if (button_txt[x].text == "Continue")
                buttons[x].onClick.AddListener(() => { GameManager.instance.LoadNextScene(); });
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        SetUpAllButtons();
    }

}
