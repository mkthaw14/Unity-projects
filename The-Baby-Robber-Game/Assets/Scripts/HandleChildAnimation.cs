using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleChildAnimation : MonoBehaviour
{
    [SerializeField] private Vector3 RhandPos, LhandPos, RfeetPos, LfeetPos;
    [SerializeField] private Vector3 LhintPos;
    [SerializeField] private Transform handR, handL, feetR, feetL, hintL;
    [SerializeField] private float IKWeight;

    SuperObject superObject;
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        superObject = GetComponentInParent<SuperObject>();
        CreateIKTargetObjects();
        SetUpIKTargetPosRot();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (superObject.owner)
        {
            IKWeight = 1;

            UpateIKHint();

            UpdateIK(AvatarIKGoal.RightHand, handR.position, handR.eulerAngles, IKWeight);
            UpdateIK(AvatarIKGoal.LeftHand, handL.position, handL.eulerAngles, IKWeight);

            UpdateIK(AvatarIKGoal.RightFoot, feetR.position, feetR.eulerAngles, IKWeight);
            UpdateIK(AvatarIKGoal.LeftFoot, feetL.position, feetL.eulerAngles, IKWeight);
        }
        else
        {
            IKWeight = 0;
        }
    }

    void CreateIKTargetObjects()
    {
        handR = new GameObject("Right Hand").transform;
        handR.SetParent(transform.parent);

        handL = new GameObject("Left Hand").transform;
        handL.SetParent(transform.parent);

        feetR = new GameObject("Right Feet").transform;
        feetR.SetParent(transform.parent);

        feetL = new GameObject("LeftFeet").transform;
        feetL.SetParent(transform.parent);

        hintL = new GameObject("Left Hint").transform;
        hintL.SetParent(transform.parent);
    }

    void SetUpIKTargetPosRot()
    {
        handR.localPosition = RhandPos;
        handL.localPosition = LhandPos;

        feetR.localPosition = RfeetPos;
        feetL.localPosition = LfeetPos;

        hintL.localPosition = LhintPos;
    }

    void UpdateAnimation()
    {
        animator.SetFloat("Speed", superObject.GetNavmeshSpeed());
    }

    void UpdateIK(AvatarIKGoal goal, Vector3 pos, Vector3 rot, float IKWeight)
    {
        animator.SetIKPositionWeight(goal, IKWeight);
        animator.SetIKPosition(goal, pos);

        animator.SetIKRotationWeight(goal, IKWeight);
        animator.SetIKRotation(goal, Quaternion.Euler(rot));
    }

    void UpateIKHint()
    {
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, IKWeight);
        animator.SetIKHintPosition(AvatarIKHint.LeftKnee, hintL.position);
    }
}
