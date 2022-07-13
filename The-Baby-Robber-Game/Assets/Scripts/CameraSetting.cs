using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetting 
{
    public float adaptSpeed = 9f;
    public float moveSpeed = 150f;
    public float aimSpeed = 25f;

    public float smoothX;
    public float smoothY;
    public float lookAngle;
    public float tiltAngle;
    public float y_rotateSpeed;
    public float x_rotateSpeed;
    public float minAngle = -45;
    public float maxAngle = 65;

    public float PositionNormalX = 0.6f;
    public float PositionNormalY = 1.6f; 
    public float PositionNormalZ = -2.5f;
    public float PositionAimingX = 0.4f;
    public float PositionAimingZ = -0.8f;

    public bool invertX;
    public bool invertY;

    public CameraSetting()
    {
        SetDefaultValues();
    }

    public void SetDefaultValues()
    {
        x_rotateSpeed = 3;
        y_rotateSpeed = 3;

        invertX = false;
        invertY = false;
    }
}
