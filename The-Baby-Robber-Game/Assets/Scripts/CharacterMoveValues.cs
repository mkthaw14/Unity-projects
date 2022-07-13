using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MoveVariables")]
public class CharacterMoveValues : ScriptableObject
{
    public float moveAmount;
    public float speed;
    public float crouchSpeed;
    public float walkSpeed;
    public float runSpeed;
    public float aimSpeed;
    public float rotateSpeed;
    public Vector3 moveDirection;
    public Vector3 rotateDirection;
}
