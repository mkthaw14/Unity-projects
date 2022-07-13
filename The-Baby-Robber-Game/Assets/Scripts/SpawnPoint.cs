using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public int spawnLimit;
    public LayerMask layer;
    public Collider[] guardCol;

    public bool CheckTowerGuardIsAlive()
    {
        guardCol = Physics.OverlapSphere(transform.position, 2, layer);

        if (guardCol.Length == 0)
            return false;

        else
            return true;
    }
}
