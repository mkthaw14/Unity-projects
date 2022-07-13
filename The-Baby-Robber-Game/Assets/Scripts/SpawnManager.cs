using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    public bool isOccupied = false;

    private SpawnPoint _point;
    public SpawnPoint point
    {
        get
        {
            if(_point == null)
                _point = GetComponentInChildren<SpawnPoint>();

            return _point;
        }
    }


    public Vector3 GetPos()
    {
        Vector3 pos = point.transform.position + new Vector3(Random.Range(-1, 2), 0, Random.Range(-1, 2));
        //Debug.DrawRay(point.transform.position, pos - point.transform.position, Color.yellow);
        return pos;
    }    
}
