using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : UIMenu
{
    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.instance.ResumeGame();
            }
        }
    }
}
