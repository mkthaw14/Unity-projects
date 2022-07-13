using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Weapon Properties"))]
public class WeaponProperties : ScriptableObject
{
    [SerializeField]
    public WeaponSettings weapSetting;
    [SerializeField]
    public HandIKSettings handIKsettings;
    [SerializeField]
    public EquipSettings equipSettings;

    [System.Serializable]
    public class HandIKSettings
    {
        public Vector3 rHandPos;
        public Vector3 rHandRot;
    }

    [System.Serializable]
    public class EquipSettings
    {
        [Header("Equip")]
        public Vector3 equipPos;
        public Vector3 equipRot;

        [Header("Unequip")]
        public Vector3 unequipPos;
        public Vector3 unequipRot;

        [Header("Settings for all UnEquip Positions and Rotations")]
        public static Vector3[] unEquipPositions = new Vector3[3];
        public static Vector3[] unEquipRotations = new Vector3[3];

        public EquipSettings()
        {
            unEquipPositions[0] = new Vector3(-0.067f, 0.107f, -0.034f);
            unEquipPositions[1] = new Vector3(0.0103f, 0.0041f, -0.0705f);
            unEquipPositions[2] = new Vector3(-0.072f, 0.071f, 0.042f);

            unEquipRotations[0] = new Vector3(-266.924f, 94.423f, 95.978f);
            unEquipRotations[1] = new Vector3(81.242f, 196.526f, 111.173f);
            unEquipRotations[2] = new Vector3(-266.803f, 111.006f, 292.844f);
        }
    }


    [System.Serializable]
    public class WeaponSettings
    {
        public int weaponDamage;
        public int bulletCount;

        public int currentAmmmo;
        public int fullAmmo;
        public float recoilAmount;
        public float recoilSpeed;
        public float spreadX;
        public float spreadY;
        public float bulletLoadTime;
        public float weaponRange;

        [SerializeField]
        private int backUpAmmo;// Note call only from GetBackUpAmmo()

        public int GetBackUpAmmo(Character.ControlType controlType)
        {
            int amount = 0;

            switch (controlType)
            {
                case Character.ControlType.ComputerControlled:
                    amount = int.MaxValue;
                    break;
                case Character.ControlType.HumanControlled:
                    amount = backUpAmmo;
                    break;
            }

            return amount;
        }
    }

    //Recoil
    public AnimationCurve recoilZ;
    public AnimationCurve recoilY;

    public int GetWeaponDMG(int damageMultiplier = 1)
    {
        if (damageMultiplier == 0)
            damageMultiplier = 1;

        return weapSetting.weaponDamage * damageMultiplier;
    }
}
