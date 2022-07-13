using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUp : MonoBehaviour
{
    public Weapon weapon;

    /*private void //OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WeaponManager>())
        {
            WeaponManager weaponManager = other.GetComponent<WeaponManager>();
            weaponManager.PickUpAmmo(weapon);
            Destroy(gameObject);
        }
    }*/
}
