using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI 
{
    public Character character;
    public Transform m_Transform;
    public WeaponPickUpItem weaponToPickUp;

    public NavMeshAgent nav;
    public Character targetedCharacter;
    public Vector3 attackZone;
    public Vector3 newAssaultPosition;

    public AI_Combat_Behaviour_System combatSystem;
    public AI_TeamLeader_Behaviour_System leaderSystem;
    public AI_Folower_Behaviour_System followerSystem;
    public AIManager aiManager;

    public LayerMask cover;
    public float travelTimer;
    public bool lockTarget;
    public bool isStopped;

    private Transform firePoint;
    private WeaponManager wepMan;

    public int numOfShot, shootLimit;

    float moveSpeed, moveSpeedWhileAiming;

    public AI(Character character, Transform _currentTransform, NavMeshAgent agent, WeaponManager weaponManager)
    {
        //WARING ONLY REGISTER THE NEWUNIT FROM GAMEMANAGER CLASS
        this.character = character;
        m_Transform = _currentTransform;

        character.Health = 100;
        character.FullHealth = character.Health;
        nav = agent;
        aiManager = character.GetComponent<AIManager>();
        aiManager.SetUp(character, this);

        moveSpeed = GameManager.instance.ai_moveSpeed;
        moveSpeedWhileAiming = GameManager.instance.ai_moveSpeedWhileAiming;

        nav.speed = moveSpeed;
        nav.stoppingDistance = 1;
        wepMan = weaponManager;

        if (wepMan.IsSniper())
            aiManager.attackRange = 40;
        else
            aiManager.attackRange = 30;

        wepMan.shootWeapon += ShootCount;
        obstaclesLayer = LayerMask.GetMask("Obstacle", "CoverObject");

        combatSystem = new AI_Combat_Behaviour_System();

        CreateLeaderOrFollower(character.teamUnitType, false);
        //WARING ONLY REGISTER THE NEWUNIT FROM GAMEMANAGER CLASS
    }

    public static Vector3 RandomNavmeshArea(Vector3 origin, float distance, int layerMask)
    {
        Vector3 randomArea = Random.insideUnitSphere * distance;
        randomArea += origin;

        NavMeshHit hit;

        NavMesh.SamplePosition(randomArea, out hit, distance, layerMask);

        return hit.position;
    }

    public void CreateLeaderOrFollower(Character.TeamUnitType teamUnitType, bool switchUnitType)
    {
        if (switchUnitType)
        {
            character.SwitchTeamUnitType(teamUnitType);
        }
        if(teamUnitType == Character.TeamUnitType.TeamLeader)
        {
            if(leaderSystem == null)
            {
                leaderSystem = new AI_TeamLeader_Behaviour_System(this, aiManager, character.superObject, combatSystem);
            }    
        }
        else
        {
            if(followerSystem == null)
            {
                followerSystem = new AI_Folower_Behaviour_System(this, aiManager, combatSystem);
            }
        }

        combatSystem.SetUp(this, aiManager, leaderSystem, followerSystem);
    }

    public bool isLeader()
    {
        return character.teamUnitType == Character.TeamUnitType.TeamLeader;
    }

    void UpdateBehaviours()
    {
        if(character.teamUnitType == Character.TeamUnitType.TeamLeader)
        {
            leaderSystem.UpdateBehaviour(leaderSystem);
        }
        else
        {
            followerSystem.UpdateBehaviours(followerSystem);
        }
    }



    public void Tick(float d)
    {
        character.AI_CombatBehaviourState = "";
        character.AI_CurrentBehaviourState = "";

        if (!nav || !nav.isOnNavMesh) return;

        CheckAiming();
        UpdateRotation();
        HandleMovementSpeed();
        character.CheckCharacterMotion(GetMagnitude);
        UpdateBehaviours();
        HandleNavMeshAnimation(d);
    }

    void UpdateRotation()
    {
        nav.updateRotation = !character.aim;
    }

    void CheckAiming()
    {
        if (character.aim)
        {
            AimState();
        }
    }
    float GetMagnitude
    {
        get
        {
            float magnitude = Mathf.Clamp01(nav.velocity.magnitude);
            return magnitude;
        }
    }

    void HandleMovementSpeed()
    {
        nav.speed = character.aim ? moveSpeedWhileAiming : moveSpeed; 
    }

    void HandleNavMeshAnimation(float d)
    {
        Vector3 relative = m_Transform.InverseTransformDirection(nav.desiredVelocity);
        float forward = relative.z;
        float strafe = relative.x;

        bool strafeLogic = character.aim;

        character.mV.moveAmount = nav.velocity.magnitude;

        character.HandleAnimation(strafe, forward, character.mV.moveAmount, d, strafeLogic);
    }

    public Character GetTeamLeader()
    {
        return character.team.teamLeader;
    }

    public LayerMask visiableLayer;
    public float waitForShooting;

    public bool CanKillTarget()
    {
        bool value = false;

        if (wepMan.EmptyWeapon())
        {
            if (NoBackUpAmmo())
            {
                if (wepMan.GetAnyWeaponWithAmmo())
                {
                    character.SwitchWeapon(true);
                    value = true;
                }
                else
                {
                    value = false;
                }
            }
            else
            {
                Reload();
            }
        }
        else
        {
            value = true;
        }

        return value;
    }

    public void EliminateEnemy()
    {
        if (!character.aim || character.eliminated)
            return;

        FireWeapon();
    }

    private bool NoBackUpAmmo()
    {
        bool val = false;

        if (wepMan.currentWeapon.backUpAmmo <= 0)
            val = true;

        return val;
    }

    void PullTrigger()
    {
        firePoint = wepMan.currentWeapon.firePoint;
        Vector3 origin = firePoint.position;
        Vector3 targetPos = aiManager.primaryThreat.transform.position;
        targetPos.y += 1.2f;

        Vector3 dir = firePoint.forward;
        wepMan.FireWeapon(origin, dir, firePoint.rotation);

        waitForShooting = 0;
    }

    void Reload()
    {
        if (getCoverPos)
            character.isCrouching = true;

        character.reloading = true;
        character.ReloadWeapon(character.reloading);
    }

    void FireWeapon()
    {
        if (aiManager.primaryThreat == null || !character.canFireWeapon) return;

        if(wepMan.EmptyWeapon() && !wepMan.IsRocketLauncher())
        {
            Reload();
        }
        if (wepMan.readyToFire() && !character.isCrouching)
        {
            if(wepMan.currentWeapon.weaps == Weapon.weaponType.BurstFireRifle)
            {
                wepMan.currentWeapon.UnlockTrigger();

                if (wepMan.currentWeapon.TriggerIsUnlock())
                {
                    PullTrigger();
                }
            }
            else
            {
                PullTrigger();
            }
        }
    }
    public void ShootCount()
    {
        numOfShot++;
    }

    public void Death()
    {
        lockTarget = false;
        nav.enabled = false;
        wepMan.shootWeapon -= ShootCount;

        if (leaderSystem != null) leaderSystem.UnSubscribeDelegate();
        if (character.team.units.Count > 0)
            character.CreateNewTeamLeader();

    }


    public void TakeDamage(Character attacker)
    {      
        //System.Action death = new System.Action(() => { character.TakeDamage(1000, attacker, hit, hitDir); });
        //hit.HitBodyPart(death, ref character.isTakingDamage);

        if(attacker)
            aiManager.GetTarget(attacker);

        //character.AlertAllTeamUnit(attacker);
    }




    private bool getCoverPos;

    private const float maxCoverScaningDelay = 1f;
    private float timer;
    private float coverScaningDelayTimer = maxCoverScaningDelay;

    private int turn;

    public int currentCoverScore;
    public float changeCoverTimer;

    public LayerMask wallLayer;
    public float CoverDist;

    public void FindCover(bool scanNow)
    {
        if (!scanNow && coverScaningDelayTimer > 0)
        {
            coverScaningDelayTimer -= Time.deltaTime;
        }
        else
        {
            ScanCover();
            coverScaningDelayTimer = maxCoverScaningDelay;
        }
    }

    private void ScanCover()
    {
        Collider[] cols = Physics.OverlapSphere(m_Transform.position, 10f, LayerMask.GetMask("CoverSpot"));
        List<CoverSpot> coverSpots = new List<CoverSpot>();

        for (int x = 0; x < cols.Length; x++)
        {
            if (!cols[x].GetComponent<CoverSpot>().preoccupied)
            {
                if (aiManager.currentCover && aiManager.currentCover == cols[x].GetComponent<CoverSpot>())
                {
                    continue;
                }

                coverSpots.Add(cols[x].GetComponent<CoverSpot>());
            }
        }

        int[] scores = new int[coverSpots.Count];
        float mDist = float.MinValue;

        for (int i = 0; i < coverSpots.Count; i++)
        {
            scores[i] = 0;

            Vector3 origin1 = coverSpots[i].transform.position + Vector3.up * character.aimPivot.position.y;
            Vector3 dir1 = aiManager.primaryThreat.aimPivot.position - origin1;

            /*if(coverSpots[i].Height == CoverSpot.CoverHeight.Low)
            {
                bool canNotSee = Physics.Raycast(origin1, dir1, GetTargetDist(), LayerMask.GetMask("Obstacle"));
                Debug.DrawRay(origin1, dir1, Color.green);
                if (canNotSee)
                {
                    coverSpots[i].isReliable = false;
                    continue;
                }
            }
            else
            {
                if (!coverSpots[i].CanShootFromHighWallCover(this))
                {
                    coverSpots[i].isReliable = false;
                    continue;
                }
            }*/

            bool canNotSee = Physics.Raycast(origin1, dir1, GetTargetDist(), LayerMask.GetMask("Obstacle", "CoverObject"));
            Debug.DrawRay(origin1, dir1, Color.green);
            if (canNotSee)
            {
                coverSpots[i].isReliable = false;
                continue;
            }

            //Debug.Log(faction + " " + coverSpots[i].isReliable);

            /*RaycastHit hit;
            bool isCoverObject = Physics.Raycast(origin1, dir1, out hit, GetTargetDist(), LayerMask.GetMask("CoverObject"));

            if (isCoverObject)
            {
                if (hit.collider.GetComponent<CoverObject>())
                {
                    CoverObject coverObj = hit.collider.GetComponent<CoverObject>();
                    bool targetBehindCover = coverObj.isTargetBehindThisCover(primaryTarget.gameObject.layer);
                    if (targetBehindCover)
                        coverSpots[i].isReliable = true;
                }
                else
                {
                    coverSpots[i].isReliable = false;
                }
            }*/


            Vector3 origin2 = coverSpots[i].transform.position;

            float tdist = GetDist(m_Transform.position, coverSpots[i].transform.position);
            if (tdist < mDist)
            {
                mDist = tdist;
                scores[i] += 5;
            }

            for (int x = 0; x < GameManager.instance.allTeams.Count; x++)
            {
                if (GameManager.instance.allTeams[x].teamName == character.faction.ToString())
                    continue;

                for (int y = 0; y < GameManager.instance.allTeams[x].units.Count; y++)
                {
                    Vector3 dir2 = GameManager.instance.allTeams[x].units[y].aimPivot.position - origin2;
                    dir2.Normalize();

                    //Debug.Log(faction + " " + coverSpots[i].isReliable);

                    Debug.DrawRay(origin2, dir2 * 4f, Color.blue);

                    if (coverSpots[i].ShootRay(GameManager.instance.allTeams[x].units[y].aimPivot.position, origin2))
                    {
                        coverSpots[i].isReliable = false;
                    }
                    else
                    {
                        if (x == GameManager.instance.allTeams[x].units.Count - 1 && !coverSpots[i].isReliable)
                        {
                            continue;
                        }
                        coverSpots[i].isReliable = true;
                        scores[i] += 5;

                        //Debug.Log(faction + " " + coverSpots[i].isReliable);
                    }
                }

            }

            

            //Debug.Log(faction + " " + coverSpots[i].isReliable);
            Collider[] nearByEnemies = Physics.OverlapSphere(coverSpots[i].transform.position, 10f, LayerMask.GetMask("Target"));

            for (int z = 0; z < nearByEnemies.Length; z++)
            {
                if (nearByEnemies[z].GetComponent<Character>() && nearByEnemies[z].GetComponent<Character>().faction != character.faction)
                {
                    coverSpots[i].isReliable = false;
                }
            }

            //Debug.Log(faction + " " +  coverSpots[i].isReliable);
        }

        GetCover(ref scores, ref coverSpots);
    }

    private void GetCover(ref int[] scores, ref List<CoverSpot> covers)
    {
        int mScore = int.MinValue;
        CoverSpot previousCover = null;
        for (int x = 0; x < scores.Length; x++)
        {
            if (!covers[x].isReliable)
                continue;

            int tScore = scores[x];
            if (tScore > mScore)
            {
                mScore = tScore;
                if (covers[x].preoccupied)
                    continue;
                if (previousCover != null) previousCover.preoccupied = false;

                previousCover = covers[x];
                aiManager.currentCover = covers[x];
                aiManager.currentCover.preoccupied = true;
                currentCoverScore = scores[x];
            }
        }

        covers.Clear();
    }

    public void HideBehindCover()
    {
        float coverDist = aiManager.GetDist(m_Transform.position, aiManager.currentCover.transform.position);

        if (coverDist < 0.8f)
        {
            getCoverPos = true;

            if (getCoverPos)
            {
                changeCoverTimer += Time.deltaTime;
                ShootFromCover();
            }
            
        }
        else
        {
            getCoverPos = false;
            character.isCrouching = false;
        }
    }

    public void ShootFromCover()
    {
        if (character.isTakingDamage)
        {
            turn = 1;
        }
        else
        {
            if (timer < 0)
            {
                timer = 1.5f;
                turn += 1;
                turn = turn == 3 ? 0 : turn;
            }
            else
            {
                timer -= Time.fixedDeltaTime;
            }
        }

        HideAndShoot();

    }

    void HideAndShoot()
    {
        switch (turn)
        {
            case 1:
                Hide();
                break;
            case 2:
                UnCover();
                break;
        }
    }

    void Hide()
    {
        /*if (currentCover.Height == CoverSpot.CoverHeight.High)
        {
           nav.destination = currentCover.transform.position;          
        }*/

        character.aim = false;
        character.isCrouching = true;
    }

    void UnCover()
    {
        /*if (currentCover.Height == CoverSpot.CoverHeight.High)
        {
           nav.destination = currentCover.GetPeekPosition;
        }*/

        character.isCrouching = false;
        character.aim = true;
    }

    public void Stop()
    {
        nav.isStopped = true;
    }


    public void Aim(bool value)
    {
        character.aim = value;

        if (character.aim) character.isCrouching = false;
    }

    public void AimAtEnemy(Transform target)
    {
        Vector3 dir = (target.position - character.aimPivot.position).normalized;
        dir.y -= 0.01f;

        if (dir == Vector3.zero)
            return;

        character.AimPosition(character.aimPivot, dir);
    }

    public void LookAtEnemy(Transform target, float d)
    {
        
        Vector3 dir = (target.position - m_Transform.position).normalized;
        aiManager.lookRotationVector = dir;

        if (dir == Vector3.zero)
        {
            character.aim = false;
            return;
            //Debug.Break(); 
        }

        Quaternion lookRot = Quaternion.LookRotation(dir);
        Quaternion lookDir = Quaternion.Slerp(m_Transform.rotation, lookRot, character.mV.rotateSpeed * d);
        lookDir.x = 0;
        lookDir.z = 0;
        m_Transform.rotation = lookDir;
    }

    public void AimState()
    {
        if(aiManager.primaryThreat)
        {
            LookAtEnemy(aiManager.primaryThreat.transform, Time.deltaTime);
            AimAtEnemy(aiManager.primaryThreat.chestTran);
        }
    }


    public void SearchEnemy(List<Character> characters, float maxDist)
    {
        for(int x = 0; x < characters.Count; x++)
        {
            float dist = aiManager.GetDist(m_Transform.position, characters[x].transform.position);

            if (dist < maxDist)
            {
                maxDist = dist;
                Character target = characters[x];
                aiManager.GetTarget(target);
            }
        }
    }

    public Vector3 GetTargetDirection()
    {
        return aiManager.primaryThreat.transform.position - m_Transform.position;
    }
    public float GetDist(Vector3 origin, Vector3 target)
    {
        return Vector3.Distance(origin, target);
    }

    public float GetTargetDist()
    {
        return Vector3.Distance(m_Transform.position, aiManager.primaryThreat.transform.position);
    }

    public bool istargetInRange(float _range)
    {
        bool value = false;
        if(aiManager.primaryThreat)
        {
            value = GetDist(m_Transform.position, aiManager.primaryThreat.transform.position) < _range;
        }
        return value;
    }

    public bool isTargetBehindWall()
    {
        return RayCastCheck(character.aimPivot.position, (aiManager.primaryThreat.chestTran.position - character.aimPivot.position).normalized, GetTargetDist());
    }

    public LayerMask obstaclesLayer;
    public bool RayCastCheck(Vector3 origin, Vector3 direction, float maxDist)
    {
        //Debug.DrawRay(origin, direction, Color.red);
        return Physics.Raycast(origin, direction, maxDist, obstaclesLayer);
    }



    public bool isTravellingTooLong()
    {
        if (travelTimer > 2f)
        {
            travelTimer = 0;
            return true;
        }
        else
        {
            travelTimer += Time.deltaTime;
            return false;
        }
    }

    public void Destination(Vector3 pos)
    {
        nav.destination = pos;
    }

    public void StayInThatPosition()
    {
        nav.destination = m_Transform.position;
    }

    private float scanTimer;
    public Character ScanNearByEnemy(float range, bool scanImmediatly)
    {
        Character c = null;

        if (scanImmediatly || scanTimer < 0)
        {
            Collider[] cols = Physics.OverlapSphere(m_Transform.position, range, LayerMask.GetMask("Target"));

            List<Character> enemies = new List<Character>();

            for (int x = 0; x < cols.Length; x++)
            {
                if (cols[x].GetComponent<Character>())
                {
                    Character enemy = cols[x].GetComponent<Character>();
                    if (enemy.faction != character.faction)
                    {
                        enemies.Add(enemy);
                    }
                }
            }

            if (enemies.Count != 0)
            {
                aiManager.primaryThreat = enemies[Random.Range(0, enemies.Count)];
                c = aiManager.primaryThreat;
            }


            enemies.Clear();

            scanTimer = 2f;
        }

        scanTimer -= Time.deltaTime;

        return c;
    }

    public Character ScanThreats()
    {
        Character enemy = null;

        if (character.superObject.owner == null || character.team == GameManager.instance.TeamHasSuperObject)
        {
            aiManager.debug1 = "GetAnyEnemy";
            enemy = GameManager.instance.GetAnyEnemy(character, 15);
            //if (enemy) Debug.Break();
        }
        else
        {
            aiManager.debug1 = "GetSuperObjectOwner";
            enemy = GameManager.instance.GetEnemyWhoHasSuperObject(character);
        }

        return enemy;
    }

    public void CallAirStrike()
    {
        if (GameManager.instance.sceneIndex == 2 || GameManager.instance.sceneIndex == 8)
        {
            try
            {
                character.team.CallAirStrikeWhenItReady(true, aiManager.primaryThreat.transform);
            }
            catch (System.Exception e) { }
        }
    }

}
