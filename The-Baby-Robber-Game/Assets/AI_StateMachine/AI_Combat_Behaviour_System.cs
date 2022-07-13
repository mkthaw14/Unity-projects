using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Combat_Behaviour_System 
{
    public AI ai;
    public AIManager aiManager;

    AI_TeamLeader_Behaviour_System leaderBehaviourSystem;
    AI_Folower_Behaviour_System followerBehaviourSystem;

    float switchPositionTimer;
    float coverHoldingTimer;

    [SerializeField]
    public Behaviours behaviours;

    [System.Serializable]
    public enum Behaviours
    {
        MoveToEnemyPosition,
        FindPositionsToSpreadOut,
        FindNewPositions,
        MoveToNewPositions,
        TakeCover,
        HoldCover
    }

    public void SetUp(AI ai, AIManager aiManager, AI_TeamLeader_Behaviour_System leaderBehaviourSystem, AI_Folower_Behaviour_System followerBehaviourSystem)
    {
        this.ai = ai;
        this.aiManager = aiManager;
        this.leaderBehaviourSystem = leaderBehaviourSystem;
        this.followerBehaviourSystem = followerBehaviourSystem;
    }

    public void UpdateBehaviours()
    {
        switch (behaviours)
        {
            case Behaviours.MoveToEnemyPosition:
                //Debug.Log("Hgg3" + ai.character);
                MoveToEnemyPosition();
                break;
            case Behaviours.FindPositionsToSpreadOut:
                FindPositionsToSpreadOut();
                break;
            case Behaviours.FindNewPositions:
                //Debug.Log("Hgg4" + ai.character);
                FindNewPositions();
                break;
            case Behaviours.MoveToNewPositions:
                //Debug.Log("Hgg5" + ai.character);
                MoveToNewPositions();
                break;
            case Behaviours.TakeCover:
                //Debug.Log("Hgg6" + ai.character);
                TakeCover();
                break;
            case Behaviours.HoldCover:
                //Debug.Log("Hgg7" + ai.character);
                HoldCover();
                break;
        }

        ai.character.AI_CombatBehaviourState = behaviours.ToString();
        aiManager.combatBehaviours = behaviours.ToString();
    }

    public void SwitchBehaviour(Behaviours behaviour)
    {
        this.behaviours = behaviour;
    }


    private void MoveToEnemyPosition()
    {
        if (/*GameManager.instance.playerIsDead ||*/ ai.character.eliminated) { aiManager.debug1 = "1"; return;  }

        else if (aiManager.primaryThreat == null || aiManager.primaryThreat.eliminated)
        {
            aiManager.debug2 = "Threat eliminated";
            ai.Aim(false);
            EndCombatBehaviours();
        }
        else if (!ai.istargetInRange(aiManager.attackRange) || ai.isTargetBehindWall())
        {
            ai.Aim(false);
            aiManager.debug2 = "canNotSeeTarget";
            ai.nav.destination = aiManager.primaryThreat.transform.position;
        }
        else
        {
            aiManager.debug2 = "Spread Out";
            ai.Aim(true);

            if (aiManager.primaryThreat != ai.character.superObject.owner)
            {
                ai.FindCover(true);
            }

            if (aiManager.currentCover)
            {
                SwitchBehaviour(Behaviours.TakeCover);
            }
            else
            {
                ai.nav.destination = ai.m_Transform.position;
                SwitchBehaviour(Behaviours.FindPositionsToSpreadOut);
            }

        }
    }


    private void FindPositionsToSpreadOut()
    {
        if (/*GameManager.instance.playerIsDead ||*/ ai.character.eliminated) { return; }

        else if (aiManager.primaryThreat == null || aiManager.primaryThreat.eliminated)
        {
            EndCombatBehaviours();
        }

        if (ai.istargetInRange(aiManager.attackRange * 1.6f))
        {
            aiManager.debug4 = "isTarget";
            if (ai.isTargetBehindWall())
            {
                aiManager.debug4 = "TargetIsBehindWall";
                ai.Aim(false);
                SwitchBehaviour(Behaviours.MoveToEnemyPosition);
            }

            ai.EliminateEnemy();

            List<Vector3> newPositions = new List<Vector3>();
            Vector3[] dirs = new Vector3[4];
            dirs[0] = aiManager.primaryThreat.transform.right;
            dirs[1] = -aiManager.primaryThreat.transform.right;
            dirs[2] = aiManager.primaryThreat.transform.forward + aiManager.primaryThreat.transform.right;
            dirs[3] = aiManager.primaryThreat.transform.forward - aiManager.primaryThreat.transform.right;

            float maxRayLength = aiManager.attackRange;

            for (int x = 0; x < dirs.Length; x++)
            {
                bool isFreeSpace = !ai.RayCastCheck(aiManager.primaryThreat.transform.position, dirs[x], maxRayLength);

                if (isFreeSpace)
                {
                    aiManager.debug4 = "isFreeSpace";
                    Ray pos = new Ray(aiManager.primaryThreat.transform.position, dirs[x]);
                    Vector3 origin = pos.GetPoint(maxRayLength) + Vector3.up * ai.character.aimPivot.position.y;
                    bool isGoodAssaultPosition = !ai.RayCastCheck(origin, aiManager.primaryThreat.chestTran.position - origin, ai.GetTargetDist());

                    if (isGoodAssaultPosition)
                    {
                        aiManager.debug4 = "IsGoodAssaultPosition";
                        newPositions.Add(pos.GetPoint(maxRayLength));
                    }
                }

                bool finalLoop = x == 3;

                if (finalLoop && newPositions.Count != 0)
                {
                    int randomPos = 0;
                    if (newPositions.Count > 0)
                        randomPos = Random.Range(0, newPositions.Count);

                    aiManager.debug4 = "PositionFound";
                    ai.newAssaultPosition = newPositions[randomPos];
                    newPositions.Clear();
                    SwitchBehaviour(Behaviours.MoveToNewPositions);
                }
            }

        }
        else
        {
            aiManager.debug4 = "TargetNotInRange";
            ai.Aim(false);
            SwitchBehaviour(Behaviours.MoveToEnemyPosition);
        }
    }

    private void FindNewPositions()
    {
        if (/*GameManager.instance.playerIsDead ||*/ ai.character.eliminated) { return; }

        else if (aiManager.primaryThreat == null || aiManager.primaryThreat.eliminated)
        {
            EndCombatBehaviours();
        }

        if (ai.istargetInRange(aiManager.attackRange * 1.6f))
        {
            ai.Aim(true);
            ai.EliminateEnemy();

            if(aiManager.primaryThreat != ai.character.superObject.owner)
            {
                ai.FindCover(false);
            }

            if (aiManager.currentCover)
            {
                SwitchBehaviour(Behaviours.TakeCover);
            }
            if (ai.character.isTakingDamage || switchPositionTimer > 1f)
            {
                if (ai.isTargetBehindWall())
                {
                    ai.Aim(false);
                    SwitchBehaviour(Behaviours.MoveToEnemyPosition);
                }

                SearchBetterFiringSight();

                switchPositionTimer = 0;
            }
            else
            {
                switchPositionTimer += Time.deltaTime;
            }
        }
        else
        {
            ai.Aim(false);
            SwitchBehaviour(Behaviours.MoveToEnemyPosition);
        }
    }

    private void SearchBetterFiringSight()
    {
        List<Vector3> newPositions = new List<Vector3>();
        Vector3[] dirs = new Vector3[] { ai.m_Transform.right, -ai.m_Transform.right };

        float maxRayLength = 4f;

        for (int x = 0; x < dirs.Length; x++)
        {
            bool finalLoop = x == dirs.Length - 1;
            bool isFreeSpace = !ai.RayCastCheck(ai.m_Transform.position, dirs[x], maxRayLength);

            if (isFreeSpace)
            {
                Ray pos = new Ray(ai.m_Transform.position, dirs[x]);

                Vector3 origin = pos.GetPoint(maxRayLength) + Vector3.up * ai.character.aimPivot.position.y;
                Vector3 dir = aiManager.primaryThreat.chestTran.position - origin;

                bool ClearSightOfView = !Physics.Raycast(origin, dir, ai.GetTargetDist(), LayerMask.GetMask("Obstacle", "CoverObject"));
                bool isGoodAssaultPosition;

                if (ClearSightOfView)
                {
                    isGoodAssaultPosition = true;
                }
                else
                {
                    isGoodAssaultPosition = false;
                }


                if (isGoodAssaultPosition)
                {
                    newPositions.Add(pos.GetPoint(maxRayLength));
                }

            }

            if (finalLoop)
            {
                if (newPositions.Count == 0)
                {
                    newPositions.Clear();
                    ai.nav.destination = aiManager.primaryThreat.transform.position;
                }
                else
                {
                    int randomPos = 0;
                    if (newPositions.Count > 0)
                        randomPos = Random.Range(0, newPositions.Count);


                    ai.newAssaultPosition = newPositions[randomPos];


                    newPositions.Clear();
                    SwitchBehaviour(Behaviours.MoveToNewPositions);
                }
            }
        }
    }

    private void MoveToNewPositions()
    {
        if (/*GameManager.instance.playerIsDead ||*/ ai.character.eliminated) { return; }

        else if (aiManager.primaryThreat == null || aiManager.primaryThreat.eliminated)
        {
            EndCombatBehaviours();
        }
        if (ai.istargetInRange(aiManager.attackRange * 1.6f))
        {
            ai.Aim(true);

            float newPositionDistance = ai.GetDist(ai.m_Transform.position, ai.newAssaultPosition);

            if (newPositionDistance < 0.4f || ai.isTravellingTooLong())
            {
                ai.travelTimer = 0;
                SwitchBehaviour(Behaviours.FindNewPositions);
            }
            else
            {
                ai.nav.destination = ai.newAssaultPosition;
            }
        }
        else
        {
            ai.travelTimer = 0;
            ai.Aim(false);
            SwitchBehaviour(Behaviours.MoveToEnemyPosition);
        }


        ai.EliminateEnemy();
    }

    private void TakeCover()
    {
        if (/*GameManager.instance.playerIsDead ||*/ ai.character.eliminated) { return; }

        else if (aiManager.primaryThreat == null || aiManager.primaryThreat.eliminated)
        {
            ai.Aim(false);
            EndCombatBehaviours();
        }

        //Debug.Log("Y1");
        aiManager.s1 = "Y1";

        ai.nav.stoppingDistance = 0;


        if (aiManager.switchPositionTimer > 12f)
        {
            //Debug.Log("Y2");
            aiManager.s1 = "Y2";
            aiManager.switchPositionTimer = 0;
            aiManager.currentCover = null;
            SwitchBehaviour(Behaviours.FindNewPositions);
        }
        else
        {
            //Debug.Log("Y3");
            aiManager.s1 = "Y3";
            aiManager.switchPositionTimer += Time.deltaTime;
            float dist = ai.GetDist(ai.m_Transform.position, aiManager.currentCover.transform.position);
            if (dist < 0.5f)
            {
                aiManager.s1 = "Y4";
                //Debug.Log("Y4");
                ai.nav.isStopped = true;

                OnHoldingCoverPosition();

                SwitchBehaviour(Behaviours.HoldCover);
            }
            else
            {
                //Debug.Log("Y5");
                aiManager.s1 = "Y5";
                ai.nav.destination = aiManager.currentCover.transform.position;
            }
        }

        //Debug.Log("Y6");
        ai.EliminateEnemy();
    }

    private void OnHoldingCoverPosition()
    {
        aiManager.s1 = "Y7";
        aiManager.switchPositionTimer = 0;
        ai.nav.stoppingDistance = 1;
    }

    private void HoldCover()
    {

        if (/*GameManager.instance.playerIsDead ||*/ ai.character.eliminated) { return; }

        else if (aiManager.primaryThreat == null || aiManager.primaryThreat.eliminated)
        {
            aiManager.debug5 = "primaryThreat eliminated";
            EndCombatBehaviours();
        }
        else
        {
            Vector3 origin1 = ai.character.aimPivot.position;
            Vector3 dir1 = aiManager.primaryThreat.chestTran.position - origin1;

            bool canNotSee = Physics.Raycast(origin1, dir1, ai.GetTargetDist(), LayerMask.GetMask("Obstacle"));
            if (canNotSee)
            {
                aiManager.debug5 = "canNotSee";
                OnLeavingCoverPosition();
                SwitchBehaviour(Behaviours.MoveToEnemyPosition);
            }
            if (coverHoldingTimer > 0)
            {
                aiManager.debug5 = "counting";
                coverHoldingTimer -= Time.deltaTime;

                ai.ShootFromCover();
            }
            else
            {
                aiManager.debug5 = "FindNewPositions";
                coverHoldingTimer = 3f;
                OnLeavingCoverPosition();
                SwitchBehaviour(Behaviours.FindNewPositions);
            }
        }
    }

    private void OnLeavingCoverPosition()
    {
        aiManager.currentCover.preoccupied = false;
        aiManager.currentCover = null;
        coverHoldingTimer = 4f;
        ai.nav.stoppingDistance = 1f;
    }

    private void EndCombatBehaviours()
    {
        ai.Aim(false);
        ai.character.isCrouching = false;

        aiManager.primaryThreat = ai.ScanThreats();
              
        if(aiManager.primaryThreat == null)
        {
            aiManager.debug3 = "no primary threat";
            if (ai.isLeader())
            {
                aiManager.debug3 = "isLeader";
                ai.character.Regroup();
                leaderBehaviourSystem.SwitchBehaviour(AI_TeamLeader_Behaviour_System.Behaviours.SearchTarget);
            }
            else
            {
                aiManager.debug3 = "isFollower";
                followerBehaviourSystem.SwitchBehaviour(AI_Folower_Behaviour_System.Behaviours.FollowLeader);
            }
        }
        else
        {
            aiManager.debug3 = "Detecting Threat" + "  " + aiManager.primaryThreat;          
        }


    }
}
