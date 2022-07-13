using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenu : MonoBehaviour
{
    [SerializeField]
    GameObject firstButton;
    UIMenuHandler uIMenuHandler;

    private void OnEnable()
    {
        SetUpFirstButton();
    }

    protected void SetUpFirstButton()
    {
        uIMenuHandler = GetComponentInParent<UIMenuHandler>();
        uIMenuHandler.ChangeFirstSelectedButton(firstButton);
    }

    public virtual void SetUp() 
    {

    }

    public virtual void Tick() { }

}
