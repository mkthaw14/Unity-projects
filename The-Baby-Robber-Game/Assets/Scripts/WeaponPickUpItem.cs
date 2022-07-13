using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUpItem : MonoBehaviour
{
    SphereCollider sphereCollider;
    Weapon newWeapon;

    public AI potentialLooter;
    public Weapon GetWeapon { get { return newWeapon; } private set { } }

    public void DestroyItem(ref WeaponPickUpItem item)
    {
        item = null;
        Destroy(gameObject);
    }

    private void Awake()
    {
        newWeapon = GetComponent<Weapon>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.GetComponent<Character>() && newWeapon.User == null)
    //    {
    //        Character c = other.GetComponent<Character>();
    //        c.PickableWeapon(this, true);
    //    }

    //}


    //private void OnTriggerExit(Collider other)
    //{
    //    if(other.GetComponent<Character>() && newWeapon.User == null)
    //    {
    //        Character c = other.GetComponent<Character>();
    //        c.PickableWeapon(null, false);
    //    }
    //}


    private Weapon isSameWeapon(WeaponManager weaponManager)
    {
        Weapon weapon = null;
        for(int x = 0; x < weaponManager.weapons.Count; x++)
        {
            if(weaponManager.weapons[x].weaps == newWeapon.weaps)
            {
                weapon = weaponManager.weapons[x];
                break;
            }                
        }

        return weapon;
    }
}
