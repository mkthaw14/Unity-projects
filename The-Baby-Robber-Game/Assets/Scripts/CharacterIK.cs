using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIK : MonoBehaviour
{
    [HideInInspector]
    public Transform l_HandTarget;
    [HideInInspector]
    public Transform equipT;
    [HideInInspector]
    public Transform Spine;

    public float rH_weight;
    public float lookAtWeight;
    public float bodyWeight;
    public float headWeight;

    private Animator anim;
    private Vector3 aimPos;

    private Transform aimPivot;
    private Transform shoulder;
    private Transform chest;
    private Transform l_Hand;
    private Transform R_Hand; 

    private Character character;
    private WeaponManager wepMan;
    

    private bool isAiming;
    private bool reloading;
    private bool switchingWeapon;
    

    void Update()
    {
        Tick();
    }

    public void SetUp(Character c, WeaponManager manager)
    {
        character = c;
        wepMan = manager;

        if(anim == null)
            anim = GetComponent<Animator>();

        SetUpBoneTrans();
        SetUpAimPivot();
        manager.SetUp(this, R_Hand, Spine);

        character.aimPivot = aimPivot;
        character.chestTran = chest;

        SetUpIK();

    }

    public void Tick()
    {
        if (!character) return;

        isAiming = character.aim;
        reloading = character.reloading;
        switchingWeapon = character.weaponSwitch;

    }

    void SetUpBoneTrans()
    {
        shoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder).transform;
        equipT = anim.GetBoneTransform(HumanBodyBones.RightHand).transform;
        Spine = anim.GetBoneTransform(HumanBodyBones.Spine).transform;
        chest = anim.GetBoneTransform(HumanBodyBones.Chest).transform;
    }

    void SetUpAimPivot()
    {
        aimPivot = new GameObject().transform;
        aimPivot.name = "AimPivot";
        aimPivot.transform.parent = transform.parent;

        R_Hand = new GameObject().transform;
        R_Hand.name = "Right Hand";
        R_Hand.transform.parent = aimPivot;

        l_Hand = new GameObject().transform;
        l_Hand.name = "Left Hand";
        l_Hand.transform.parent = aimPivot;
    }

    void SetUpIK()
    {
        if (wepMan.currentWeapon)
        {
            R_Hand.localPosition = wepMan.currentWepPro.handIKsettings.rHandPos;
            R_Hand.localEulerAngles = wepMan.currentWepPro.handIKsettings.rHandRot;
            
            character.SetUpHandIK(R_Hand, R_Hand.localPosition, R_Hand.localEulerAngles);
        }
    }

    void HandleLeftHand()
    {
        l_Hand.rotation = l_HandTarget.rotation;
        l_Hand.position = l_HandTarget.position;
    }

    void HandleWeight()
    {
        if (isAiming)
            rH_weight += Time.deltaTime * 2f;
        else 
            rH_weight -= Time.deltaTime * 2f;

        rH_weight = Mathf.Clamp(rH_weight, 0, 1);
    }

    void OnAnimatorMove()
    {
        if (!shoulder) return;

        HandleShoulder();
        CheckTakingDamageAnimState();
    }

    void OnAnimatorIK()
    {
        if (!shoulder) return;

        HandleLeftHand();
        HandleIK();
    }

    void HandleShoulderRotation()
    {
        aimPos = character.aimPos;
        Vector3 LookDir = aimPos - aimPivot.position;

        Quaternion lookRot = Quaternion.LookRotation(LookDir);
        aimPivot.rotation = Quaternion.Slerp(aimPivot.rotation, lookRot, Time.deltaTime * 1000f);

    }

    void HandleShoulderPos()
    {
        aimPivot.position = shoulder.position;
    }

    void UpdateIK(AvatarIKGoal goal, Transform Hand, float handWeight)
    {
        anim.SetIKPositionWeight(goal, handWeight);
        anim.SetIKRotationWeight(goal, handWeight);

        anim.SetIKPosition(goal, Hand.position);
        anim.SetIKRotation(goal, Hand.rotation);
    }

    void HandleShoulder()
    {
        HandleShoulderPos();
        HandleShoulderRotation();
    }

    void LookAtTarget(float l_w, float b_w, float h_w)
    {
        anim.SetLookAtWeight(l_w, b_w, h_w);
        anim.SetLookAtPosition(character.spineLookPos);       
    }

    void HandleIK()
    {
        HandleWeight();

        if (reloading)
            return;

        else if (character.aim)
        {
            LookAtTarget(lookAtWeight, bodyWeight, headWeight);
            
            if (l_HandTarget != null)
            {
                UpdateIK(AvatarIKGoal.LeftHand, l_Hand, 1);
            }

            UpdateIK(AvatarIKGoal.RightHand, R_Hand, rH_weight);
        }
        else if(!character.isTakingDamage)
        {
            UpdateIK(AvatarIKGoal.LeftHand, l_Hand, 1);
        }

    }

    public void ChangeWeaponAnim(int id)
    {
        character.animID = id;
        SetUpIK();
    }


    void CheckTakingDamageAnimState()
    {
        if (anim.GetCurrentAnimatorStateInfo(2).IsName("damage 1"))
        {
            character.isTakingDamage = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(2).IsName("damage 2"))
        {
            character.isTakingDamage = true;
        }
        else
        {
            character.isTakingDamage = false;            
        }
    }
}

