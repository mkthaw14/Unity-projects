using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject mainCam;
    public Transform snipePos;
    public SniperScope scope;
    public void SetUp()
    {
        scope = GetComponentInChildren<SniperScope>();
        mainCam = Camera.main.gameObject;
    }

    public void ChangeCameraPosition(bool isAiming)
    {
        if (isAiming)
        {
            MoveToSnipePosition();
        }
        else
        {
            MoveToOriginalPosition();
        }
    }

    private void MoveToSnipePosition()
    {
        Vector3 newPos = Vector3.Lerp(mainCam.transform.localPosition, snipePos.localPosition, Time.deltaTime * 3);
        mainCam.transform.localPosition = newPos;
    }

    public void MoveToOriginalPosition()
    {
        mainCam.transform.localPosition = Vector3.zero;
    }
}
