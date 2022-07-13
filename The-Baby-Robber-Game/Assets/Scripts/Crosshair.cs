using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public float restingSize;
    public float maxSize;
    public float speed;


    private RectTransform rectTran;
    private float currentSize;

    void Start()
    {
        rectTran = GetComponent<RectTransform>();
        UIManager.instance.HUD_handler.CrossHair = this;
    }

    public void CrosshairSpread(bool spreadLogic)
    {
        if (spreadLogic)
        {
            currentSize = Mathf.Lerp(currentSize, maxSize, Time.deltaTime * speed);
        }
        else
        {
            currentSize = Mathf.Lerp(currentSize, restingSize, Time.deltaTime * speed);
        }

        rectTran.sizeDelta = new Vector2(currentSize, currentSize);
    }




}
