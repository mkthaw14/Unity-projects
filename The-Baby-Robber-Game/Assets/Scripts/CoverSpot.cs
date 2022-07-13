using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverSpot : MonoBehaviour
{
    public AI currentUser;
    public LayerMask coverLayer;
    public LayerMask PeekLayer;
    public Transform[] peekPoints;

    public bool canSeeTarget;
    public bool preoccupied;
    public bool isReliable;

    public Transform[] duckFiringTransform = new Transform[2];
    private Vector3 PeekPosition;

    [System.Serializable]
    public enum CoverHeight
    {
        High, Low
    }

    [System.Serializable]
    public enum RayDir
    {
        right, left, forward, back
    }

    [SerializeField]
    public RayDir rayDir;
    [SerializeField]
    public CoverHeight Height;

    private Vector3 rayDirection;



    public string NotSafe;

    public bool ShootRay(Vector3 target, Vector3 origin)
    {
        Vector3 dir2 = target - origin;
        dir2.Normalize();
        bool notSafe = !Physics.Raycast(origin, dir2, 4f, LayerMask.GetMask("CoverObject"));
        NotSafe = notSafe.ToString();

        return notSafe;
    }

    void Start()
    {
        EnableTrigger();
        PeekLayer = LayerMask.GetMask("PeekPos");
        GetDuckFiringPosition();
    }


    void EnableTrigger()
    {
        Collider[] cols = transform.GetComponentsInChildren<Collider>();

        for (int x = 0; x < cols.Length; x++)
        {
            cols[x].isTrigger = true;

            if (!cols[x].GetComponent<CoverSpot>())
                cols[x].gameObject.layer = 13;
        }
    }


    public AI SetOwner
    {
        set
        {
            if (currentUser == null)
                currentUser = value;
        }
    }

    public AI RemoveOwner()
    {
        currentUser = null;
        return currentUser;
    }

    public Vector3 GetPeekPosition
    {
        get
        {
            return PeekPosition;
        }
    }

    public bool CanShootFromHighWallCover(AI user)
    {
        bool val = false;

        for (int x = 0; x < duckFiringTransform.Length; x++)
        {
            Vector3 enemyDir = user.aiManager.primaryThreat.chestTran.position - (duckFiringTransform[x].position * user.character.aimPivot.position.y);

            Ray ray = new Ray(duckFiringTransform[x].position, enemyDir);

            bool isBlockedByObjects = Physics.Raycast(ray, 2f, LayerMask.GetMask("Obstacle", "CoverObject"));

            if (!isBlockedByObjects)
            {
                val = true;
                PeekPosition = duckFiringTransform[x].position;
                break;
            }
            else
            {
                val = false;
            }
        }

        return val;
    }

    

    private void GetDuckFiringPosition()
    {
        RaycastHit hit;

        Ray[] rays = new Ray[2];

        Vector3 origin = transform.position;
        Vector3 dir = GetRayDir();
        Vector3[] dirs = new Vector3[2];

        dirs[0] = dir;
        dirs[1] = -dir;


        for (int x = 0; x < rays.Length; x++)
        {
            rays[x] = new Ray(origin, dirs[x]);

            if (Physics.Raycast(rays[x], out hit, 1f, PeekLayer, QueryTriggerInteraction.Collide))
            {
                duckFiringTransform[x] = hit.transform;
            }
        }
    }

    private Vector3 GetRayDir()
    {
        switch (rayDir)
        {
            case RayDir.forward:
                rayDirection = transform.forward;
                break;
            case RayDir.back:
                rayDirection = -transform.forward;
                break;
            case RayDir.right:
                rayDirection = transform.right;
                break;
            case RayDir.left:
                rayDirection = -transform.right;
                break;
        }

        return rayDirection;
    }

}
