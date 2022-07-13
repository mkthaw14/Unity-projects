using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandling : MonoBehaviour
{
    [HideInInspector]
    public float vertical;
    [HideInInspector]
    public float horizontal;
    [HideInInspector]
    public float inputX;
    [HideInInspector]
    public float inputY;
    [HideInInspector]
    public float zooming;
    [HideInInspector]
    public bool Aiming;
    [HideInInspector]
    public bool Fire;
    [HideInInspector]
    public bool FireOneShot;
    [HideInInspector]
    public bool Crouch;
    [HideInInspector]
    public bool reload;
    [HideInInspector]
    public bool weaponSwitch;
    [HideInInspector]
    public bool callAirStrike;
    [HideInInspector]
    public bool orderAttack;
    [HideInInspector]
    public bool orderFollow;

    private int aimButton = 1;
    private int fireButton = 0;

    public void UpdateInput()
    {
        vertical = Input.GetAxis(StaticString.v);
        horizontal = Input.GetAxis(StaticString.h);
        inputX = Input.GetAxis(StaticString.mouseX);
        inputY = Input.GetAxis(StaticString.mouseY);
        zooming = Input.GetAxis(StaticString.ZoomInOut);

        Aiming = Input.GetMouseButton(aimButton);
        Fire = Input.GetMouseButton(fireButton);
        FireOneShot = Input.GetMouseButtonDown(fireButton);
        Crouch = Input.GetKey(KeyCode.Space);
        reload = Input.GetKeyDown(KeyCode.R);
        weaponSwitch = Input.GetKeyDown(KeyCode.Q);
        callAirStrike = Input.GetKeyDown(KeyCode.C);
        orderAttack = Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Q);
        orderFollow = Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E);
    }
}
