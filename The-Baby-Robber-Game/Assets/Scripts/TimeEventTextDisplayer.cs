using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeEventTextDisplayer : MonoBehaviour
{
    protected Text displayText;
    protected float timeLimitToDisappearText;
    private float timer;

    public virtual void SetUp()
    {
        displayText = GetComponentInChildren<Text>();
    }

    public virtual void Tick()
    {
        if (gameObject.activeSelf)
        {
            if (timer > timeLimitToDisappearText)
            {
                SetActive(false);
                ResetFadeOutTimer();
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }


    protected void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    protected void ResetFadeOutTimer()
    {
        timer = 0;
    }
}
