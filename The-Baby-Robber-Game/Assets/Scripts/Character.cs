using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : TeamFaction
{
    public CharacterMoveValues mV;
    [HideInInspector]
    public CharacterIK charIK;
    [HideInInspector]
    public WeaponManager wepMan;
    [HideInInspector]
    public Transform chestTran;
    [HideInInspector]
    public Transform aimPivot;

    public Vector3 aimPos;

    public Vector3 spineLookPos;
    [HideInInspector]
    public HealthBar hb;

    public LayerMask Target;

    public string AI_CurrentBehaviourState, AI_CombatBehaviourState;

    public bool isCrouching;
    public bool aim;
    public bool DebugAim;
    public bool reloading;
    public bool weaponSwitch;
    public bool eliminated;
    public bool isTakingDamage = false;
    public bool isMoving;
    public bool canFireWeapon = false;
    public bool targetInReticle = false;
    public bool recoilIsInit = false;
    public bool nearTheObject = false;
    public bool noAmmoLeft = false;

    public int animID;
    public int Health;
    public int FullHealth;
    public float shootingTimer, airstrikeTimer;

    public WeaponPickUpItem[] items;

    [SerializeField]
    public TeamUnitType teamUnitType;

    [System.Serializable]
    public enum TeamUnitType
    {
        TeamUnit, TeamLeader
    }

    public Player humanPlayer;
    public AI AIPlayer;

    private Animator anim;
    private CharacterController characterController;
    private NavMeshAgent navAgent;
    private NavMeshObstacle navMeshObstacle;
    private Rigidbody[] rigids;
    private Collider bodyCollider;
    private CharacterJoint[] characterjoint;
    private CapsuleCollider cap;
    private Transform healthBar;
    private float recoilT;


    [SerializeField]
    public ControlType controlType;

    [System.Serializable]
    public enum ControlType
    {
        ComputerControlled, HumanControlled
    }

    [SerializeField]
    public WeaponSkill weapSkill;
    [System.Serializable]
    public class WeaponSkill
    {
        public int weaponDMG;
        public float spreadX;
        public float spreadY;
    }


    public void EnableDisableRagdoll(bool value)
    {
        if (!value)
        {
            characterController.enabled = false;
            bodyCollider.enabled = false;
        }
        else
        {
            for (int y = 0; y < characterjoint.Length; y++)
            {
                characterjoint[y].enableProjection = true;
                characterjoint[y].enablePreprocessing = true;
            }
        }

        for (int i = 0; i < rigids.Length; i++)
        {
            if(rigids[i].gameObject.layer == 10)
            {
                rigids[i].maxDepenetrationVelocity = 0.01f;
                rigids[i].isKinematic = value;
                rigids[i].collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }
    }

    public void SwitchTeamUnitType(TeamUnitType teamUnitType)
    {
        this.teamUnitType = teamUnitType;
    }

    public void SwitchControlType(ControlType controlType)
    {
        if (controlType == ControlType.ComputerControlled)
        {
            characterController.enabled = false;
            navMeshObstacle.enabled = false;
 
            if(AIPlayer == null)
            {
                AIPlayer = new AI(this, transform, navAgent, wepMan);
            }
        }
        else
        {
            characterController.enabled = true;
            navMeshObstacle.enabled = true;
            navAgent.enabled = false;

            if (humanPlayer == null)
            {
                humanPlayer = new Player(this, characterController, transform, wepMan);
                //Debug.Log(gameObject.name + " " + humanPlayer);
            }
        }

        wepMan.SetUpAmmo(controlType);
        this.controlType = controlType;
    }

    public void AlertAllTeamUnit(Character enemy)
    {
        for (int x = 1; x < team.units.Count; x++) // x starts from 1 by excluding leader
        {
            team.units[x].AIPlayer.aiManager.primaryThreat = enemy;
            team.units[x].AIPlayer.followerSystem.SwitchBehaviour(AI_Folower_Behaviour_System.Behaviours.ReadyToCombatEnemy);
        }
    }

    public void Regroup()
    {
        for(int x = 1; x < team.units.Count; x++)
        {
            team.units[x].aim = false;
            team.units[x].AIPlayer.followerSystem.SwitchBehaviour(AI_Folower_Behaviour_System.Behaviours.FollowLeader);
        }
    }

    public void SetUp(ControlType controlType, TeamUnitType teamUnitType, SuperObject superObject)
    {
        unitType = UnitType.Humanoid;
        characterController = GetComponent<CharacterController>();
        navAgent = GetComponent<NavMeshAgent>();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        navAgent.updateRotation = true;

        charIK = GetComponentInChildren<CharacterIK>();
        wepMan = GetComponent<WeaponManager>();
       
        anim = GetComponentInChildren<Animator>();
        rigids = transform.GetChild(0).GetComponentsInChildren<Rigidbody>();
        characterjoint = transform.GetChild(0).GetComponentsInChildren<CharacterJoint>();
        bodyCollider = GetComponent<Collider>();
        cap = GetComponent<CapsuleCollider>();
        cap.enabled = true;
        cap.isTrigger = true;

        EnableDisableRagdoll(true);

        charIK.SetUp(this, wepMan);

        wepMan.hitObjects = LayerMask.GetMask("Obstacle", "CoverObject", "Ragdoll");
        hb = GetComponentInChildren<HealthBar>();
        healthBar = hb.transform.parent;
        healthBar.parent.gameObject.SetActive(false);
        this.superObject = superObject;

        SwitchTeamUnitType(teamUnitType);
        SwitchControlType(controlType); 
    }


    public void Tick()
    {
        CharacterLogic();
    }


    public void HandleAnimation(float x, float y, float moveAmount, float d, bool strafingLogic)
    {
        float v = 0;
        float h = 0;

        float dampTime = 0;

        if (strafingLogic) // The logic for strafing 
        {
            v = y;
            h = x;

            dampTime = 0.2f;
        }
        else
        {
            v = moveAmount;
            dampTime = 0.15f;
        }

        anim.SetFloat(StaticString.Y, v, dampTime, d);
        anim.SetFloat(StaticString.X, h, dampTime, d);

        anim.SetBool(StaticString.aim, aim);
        anim.SetBool(StaticString.reload, reloading);
        anim.SetBool(StaticString.crouch, isCrouching);
        anim.SetInteger(StaticString.switchWeap, animID);
    }

    public void Death()
    {
        if(!eliminated)
        {
            Health = 0;
            eliminated = true;
        }

        if (superObject.owner == this)
        {
            superObject.RemoveFromDeathPlayer();
            superObject = null;
            GameManager.instance.TeamHasSuperObject = null;

            if (GameManager.instance.gameMode == GameManager.GameMode.CatchBabyAndDefendYourself)
            {
                UIManager.instance.HUD_handler.SetTimePanel(false);
            }
            if(GameManager.instance.MainPlayer == this)
            {
                UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext( 1, team.teamName);
            }
        }

        DisableAll();
        ActivateRagdoll();

        UIManager.instance.HUD_handler.UpdateHealthBar(this.hb, this.Health, this.FullHealth);
        team.RemoveFromTeam(this);

        if (controlType == ControlType.ComputerControlled)
            AIPlayer.Death();
        else
            humanPlayer.Death();

        Destroy(gameObject, 12f);
    }

    public void CreateNewTeamLeader()
    {
        if (teamUnitType == TeamUnitType.TeamLeader)
        {
            Character newLeader = team.units[0];
            newLeader.team.teamLeader = newLeader;

            if(controlType == ControlType.ComputerControlled) //If current leader is computer controlled
            {
                newLeader.AIPlayer.CreateLeaderOrFollower(TeamUnitType.TeamLeader, true);
                AIPlayer.leaderSystem = null;
            }
            else
            {
                GameManager.instance.MainPlayer = newLeader;
                GameManager.instance.MainPlayer.SwitchControlType(ControlType.HumanControlled);
                GameManager.instance.cameraHolder.target = newLeader.humanPlayer;
                UIManager.instance.HUD_handler.UpdateHealthBar(newLeader.hb, newLeader.Health, newLeader.FullHealth);
                newLeader.teamUnitType = TeamUnitType.TeamLeader;
            }

            //Debug.Log(team.units.Count + " " + team.teamName);
        }
    }

    private void DisableAll()
    {
        bodyCollider.enabled = false;        
        wepMan.DisableWeapon();
    }

    public void ActivateRagdoll()
    {
        anim.enabled = false;
        EnableDisableRagdoll(false);    
    }

    public void TakeDamage(int weaponDamage, TeamFaction attacker, HitPosition hit)
    {
        if (eliminated)
            return;

        Character humanoidAttacker;
        bool isHumanPlayer = this == GameManager.instance.MainPlayer;
        hit.CalculateDamagePoint(ref Health, FullHealth, weaponDamage, isHumanPlayer);

        UIManager.instance.HUD_handler.UpdateHealthBar(this.hb, this.Health, this.FullHealth);

        if(attacker.unitType == UnitType.Humanoid)
        {
            humanoidAttacker = attacker.humanoidCharacter;
            AlertAllTeamUnit(humanoidAttacker);

            if (controlType == ControlType.ComputerControlled)
                AIPlayer.TakeDamage(humanoidAttacker);
            else
                humanPlayer.TakeDamage(humanoidAttacker);
        }
        if (this.Health <= 0)
        {
            eliminated = true;

            if(attacker.unitType == UnitType.Humanoid)
                attacker.team.AddKillCount();

            Death();
        }
        else if (!isTakingDamage)
        {
            isTakingDamage = true;
        }
    }

    public void ReloadWeapon(bool reloadingLogic)
    {
        if (reloadingLogic)
        {
            if (reloading)
            {
                StartCoroutine(StopReloading());
                return;
            }
        }
    }

    private IEnumerator StopReloading()
    {
        yield return new WaitForSeconds(3f);

        if (wepMan.currentWeapon != null) { wepMan.currentWeapon.Reload(ref reloading); }

    }

    public void SwitchWeapon(bool value)
    {
        if (value)
        {
            weaponSwitch = value;
            wepMan.ChangeWeapon();
            StartCoroutine(StopSwitchingWeapon());
            return;
        }
    }

    private IEnumerator StopSwitchingWeapon()
    {
        yield return new WaitForSeconds(0.8f);
        weaponSwitch = false;
    }

    public Vector3 firingDirection;

    public void AimPosition(Transform sourceTransform, Vector3 aimingDirection)
    {
        Ray aimHelper_ray = new Ray(sourceTransform.position, aimingDirection);
        aimPos = aimHelper_ray.GetPoint(60);
        if(this == GameManager.instance.MainPlayer && !aimPos.Equals(aimHelper_ray.GetPoint(60)))
        {
            Debug.Log("Equal");
            //Debug.Break();
        }

        RaycastHit hit;

        firingDirection = aimingDirection;
        canFireWeapon = true;

        Debug.DrawRay(sourceTransform.position, aimingDirection * 100, Color.yellow);


        bool hitObject = Physics.Raycast(aimHelper_ray, out hit, 100f, LayerMask.GetMask("CoverObject", "Obstacle","Ragdoll"));
        if (hitObject)
        {
            if (hit.collider.gameObject.layer == 10 && hit.collider.GetComponentInParent<Character>() == this)
            {
                return;
            }

            float dist = Vector3.Distance(sourceTransform.position, hit.point);

            if (dist > 2f)
            {
                if(hit.collider.gameObject.layer == 10)
                {
                    targetInReticle = true;
                    aimPos = hit.point;

                    if(!wepMan.IsSniper())
                        firingDirection = (hit.point - wepMan.currentWeapon.firePoint.position).normalized;
                }
            }
            else
            {
                targetInReticle = false;
                canFireWeapon = false;
                aimPos = aimHelper_ray.GetPoint(10) + Vector3.up * (-8f);
            }
            
        }

        Vector3 lookDirection = aimingDirection + sourceTransform.up * (-0.2f);
        Ray spineLookHelper_ray = new Ray(sourceTransform.position, lookDirection);

        spineLookPos = spineLookHelper_ray.GetPoint(30);

        Debug.DrawRay(sourceTransform.position, lookDirection * 100, Color.red);

        if (this == GameManager.instance.MainPlayer && !spineLookPos.Equals(spineLookHelper_ray.GetPoint(30)))
        {
            Debug.Log("Equal SPine");
            //Debug.Break();
        }
    }


    void CharacterLogic()
    {
        if (eliminated)
            return;

        if(controlType == ControlType.ComputerControlled)
        {
            AIPlayer.Tick(Time.deltaTime);
        }
        else
        {
            humanPlayer.Tick(Time.deltaTime);
        }
    }

    public bool isPushing;
    public WeaponPickUpItem newWeaponItem;
    public Transform pushingCharacter;
    public Character hitCharacter;

    public void SomeoneIsPushing()
    {      
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * 1, transform.forward);
        bool isPushing = Physics.SphereCast(ray, 1F, out hit, 1F, LayerMask.GetMask("Target"));
        //Debug.Break();

        Debug.DrawRay(transform.position + Vector3.up * 1, transform.forward * 1, Color.blue);
        //if (hit.collider)
        //{
        //    Debug.Log(hit.collider.name);
        //}

        if (isPushing)
        {
            hitCharacter = hit.transform.GetComponent<Character>();

            if (hitCharacter.teamUnitType == TeamUnitType.TeamLeader) return;
            else if (hitCharacter.faction != faction) return;
            else if (hitCharacter.controlType == ControlType.HumanControlled) return;
            else if (hitCharacter.AIPlayer.followerSystem.behaviours == AI_Folower_Behaviour_System.Behaviours.ReadyToCombatEnemy) return;
            else 
            {
                pushingCharacter = hitCharacter.transform;
                hitCharacter.AIPlayer.followerSystem.SwitchBehaviour(AI_Folower_Behaviour_System.Behaviours.GetAwayFromOtherPlayerPath);
            }
        }
    }

    public void CheckNearByObjects(bool pickUp)
    {
        if(nearTheObject && superObject != null && pickUp)
        {
            if(this == GameManager.instance.MainPlayer)
                superObject.targetIndicator.enabled = false;

            superObject.transform.SetParent(charIK.Spine);
            superObject.MoveToParentedPositionAndRotation();
            superObject.owner = this;
            superObject.ownerTeam = team;
            GameManager.instance.TeamHasSuperObject = team;
        }
    }

    public SuperObject superObject;
    public void PickableObject(bool value)
    {
        nearTheObject = value;
        superObject.agent.enabled = false;

        if(GameManager.instance.gameMode == GameManager.GameMode.CatchBabyAndDefendYourself)
        {
            if (this == GameManager.instance.MainPlayer)
                UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext(2, team.teamName);
            else
                UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext(4, team.teamName);

            GameManager.instance.ResetCountDownTimer();
            UIManager.instance.HUD_handler.SetTimePanel(true);
        }
        else
        {
            if (this == GameManager.instance.MainPlayer)
                UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext(3, team.teamName);
            else
                UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext(4, team.teamName);
        }
    }

    public void PickableWeapon(WeaponPickUpItem newWeaponItem, bool value)
    {
        if (eliminated) return;
        this.newWeaponItem = newWeaponItem;
        nearTheObject = value;
    }

    private Transform rightHand;

    private Vector3 baseRightHandPos;
    private Vector3 offsetRightHandPos;
    private Vector3 baseRightHandRot;
    private Vector3 offsetRightHandRot;

    private float CurveTimeZ;
    private float CurveTimeY;


    public void SetUpHandIK(Transform rightH, Vector3 rightHandPos, Vector3 rightHandRot)
    {
        rightHand = rightH;
        rightHand.localPosition = rightHandPos;
        rightHand.localEulerAngles = rightHandRot;
        baseRightHandPos = rightHandPos;
        baseRightHandRot = rightHandRot;
    }



    public void StartRecoil() // Must be put in shoot function
    {
        if (!recoilIsInit)
        {
            recoilIsInit = true;
            recoilT = 0;
        }
    }



    public void UpdateCharacterRecoil(float d) // Must be put in LateUpdate function
    {
        if (recoilIsInit)
        {
            float speed = wepMan.currentWeapon.weapPro.weapSetting.recoilSpeed;
            recoilT += d * speed;
            if (recoilT > 2)
            {
                recoilT = 2f;
                recoilIsInit = false;
            }

            WeaponRecoil();
        }
    }

    private void WeaponRecoil()
    {
        if (!GameManager.instance.StartGame)
        {
            rightHand.localEulerAngles = baseRightHandRot;
        }
        else
        {
            CurveTimeZ = wepMan.currentWepPro.recoilZ.Evaluate(recoilT); // float variable
            CurveTimeY = wepMan.currentWepPro.recoilY.Evaluate(recoilT); // float variable


            offsetRightHandPos = Vector3.forward * 3 * CurveTimeZ;

            rightHand.localPosition = baseRightHandPos + offsetRightHandPos;

            if (wepMan.currentWeapon.weaps == Weapon.weaponType.AssaultRifle)
                offsetRightHandRot = RandomRecoil() * 180 * CurveTimeY;
            else
                offsetRightHandRot = Vector3.right * 180 * CurveTimeY;

            rightHand.localEulerAngles = baseRightHandRot + offsetRightHandRot;

            ///THERE WILL BE TWO RECOIL FUNCTIONS FROM PLAYER AND AI CLASSES

            if (controlType == ControlType.HumanControlled)
            {
                humanPlayer.WeaponRecoil(recoilT);
            }
        }
    }




    public void CheckCharacterMotion(float moveAmount) 
    {
        if (moveAmount != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    private Vector3 RandomRecoil()
    {
        int random = Random.Range(0, 7);
        Vector3 v = new Vector3();
        switch (random)
        {
            case 0:
                v = Vector3.forward;
                break;
            case 2:
                v = -Vector3.forward;
                break;
            case 3:
                v = Vector3.right;
                break;
            case 4:
                v = -Vector3.right;
                break;
            case 5:
                v = Vector3.up;
                break;
            case 6:
                v = -Vector3.up;
                break;
        }

        return v;

    }
}


