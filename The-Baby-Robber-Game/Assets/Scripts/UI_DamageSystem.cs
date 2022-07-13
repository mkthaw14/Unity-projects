using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UI_DamageSystem : MonoBehaviour
{
    public static Action<Transform> SpawnANewPointer = delegate { };
    public Transform holder;
    public DirectionalPointer pointerPrefab;

    private Camera _cam;
    private Camera Cam
    {
        get
        {
            if (_cam == null)
                _cam = Camera.main;

            return _cam;
        }
    }


    private Dictionary<Transform, DirectionalPointer> pointers = new Dictionary<Transform, DirectionalPointer>();

    private void OnEnable()
    {
        SpawnANewPointer += Register;
    }

    private void OnDisable()
    {
        SpawnANewPointer -= Register;
    }

    private void Register(Transform target)
    {
        if (pointers.ContainsKey(target))
        {
            pointers[target].RestartTimer();
            return;
        }

        DirectionalPointer newPointer = Instantiate(pointerPrefab, holder);
        Transform cameraTransform = Cam.transform;

        newPointer.RegisterTarget(target, cameraTransform, new Action(() => { pointers.Remove(target); }));
        pointers.Add(target, newPointer);
    }

    
}
