using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DirectionalPointer : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            return _canvasGroup;
        }
    }

    private RectTransform _rectTransform;
    private RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }

    private Transform target;
    private Transform Cam;
    private Vector3 targetPos;
    private Action unRegister;
    private IEnumerator countDown;

    private const float maxTimer = 4f;
    public float timer = maxTimer;

    public void RegisterTarget(Transform target, Transform Cam, Action unRegister)
    {
        this.target = target;
        this.Cam = Cam;
        this.unRegister = unRegister;

        StartCoroutine(RotateToTheTarget());
        StartTimer();
    }


    public void RestartTimer()
    {
        timer = maxTimer;
        StartTimer();
    }

    public string s;

    private void StartTimer()
    {
        if (countDown != null) { StopCoroutine(countDown); }

        countDown = CalculateTimer();
        StartCoroutine(countDown);
    }

    private IEnumerator RotateToTheTarget()
    {
        while (enabled)
        {
            RotatePointer();
            yield return null;
        }
    }

    private IEnumerator CalculateTimer()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += 3 * Time.deltaTime;
            yield return null;
        }
        while (timer > 0)
        {
            timer--;

            yield return new WaitForSeconds(1);
        }
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= 2 * Time.deltaTime;
            yield return null;
        }
        unRegister();
        Destroy(gameObject);
    }

    private void RotatePointer()
    {
        if (target)
        {
            targetPos = target.position;
        }

        Vector3 dir = Cam.position - targetPos;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        lookRot.z = -lookRot.y;
        lookRot.x = 0;
        lookRot.y = 0;

        rectTransform.localRotation = lookRot * Quaternion.Euler(0, 0, Cam.eulerAngles.y);
    }


}
