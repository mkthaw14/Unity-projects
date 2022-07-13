using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public int currentammo;
    public int fullammo;
    public int backupammo;

    public List<Transform> UnEquipTransforms = new List<Transform>();
    public Weapon prefab_1, prefab_2, prefab_3;
    public List<Weapon> weapons = new List<Weapon>();
    public CharacterIK charIK;
    public Character User;
    public delegate void ShootWeapon();
    public ShootWeapon shootWeapon;

    public LayerMask hitObjects;
    public Weapon currentWeapon;
    public WeaponProperties currentWepPro;

    public bool switchWeapon;
    public bool spawn;

    public int weaponIndex;
    private int bulletCount;

    private float spreadX;
    private float spreadY;

    public void SetUp(CharacterIK IK, Transform rHand, Transform spine)
    {
        charIK = IK;
        User = charIK.GetComponentInParent<Character>();
       
        CreateWeapons(IK, rHand);
        CreateUnEquipPointsAndMoveOtherWeaponsToUnEquipPosition(spine);
        currentWeapon = weapons[0];
        currentWeapon.Equip(this, charIK);
        GetCurrentWeaponProperties();
        CheckCurrentWeapon();
    }

    void CreateWeapons(CharacterIK IK, Transform rHand)
    {
        Weapon firstWeapon = Instantiate(prefab_1, rHand);
        firstWeapon.SetUp(IK, this);
        weapons.Add(firstWeapon);
    }

    void CreateUnEquipPointsAndMoveOtherWeaponsToUnEquipPosition(Transform spine)
    {
        for (int x = 0; x < weapons.Count; x++)
        {
            CreateUnEquipPoints(spine, x);
        }

        for (int y = 0; y < UnEquipTransforms.Count; y++)
        {
            SetUpWeaponUnEquipPositionAndRotation(y);
        }
    }

    void CreateUnEquipPoints(Transform spine, int index)
    {
        Transform t = new GameObject("UnEquipPoint " + index).transform;
        t.SetParent(spine);

        t.localPosition = WeaponProperties.EquipSettings.unEquipPositions[index];
        t.localEulerAngles = WeaponProperties.EquipSettings.unEquipRotations[index];

        UnEquipTransforms.Add(t);

    }

    void SetUpWeaponUnEquipPositionAndRotation(int index)
    {
        weapons[index].transform.SetParent(UnEquipTransforms[index]);
        weapons[index].transform.localPosition = Vector3.zero;
        weapons[index].transform.localEulerAngles = Vector3.zero;
        weapons[index].UnequipT = UnEquipTransforms[index];
    }

    public void DisableWeapon()
    {
        if (currentWeapon) 
        {
            currentWeapon.transform.SetParent(null);
            currentWeapon.rigid.isKinematic = false;
            currentWeapon.User = null;
            currentWeapon = null;
        }

        for(int x = 0; x < weapons.Count; x++)
        {
            weapons[x].transform.SetParent(null);
            weapons[x].rigid.isKinematic = false;
            weapons[x].User = null;
        }

        weapons.Clear();
    }

    public void ChangeWeapon()
    {
        currentWeapon.UnEquip();
		if (weaponIndex == weapons.Count - 1)
			weaponIndex = 0;
		else 
			weaponIndex++;		

        Weapon newWeapon = weapons[weaponIndex];

        currentWeapon = newWeapon;
        currentWeapon.Equip(this, charIK);

  
        CheckCurrentWeapon();
    }


    public Weapon isSameWeapon(Weapon newWeapon)
    {
        Weapon weapon = null;
        for (int x = 0; x < weapons.Count; x++)
        {
            if (weapons[x].weaps == newWeapon.weaps)
            {
                weapon = weapons[x];
                break;
            }
        }

        return weapon;
    }

    public float shootingTimer;

    public void GetCurrentWeaponProperties()
    {
        if (currentWeapon)
        {
            currentWepPro = currentWeapon.weapPro;
        }
    }

    public Weapon GetAnyWeaponWithAmmo()
    {
        Weapon w = null;

        for(int x = 0; x < weapons.Count; x++)
        {
            if(weapons[x].currentAmmo != 0 || weapons[x].backUpAmmo != 0)
            {
                w = weapons[x];
                break;
            }
        }

        return w;
    }

    Weapon GetWeaponType(Weapon.weaponType weaponType)
    {
        Weapon w = null;

        for(int x = 0; x < weapons.Count; x++)
        {
            if(w.weaps == weaponType)
            {
                w = weapons[x];
                break;
            }
        }

        return w;
    }

    void CheckCurrentWeapon()
    {
        switch (currentWeapon.weaps)
        {
            case Weapon.weaponType.Pistol:
                GetCurrentWeaponProperties();
                charIK.ChangeWeaponAnim(2);
                break;
            case Weapon.weaponType.AssaultRifle:
                GetCurrentWeaponProperties();
                charIK.ChangeWeaponAnim(1);
                break;
            case Weapon.weaponType.ShotGun:
                GetCurrentWeaponProperties();
                charIK.ChangeWeaponAnim(1);
                break;
            case Weapon.weaponType.SniperRifle:
                GetCurrentWeaponProperties();
                charIK.ChangeWeaponAnim(1);
                break;
            case Weapon.weaponType.BurstFireRifle:
                GetCurrentWeaponProperties();
                charIK.ChangeWeaponAnim(1);
                break;
            case Weapon.weaponType.RocketLaucher:
                GetCurrentWeaponProperties();
                charIK.ChangeWeaponAnim(1);
                break;
        }
    }

    public void SetUpAmmo(Character.ControlType controlType)
    {
        for (int x = 0; x < weapons.Count; x++)
        {
            weapons[x].fullAmmo = weapons[x].weapPro.weapSetting.fullAmmo;

            if (weapons[x].weaps == Weapon.weaponType.RocketLaucher)
            {
                weapons[x].currentAmmo = weapons[x].weapPro.weapSetting.GetBackUpAmmo(controlType);
            }
            else
            {
                weapons[x].backUpAmmo = weapons[x].weapPro.weapSetting.GetBackUpAmmo(controlType);
                weapons[x].currentAmmo = weapons[x].weapPro.weapSetting.fullAmmo;
            }
        }
    }

    public void GetCurrentWeapAmmo()
    {
        currentammo = currentWeapon.currentAmmo;
        fullammo = currentWeapon.fullAmmo;
        backupammo = currentWeapon.backUpAmmo;
    }

    public void FireWeapon(Vector3 origin, Vector3 dir, Quaternion projectileStartRotation)
    {
        switch (currentWeapon.weaps)
        {
            case Weapon.weaponType.Pistol:
                GetSpread(User);
                shootWeapon?.Invoke();
                currentWeapon.BulletProjectile(User, origin, dir, projectileStartRotation, spreadX, spreadY, bulletCount);
                break;
            case Weapon.weaponType.AssaultRifle:
                GetSpread(User);
                shootWeapon?.Invoke();
                currentWeapon.BulletProjectile(User, origin, dir, projectileStartRotation, spreadX, spreadY, bulletCount);
                break;
            case Weapon.weaponType.ShotGun:
                GetSpread(User);
                shootWeapon?.Invoke();
                currentWeapon.BulletProjectile(User, origin, dir, projectileStartRotation, spreadX, spreadY, bulletCount);
                break;
            case Weapon.weaponType.SniperRifle:
                GetSpread(User);
                shootWeapon?.Invoke();
                currentWeapon.BulletProjectile(User, origin, dir, projectileStartRotation, spreadX, spreadY, bulletCount);
                break;
            case Weapon.weaponType.BurstFireRifle:
                GetSpread(User);
                shootWeapon?.Invoke();
                currentWeapon.BulletProjectile(User, origin, dir, projectileStartRotation, spreadX, spreadY, bulletCount);
                break;
            case Weapon.weaponType.RocketLaucher:
                shootWeapon?.Invoke();
                currentWeapon.RocketProjectile(User, currentWeapon.firePoint, origin, dir);
                break;
        }
    }

    public bool IsSniper()
    {
        return currentWeapon.weaps == Weapon.weaponType.SniperRifle;
    }


    public bool EmptyWeapon()
    {
        bool val = false;
        if (currentWeapon != null)
            val = currentWeapon.currentAmmo <= 0;
        return val;
    }

    public bool IsRocketLauncher()
    {
        return currentWeapon.weaps == Weapon.weaponType.RocketLaucher;
    }

    public bool readyToFire()
    {
        return !currentWeapon.resettingCatridge;
    }
    void GetSpread(Character owner)
    {
        bulletCount = currentWepPro.weapSetting.bulletCount;
        spreadX = GetSpreadX(owner);
        spreadY = GetSpreadY(owner);
    }

    float GetSpreadX(Character owner)
    {
        return currentWepPro.weapSetting.spreadX; // + owner.weapSkill.spreadX;
    }

    float GetSpreadY(Character owner)
    {
        return currentWepPro.weapSetting.spreadY; //+ owner.weapSkill.spreadY;
    }
}


