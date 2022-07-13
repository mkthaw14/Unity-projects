using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Folower_Behaviour_System 
{
    public AI ai;
    public AIManager aiManager;

    AI_Combat_Behaviour_System combatSystem;

    [SerializeField]
    public Behaviours behaviours;

    Behaviours previousBehaviours = Behaviours.FollowLeader;

    [System.Serializable]
    public enum Behaviours
    {
        FollowLeader,
        ReadyToCombatEnemy,
        GetAwayFromOtherPlayerPath
    }

    public AI_Folower_Behaviour_System(AI ai, AIManager aIManager, AI_Combat_Behaviour_System combatSystem)
    {
        this.ai = ai;
        this.aiManager = aIManager;
        this.combatSystem = combatSystem;
    }

    public void UpdateBehaviours(AI_Folower_Behaviour_System followerSystem)
    {
        if (followerSystem == null)
            return;

        //if(ai.GetTeamLeader().controlType == Character.ControlType.ComputerControlled)
        //{
        //    if(ai.GetTeamLeader().AIPlayer.leaderSystem.behaviours == AI_TeamLeader_Behaviour_System.Behaviours.ReadyToCombatEnemy)
        //    {
        //        aiManager.GetTarget(ai.GetTeamLeader().AIPlayer.aiManager.primaryThreat);
        //        SwitchBehaviour(Behaviours.ReadyToCombatEnemy);
        //    }
        //}

        switch (behaviours)
        {
            case Behaviours.FollowLeader:
                FollowLeader();
                break;
            case Behaviours.ReadyToCombatEnemy:
                ReadyToCombatEnemy();
                break;
            case Behaviours.GetAwayFromOtherPlayerPath:
                GetAwayFromOtherPlayerPath();
                break;
        }

        ai.character.AI_CurrentBehaviourState = behaviours.ToString();
    }

    public void SwitchBehaviour(Behaviours behaviours)
    {
        if(behaviours == Behaviours.ReadyToCombatEnemy)
        {
            combatSystem.SwitchBehaviour(AI_Combat_Behaviour_System.Behaviours.MoveToEnemyPosition);
        }
        else if(behaviours == Behaviours.GetAwayFromOtherPlayerPath)
        {
            previousBehaviours = this.behaviours;
        }

        this.behaviours = behaviours;
    }

    public void FollowLeader()
    {
        ai.character.aim = false;
        float dist = Vector3.Distance(ai.m_Transform.position, ai.GetTeamLeader().transform.position);


        if (ai.character.isTakingDamage)
        {
            //Debug.Break();
            SwitchBehaviour(Behaviours.ReadyToCombatEnemy);
            ai.character.AlertAllTeamUnit(aiManager.primaryThreat);
        }

        if (dist > 4f)
        {
            ai.nav.destination = ai.GetTeamLeader().transform.position;
        }
        else
        {
            ai.nav.destination = ai.m_Transform.position;
        }
    }

    public void ReadyToCombatEnemy()
    {
        combatSystem.UpdateBehaviours();
    }

    public float movingAwayTimer = 0.3f;
    public void GetAwayFromOtherPlayerPath()
    {
        float dist = Vector3.Distance(ai.m_Transform.position, ai.GetTeamLeader().transform.position);
        if(dist < 3)
        {
            Vector3 dir = ai.m_Transform.position - ai.GetTeamLeader().transform.position;
            ai.nav.destination = ai.m_Transform.position + dir;
        }
        else
        {
            SwitchBehaviour(Behaviours.FollowLeader);
        }

    }
}
