using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    private AI ai;
    private Character character;

    public string isStopped;
    public bool stop;

    public Character primaryThreat;
    public Vector3 lookRotationVector;
    public Vector3 primaryTargetPos;
    public Vector3 fleePosition;
    public Vector3 hitPosition;
    public float hitDistance;

    public CoverSpot currentCover;
    public float attackRange;
    public float dangerZone;
    public float switchPositionTimer;
    public string s1, s2, s3, s4, s5;
    public string debug1, debug2, debug3, debug4, debug5;
    public string combatBehaviours;

    public void ClearString()
    {
        debug1 = debug2 =  debug3 = debug4 = debug5 = "";
    }

    public void SetUp(Character character, AI ai)
    {
        this.character = character;
        this.ai = ai;
    }

    private void Update()
    {
        if (ai != null && ai.nav && ai.nav.isOnNavMesh)
        {
            ai.nav.isStopped = stop;
            isStopped = ai.nav.isStopped.ToString();
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(fleePosition, Vector3.one);
    }

    public void MarkEnemyPos()
    {
        ai.attackZone = primaryThreat.transform.position;
    }

    public void GetTarget(Character target)
    {
        if (target == null) return;
        primaryThreat = target;
    }


    public Vector3 GetDir(Vector3 origin, Vector3 targetPos)
    {
        return targetPos - origin;
    }

    public float GetDist(Vector3 origin, Vector3 target)
    {
        float dist = Vector3.Distance(origin, target);
        return dist;
    }

    public bool EnemyInsideAttackZone(Vector3 attackZone, Vector3 target, float limitedRange)
    {
        return GetDist(target, attackZone) < limitedRange;
    }

    public bool EnemyOutsideAttackZone(Vector3 attackZone, Vector3 target, float limitedRange)
    {
        return GetDist(target, attackZone) > limitedRange;
    }


    public bool OriginInsideAttackZone(Vector3 attackZone, Vector3 origin, float limitedRange)
    {
        return GetDist(origin, attackZone) < limitedRange;
    }

    public bool OriginOutsideAttackZone(Vector3 attackZone, Vector3 origin, float limitedRange)
    {
        return GetDist(origin, attackZone) > limitedRange;
    }


    public bool DistFromCoverToAttackZone(Vector3 coverPos, Vector3 attackZone, float limitedRange)
    {
        return  GetDist(coverPos, attackZone) < limitedRange;
    }

    public bool DistFromEnemy(Vector3 origin, Vector3 target, float limitedRange)
    {
        return GetDist(origin, target) < limitedRange;
    }

    bool DistCoverFromOrigin(Vector3 origin, Vector3 cover, float limitedRange)
    {
        return GetDist(origin, cover) < limitedRange;
    }

    public bool CoverDistToAttackZone
    {
        get
        {
            return DistFromCoverToAttackZone(currentCover.transform.position,
                ai.attackZone, attackRange * 2.2f);
        }
    }

    public bool CoverDistToOrigin
    {
        get
        {
            return DistCoverFromOrigin(character.transform.position, currentCover.transform.position, attackRange);
        }
    }


    
}
