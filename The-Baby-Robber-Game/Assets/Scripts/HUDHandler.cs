using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject scope;
    [SerializeField]
    private GameObject crosshairHUD;
    [SerializeField]
    private GameObject timeHUD;
    [SerializeField]
    private GameObject KillCountHUD_RootObj;

    public KillCountHUD killCountHUD;
    public GameInstructionHUD gameInstructionHUD;
    public TeammateCommandHUD teammateCommandHUD;
    public Crosshair CrossHair;
    public HealthBar playerHealthHUD;

    [SerializeField]
    private Text[] HUDTexts; 


    private float commandTextfadeOutTime = 3.5f;

    public void SetUp()
    {
        gameInstructionHUD = GetComponentInChildren<GameInstructionHUD>();
        teammateCommandHUD = GetComponentInChildren<TeammateCommandHUD>();

        CrossHair = GetComponentInChildren<Crosshair>();
        playerHealthHUD = GetComponentInChildren<HealthBar>();


        gameInstructionHUD.SetUp();
        teammateCommandHUD.SetUp();

        if (GameManager.instance.gameMode == GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan)
        {
            killCountHUD = GetComponentInChildren<KillCountHUD>();
            killCountHUD.SetUp();
            SetTimePanelAndKillPanel(true);
        }
        else
        {
            SetTimePanelAndKillPanel(false);
        }
    }

    public void Tick()
    {
        ShowPlayerAmmo();
        DisplayCrossHair();

        gameInstructionHUD.Tick();
        teammateCommandHUD.Tick();
        if(killCountHUD != null)
            killCountHUD.Tick();
    }

    public void ActivateSniperScope_UI(bool scopeActive, bool crosshairActive)
    {
        scope.SetActive(scopeActive);
        ActivateCrossHair(crosshairActive);
    }


    public void ActivateCrossHair(bool active)
    {
        crosshairHUD.SetActive(active);
    }

    private void DisplayCrossHair()
    {
        if (GameManager.instance.isGameFreeze())
        {
            ActivateCrossHair(false);
        }
        else
        {
            if (GameManager.instance.MainPlayer)
                ActivateCrossHair(GameManager.instance.MainPlayer.aim);
        }
    }



    private void SetTimePanelAndKillPanel(bool val)
    {
        timeHUD.SetActive(val);
        KillCountHUD_RootObj.SetActive(val);
    }



    public Color GetTextColor(string teamName)
    {
        Color c = new Color();

        switch (teamName)
        {
            case "Blue":
                c = Color.blue;
                break;
            case "Red":
                c = Color.red;
                break;
            case "Yellow":
                c = Color.yellow;
                break;
            case "Black":
                c = Color.black;
                break;
            case "Green":
                c = Color.green;
                break;
        }

        return c;
    }



    public void SetTimePanel(bool val)
    {
        timeHUD.SetActive(val);
    }

    public void ShowPlayerAmmo()
    {
        if (!GameManager.instance.MainPlayer) return;

        if (GameManager.instance.MainPlayerWeapons.currentWeapon != null)
        {
            int currentammo = GameManager.instance.MainPlayerWeapons.currentWeapon.currentAmmo;
            int backupammo = GameManager.instance.MainPlayerWeapons.currentWeapon.backUpAmmo;

            Weapon currentWeapon = GameManager.instance.MainPlayerWeapons.currentWeapon;
            bool isRocket = currentWeapon.weaps == Weapon.weaponType.RocketLaucher;
            ShowAmmoCount(currentammo, backupammo, isRocket);
        }
    }

    public void ShowAmmoCount(int currentAmmo, int backUpAmmo, bool isRocket)
    {
        if (!HUDTexts[0])
            return;
        if (isRocket)
        {
            HUDTexts[0].text = currentAmmo.ToString();
        }
        else
        {
            HUDTexts[0].text = currentAmmo + "  /  " + backUpAmmo;
        }
    }

    public void ShowTimer(float timer)
    {
        if (timer < 0) return;
        int minute = Mathf.FloorToInt(timer / 60);
        int second = Mathf.FloorToInt(timer % 60);
        HUDTexts[1].text = minute + " : " + second;
    }

    public void UpdateHealthBar(HealthBar hb, float health, float fullHealth)
    {
        hb.img.fillAmount = health / fullHealth;
    }
}
