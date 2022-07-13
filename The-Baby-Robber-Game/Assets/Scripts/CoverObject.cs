using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverObject : MonoBehaviour
{
    public List<Collider> collidersWithinThisCoverRadius = new List<Collider>();

    //IMPORTANT NOTE : OTHER COLLIDER MUST HAS A RIGIBODY OR IT WILL NOT WORK!

    public bool isTargetBehindThisCover(int layer)
    {
        bool val = false;
        for (int x = 0; x < collidersWithinThisCoverRadius.Count; x++)
        {
            if (layer == collidersWithinThisCoverRadius[x].gameObject.layer)
                val = true;
        }

        return val;
    }
}
