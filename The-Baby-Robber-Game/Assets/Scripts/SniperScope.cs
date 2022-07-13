using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperScope : MonoBehaviour
{
    public InputHandling input;
    public WeaponManager Weapons;
    
    public float scrollSpeed;   
    public float minView;
    public float maxView;

    private Camera sniperCam;

    private float fov;
    private bool isSniper;

    void Start()
    {
        input = GameManager.instance.inputHandling;
        Weapons = GameManager.instance.MainPlayerWeapons;
        sniperCam = transform.parent.GetChild(0).GetComponent<Camera>();
    }

    public void ZoomInOut(bool isAiming)
    {
        Weapon currentWeapon = Weapons.currentWeapon;
        isSniper = currentWeapon.weaps == Weapon.weaponType.SniperRifle;

        if (isAiming)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }
    }

    private void ZoomIn()
    {
        fov -= input.zooming * scrollSpeed;
        fov = Mathf.Clamp(fov, minView, maxView);

        sniperCam.fieldOfView = fov;
        UIManager.instance.HUD_handler.ActivateSniperScope_UI(true, false);
    }

    public void ZoomOut()
    {
        fov = 60;
        sniperCam.fieldOfView = fov;
        UIManager.instance.HUD_handler.ActivateSniperScope_UI(false, true);
    }
}
