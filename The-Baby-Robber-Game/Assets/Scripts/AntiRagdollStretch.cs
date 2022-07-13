using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRagdollStretch : MonoBehaviour
{
    Vector3 startPosition;
    bool doItOnce;

    private void Awake()
    {
        startPosition = transform.localPosition;
        doItOnce = false;
    }

    private void LateUpdate()
    {
        if(!doItOnce)
            transform.localPosition = startPosition;

        doItOnce = true;
    }


}
