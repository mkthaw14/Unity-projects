using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HitPosition : MonoBehaviour
{
    public HumanBodyBones bones;

    Animator anim;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>();
    }

    public void CalculateDamagePoint(ref int health, int fullHealth, int weaponDamage, bool isHumanPlayer)
    {
        int damageAmount = 0;
        int bodyPartDamage = 0;

        switch (bones)
        {
            case HumanBodyBones.Head:
                anim.Play("damage 1");
                if (isHumanPlayer)
                    bodyPartDamage = 50;
                else
                    bodyPartDamage = 100;
                break;
            case HumanBodyBones.Spine:
                anim.Play("damage 1");
                break;

            case HumanBodyBones.LeftUpperArm:
                anim.Play("damage 2");
                break;
            case HumanBodyBones.LeftLowerArm:
                anim.Play("damage 2");
                break;

            case HumanBodyBones.RightUpperArm:
                anim.Play("damage 1");
                break;
            case HumanBodyBones.RightLowerArm:
                anim.Play("damage 1");
                break;

            case HumanBodyBones.LeftUpperLeg:
                anim.Play("damage 2");
                break;
            case HumanBodyBones.LeftLowerLeg:
                anim.Play("damage 2");
                break;

            case HumanBodyBones.RightUpperLeg:
                anim.Play("damage 2");
                break;
            case HumanBodyBones.RightLowerLeg:
                anim.Play("damage 2");
                break;

            case HumanBodyBones.Hips:
                anim.Play("damage 2");
                break;
            default:
                break;
        }

        damageAmount += weaponDamage + bodyPartDamage;
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, fullHealth);
    }

}
