using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    Transform childCamera;
    Vector3 moveDirection = new Vector3();

    private void Start()
    {
        childCamera = transform.GetChild(0);
    }
    private void Update()
    {
        moveDirection.z = Input.GetAxis("Vertical");
        moveDirection.x = Input.GetAxis("Horizontal");
        
        if (Input.GetKey(KeyCode.LeftShift))
            moveDirection.y = -1f;
        else if (Input.GetKey(KeyCode.Space))
            moveDirection.y = 1f;
        else
            moveDirection.y = 0;

        transform.Translate(moveDirection);
    }

    private void LateUpdate()
    {
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
        childCamera.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));
    }
}
