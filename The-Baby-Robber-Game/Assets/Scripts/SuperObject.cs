using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SuperObject : MonoBehaviour
{
    private NavMeshAgent m_agent;
    public NavMeshAgent agent
    {
        get
        {
            if (m_agent == null)
                m_agent = GetComponent<NavMeshAgent>();

            return m_agent;
        }
    }

    private Target m_targetIndicator;
    public Target targetIndicator
    {
        get
        {
            if (m_targetIndicator == null)
                m_targetIndicator = GetComponent<Target>();

            return m_targetIndicator;
        }
    }

    public Character owner;
    public Team ownerTeam;

    public Vector3 parentedPosition;
    public Vector3 parentedRotation;

    public delegate void ChaseTheTeamThatHasSuperObject();
    public ChaseTheTeamThatHasSuperObject chaseTheTeamThatHasSuperObject;

    private Vector3 moveDirection = new Vector3();
    private float rotationTimer;
    public float Timer = 1f;
    public float moveSpeed;
    public float angularSpeed;

    private void Start()
    {
        if(m_agent == null)
            m_agent = GetComponent<NavMeshAgent>();

        moveSpeed = GameManager.instance.superObjectMoveSpeed;
        angularSpeed = GameManager.instance.superObjectAngularSpeed;

        m_agent.speed = moveSpeed;
        m_agent.angularSpeed = angularSpeed;
    }

    private void Update()
    {
        if(m_agent && m_agent.isOnNavMesh)
        {
            if(Timer > 0)
            {
                m_agent.SetDestination(moveDirection);
                Timer -= Time.deltaTime;
            }
            else
            {
                moveDirection = AI.RandomNavmeshArea(transform.position, 30, LayerMask.GetMask("Default"));
                Timer = 1f;
            }
        }

        if(owner != null && GameManager.instance.StartGame && GameManager.instance.gameMode == GameManager.GameMode.CatchBabyAndDefendYourself)
        {
            GameManager.instance.CountDown();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9 && owner == null)
        {
            Character c = other.GetComponent<Character>();

            if (c.teamUnitType == Character.TeamUnitType.TeamLeader && !c.eliminated)
            {
                c.PickableObject(true);
                c.CheckNearByObjects(true);
                chaseTheTeamThatHasSuperObject?.Invoke();
            }

        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 9 && owner == null)
        {
            Character c = other.GetComponent<Character>();

            if (c.teamUnitType == Character.TeamUnitType.TeamLeader && !c.eliminated)
            {
                c.PickableObject(false);
            }
        }
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public float GetNavmeshSpeed()
    {
        return m_agent.desiredVelocity.magnitude;
    }

    public void MoveToParentedPositionAndRotation()
    {
        transform.localPosition = parentedPosition;
        transform.localEulerAngles = parentedRotation;
    }

    public void RemoveFromDeathPlayer()
    {
        if (!targetIndicator.enabled)
            targetIndicator.enabled = true;

        agent.enabled = true;
        ResetRotation();
        transform.SetParent(null);
        agent.Warp(transform.position);
        owner = null;
        ownerTeam = null;
    }
}
