using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialResigner : MonoBehaviour
{
    [SerializeField]
    Material newMaterial;

    Material currentMaterial;

    private void Awake()
    {
        if (GameManager.instance.sceneIndex == 12)
        {
            Debug.Log(GameManager.instance.sceneIndex);
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.material = newMaterial;
        }
    }
}
