using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_TeamLeader_Behaviour_System
{
    public AI ai;
    public AIManager aiManager;
    public SuperObject superObject;

    private AI_Combat_Behaviour_System combatSystem;

    float randomLocationTimer = 1f;
    List<Character> otherLeaders = new List<Character>();

    [System.Serializable]
    public enum Behaviours
    {
        SearchTarget,
        GoToTarget,
        RunAway,
        ReadyToCombatEnemy
    }

    [SerializeField]
    public Behaviours behaviours;
    Behaviours previousBehaviour = Behaviours.SearchTarget;

    public AI_TeamLeader_Behaviour_System(AI ai, AIManager aiManager, SuperObject superObject, AI_Combat_Behaviour_System combatSystem)
    {
        this.ai = ai;
        this.aiManager = aiManager;
        this.superObject = superObject;
        this.combatSystem = combatSystem;
        this.superObject.chaseTheTeamThatHasSuperObject += ChaseTheSuperObectHolderTeam;
    }


    public void UpdateBehaviour(AI_TeamLeader_Behaviour_System leaderSystem)
    {
        if (leaderSystem == null)
            return;

        switch (behaviours)
        {
            case Behaviours.SearchTarget:
                SearchTarget();
                break;
            case Behaviours.GoToTarget:
                GoToTarget();
                break;
            case Behaviours.RunAway:
                RunAway();
                break;
            case Behaviours.ReadyToCombatEnemy:
                ReadyToCombatEnemy();
                ai.CallAirStrike();
                break;
        }

        ai.character.AI_CurrentBehaviourState = behaviours.ToString();
    }

    public void SwitchBehaviour(Behaviours behaviours)
    {
        if (behaviours == Behaviours.ReadyToCombatEnemy)
        {
            //Debug.Break();
            combatSystem.SwitchBehaviour(AI_Combat_Behaviour_System.Behaviours.MoveToEnemyPosition);
            ai.character.AlertAllTeamUnit(aiManager.primaryThreat);
        }

        this.behaviours = behaviours;
    }

    private void SearchTarget()
    {
        SwitchBehaviour(Behaviours.GoToTarget);
    }

    private void GoToTarget()
    {
        ai.character.aim = false;
        float dist = Vector3.Distance(ai.m_Transform.position, ai.character.superObject.transform.position);

        if(dist < 15)
        {
            aiManager.primaryThreat = ai.ScanThreats();

            if (aiManager.primaryThreat)
            {
                //Debug.Break();
                //aiManager.debug1 = "Yes there's a primary threat";
                SwitchBehaviour(Behaviours.ReadyToCombatEnemy);
            }
        }
        if (ai.character.superObject.owner != ai.character)
        {
            //aiManager.debug1 = "Hello";
            ai.nav.destination = ai.character.superObject.transform.position;
        }
        else
        {
            //aiManager.debug1 = "Zopo";
            ai.Aim(false);
            SwitchBehaviour(Behaviours.RunAway);
        }

        ai.character.SomeoneIsPushing();
    }

    Vector3 newPosition = new Vector3();

    private void RunAway()
    {
        if (GettingCloseToOtherPlayers(40) )
        {
            if (ai.character.isTakingDamage || GettingCloseToOtherPlayers(30))
            {
                SwitchBehaviour(Behaviours.ReadyToCombatEnemy);
            }

            for (int x = 0; x < GetOtherEnemyLeaders().Length; x++)
            {
                Vector3 dir = ai.m_Transform.position - GetOtherEnemyLeaders()[x].transform.position;
                aiManager.fleePosition = ai.m_Transform.position + dir;
            }

            ai.nav.destination = aiManager.fleePosition;

        }
        else
        {
            if (randomLocationTimer > 0)
            {
                ai.nav.SetDestination(newPosition);
                randomLocationTimer -= Time.deltaTime;
            }
            else
            {
                newPosition = GameManager.instance.spawnManager[Random.Range(0, GameManager.instance.spawnManager.Count)].transform.position;
                randomLocationTimer = 1f;
            }
        }

        ai.character.SomeoneIsPushing();
    }


    private bool GettingCloseToOtherPlayers(float maxDist)
    {
        bool val = false;
        //float maxDist = float.MaxValue;

        for (int x = 0; x < GetOtherEnemyLeaders().Length; x++)
        {
            float dist = Vector3.Distance(ai.m_Transform.position, GetOtherEnemyLeaders()[x].transform.position);
            if(dist < maxDist)
            {
                val = true;
                break;
            }
        }

        return val;
    }

    Transform startTransform;
    float multplyBy = 1;

    private void RunFrom()
    {
        startTransform = ai.m_Transform;

        ai.m_Transform.rotation = Quaternion.LookRotation(ai.m_Transform.position - GetOtherEnemyLeaders()[0].transform.position);

        Vector3 runTo = ai.m_Transform.position + ai.m_Transform.forward * multplyBy;

        NavMeshHit navHit;

        NavMesh.SamplePosition(runTo, out navHit, 20, -1);

        ai.m_Transform.position = startTransform.position;
        ai.m_Transform.rotation = startTransform.rotation;

        ai.nav.SetDestination(navHit.position);
    }

    //private List<Character> GetOtherEnemyLeaders()
    //{
    //    if(otherLeaders.Count == 0)
    //    {
    //        for (int x = 0; x < GameManager.instance.allTeams.Count; x++)
    //        {
    //            if (GameManager.instance.allTeams[x].units.Count != 0)
    //            {
    //                if (GameManager.instance.allTeams[x] == ai.character.team)
    //                {
    //                    continue;
    //                }

    //                otherLeaders.Add(GameManager.instance.allTeams[x].teamLeader);
    //            }
    //        }
    //    }


    //    return otherLeaders;
    //}

    private Character[] GetOtherEnemyLeaders()
    {
        Character[] c = new Character[ai.character.team.otherTeams.Count];

        for(int x = 0; x < ai.character.team.otherTeams.Count; x++)
        {
            c[x] = ai.character.team.otherTeams[x].teamLeader;
        }

        return c;
    }

    private void ReadyToCombatEnemy()
    {
        combatSystem.UpdateBehaviours();
    }

    private void ChaseTheSuperObectHolderTeam()
    {
        SwitchBehaviour(Behaviours.SearchTarget);
        ai.character.Regroup();
    }

    public void UnSubscribeDelegate()
    {
        superObject.chaseTheTeamThatHasSuperObject -= ChaseTheSuperObectHolderTeam;
    }

    public float movingAwayTimer = 0.3f;
    public void GetAwayFromOtherPlayerPath()
    {
        if (movingAwayTimer > 0)
        {
            movingAwayTimer -= Time.deltaTime;
            Vector3 dir = ai.m_Transform.position - ai.character.pushingCharacter.position;
            ai.nav.destination = ai.m_Transform.position + dir;
        }
        else
        {
            movingAwayTimer = 0.3f;
            SwitchBehaviour(previousBehaviour);
        }

    }
}
