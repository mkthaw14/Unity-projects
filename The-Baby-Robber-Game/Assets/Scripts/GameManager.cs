using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Character MainPlayer;
    public CameraManager cameraManager;
    public List<SpawnManager> spawnManager = new List<SpawnManager>();
    public Weapon[] weaponPrefabs;
    public SuperObject superObjectPrefab, superObject;
    public Team TeamHasSuperObject;
    public CameraSetting cameraSetting;
    public SaveLoadSystem saveLoadSystem;

    [System.Serializable]
    public enum GameMode
    {
        CatchBabyAndKillMoreEnemyAsYouCan, CatchBabyAndDefendYourself
    }

    [SerializeField]
    public GameMode gameMode;

    public float gameSpeed;
    public float normalSpeed;


    public bool StartGame = false;
    public bool playerIsDead = false;

    [HideInInspector] public AirSupportSystem airSupport;
    [HideInInspector] public MrThaw.CameraHolder cameraHolder;

    public float TimeLimit;
    public float CountDownTimer;

    private int spawnLimit;
    private int base_enemy_damage;
    private int levelDifficulty;

    public int sceneIndex;
    public int maxSceneCount;
    public int currentPlayerSelectedTeamColor;

    public float ai_moveSpeed, ai_moveSpeedWhileAiming;
    public float superObjectMoveSpeed, superObjectAngularSpeed;

    string[] FactionNames;
    Weapon[] randomWeaponArrayForTeams;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        SetCursor(CursorLockMode.Locked, false);

        SetUpInstance();
        DontDestroyOnLoad(this.gameObject);
    }

    private void SetUpInstance()
    {
        if (instance == null)
        {
            instance = this;
            saveLoadSystem = new SaveLoadSystem();
            saveLoadSystem.LoadGame();
            cameraSetting = new CameraSetting();
        }
        else if (instance != this)
        {
             Destroy(this.gameObject);
        }
    }


    private IEnumerator SetUp()
    {
        yield return null; // it has to wait for a frame to complete the scene setup

        CountDownTimer = TimeLimit;
        airSupport = GetComponent<AirSupportSystem>();
        LevelManager.instance.SpawnLevels();

        AssignNewRandomWeaponForTeams();
        CreateSuperObject();
        SpawnCharacter(true);
        StartGame = true;
    }

    private Weapon GetWeaponPrefab(int teamIndex)
    {
        Weapon prefab = null;
        switch (sceneIndex)
        {
            case 1:
                prefab = weaponPrefabs[0];
                break;
            case 2:
                prefab = weaponPrefabs[1];
                break;
            case 3:
                prefab = weaponPrefabs[2];
                break;
            case 4:
                prefab = weaponPrefabs[3];
                break;
            case 5:
                prefab = GetRandomWeapon(teamIndex);
                break;
            case 6:
                prefab = weaponPrefabs[4];
                break;
            case 7:
                prefab = weaponPrefabs[3];
                break;
            case 8:
                prefab = GetRandomWeapon(teamIndex);
                break;
            case 9:
                prefab = weaponPrefabs[4];
                break;
            case 10:
                prefab = weaponPrefabs[2];
                break;
            case 11:
                prefab = weaponPrefabs[1];
                break;
            case 12:
                prefab = weaponPrefabs[0];
                break;
        }

        return prefab;
    }

    void AssignNewRandomWeaponForTeams()
    {
        randomWeaponArrayForTeams = new Weapon[FactionNames.Length];
        randomWeaponArrayForTeams[0] = weaponPrefabs[Random.Range(0, weaponPrefabs.Length - 1)];

        for(int x = 1; x < randomWeaponArrayForTeams.Length; x++)
        {
            randomWeaponArrayForTeams[x] = weaponPrefabs[x];
        }

        for(int i = 1; i < randomWeaponArrayForTeams.Length; i++)
        {           
            while (isSameWeapon(i))
            {
                int randomIndex = Random.Range(0, randomWeaponArrayForTeams.Length);
                randomWeaponArrayForTeams[i] = weaponPrefabs[randomIndex];
            }
        }
    }

    Weapon GetRandomWeapon(int teamIndex)
    {
        Weapon w = null;

        w = randomWeaponArrayForTeams[teamIndex];

        return w;
    }

    bool isSameWeapon(int index)
    {
        bool val = false;

        for(int x = 0; x < randomWeaponArrayForTeams.Length; x++)
        {
            if(index == x)
            {
                continue;
            }
            else if(randomWeaponArrayForTeams[index] == randomWeaponArrayForTeams[x])
            {
                val = true;
                break;
            }
        }

        return val;
    }

    private void Update()
    {
        //     if (Input.GetKeyDown(KeyCode.Escape) && StartGame)
        //         PauseGame(3);
        //     else if (StartGame && !isPause)
        //UpdateGamePlay();

        if (StartGame)
        {
            inputHandling.UpdateInput();
            if (!isGameFreeze())
            {
                UpdateGamePlay();
            }

        }
    }

    private void LateUpdate()
    {
        if (!StartGame) return;
        if (cameraHolder != null)
            cameraHolder.LateTick();

        UpdateCharacterWeaponRecoil();
    }

    private void CheckCursor()
	{
		if (Cursor.lockState == CursorLockMode.None) 
        {
            SetCursor(CursorLockMode.Locked, false);
		}
	}

    private void SetCursor(CursorLockMode lockMode, bool visible)
    {
        Cursor.lockState = lockMode;
        Cursor.visible = visible;
    }

    void CommandTeamMate()
    {
        if (inputHandling.orderAttack)
        {
            MainPlayer.AlertAllTeamUnit(GetAnyEnemy(MainPlayer, 20));
            UIManager.instance.HUD_handler.teammateCommandHUD.TeammateCommandText("Command: Attack");
        }
        else if (inputHandling.orderFollow)
        {
            MainPlayer.Regroup();
            UIManager.instance.HUD_handler.teammateCommandHUD.TeammateCommandText("Command: Follow");
        }
    }

    public void SetGameSpeed(bool freeze)
    {
        if (freeze)
            gameSpeed = 0;
        else
            gameSpeed = normalSpeed;

        Time.timeScale = gameSpeed;
    }

    public bool isGameFreeze()
    {
        return gameSpeed == 0;
    }

    public Transform GetNearByAirStrikeTarget()
    {
        Transform mainTargetTransform = null;

        if (AnyNonPlayerUnits() != 0)
        {
            Character targetWithSuperObject = null;
            Collider[] cols = Physics.OverlapSphere(MainPlayer.transform.position, 15f, LayerMask.GetMask("Target"));
            List<Character> targets = new List<Character>();

            for (int x = 0; x < cols.Length; x++)
            {
                if (cols[x].GetComponent<Character>().faction != MainPlayer.faction)
                {
                    Character target = cols[x].GetComponent<Character>();
                    targets.Add(target);

                    if (superObject.owner == target && target != MainPlayer)
                    {
                        targetWithSuperObject = target;
                    }
                }
            }

            if (targetWithSuperObject != null)
            {
                mainTargetTransform = targetWithSuperObject.transform;
            }
            else
            {
                if(targets.Count != 0)
                    mainTargetTransform = targets[Random.Range(0, targets.Count)].transform;

            }
        }

        return mainTargetTransform;
    }

    int AnyNonPlayerUnits()
    {
        int count = 0;

        for(int x = 0; x < allTeams.Count; x++)
        {
            if(allTeams[x] == MainPlayer.team)
            {
                continue;
            }
            else
            {
                for(int y = 0; y < allTeams[x].units.Count; y++)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void UpdateGamePlay()
    {
        if(cameraHolder != null)
            cameraHolder.Tick();
        if (gameMode == GameMode.CatchBabyAndKillMoreEnemyAsYouCan)
            CountDown();

        CheckCursor();
        //inputHandling.UpdateInput();
        UpdateCharacterState();
        SpawnCharacter(false);
        CommandTeamMate();
    }

		
    public void CountDown()
    {
        if(CountDownTimer > 0)
        {
            CountDownTimer -= Time.deltaTime;
            if(UIManager.instance != null)
                UIManager.instance.HUD_handler.ShowTimer(CountDownTimer);
        }
        else
        {
            StartGame = false;
            CountDownTimer = 0;
            try 
            { 
                UIManager.instance.HUD_handler.ShowTimer(CountDownTimer);
                DeclareWinOrLose();
            }
            catch(System.Exception e) 
            { 
                if(e != null) { }
            }
        }
    }

    public void ResetCountDownTimer()
    {
        CountDownTimer = TimeLimit;
    }

    private void DeclareWinOrLose()
    {
        int ui_index = 2;
        switch (gameMode)
        {
            case GameMode.CatchBabyAndKillMoreEnemyAsYouCan:
                WinnedByHigherKillCountAndHavingLittleGuy(out ui_index);
                break;
            case GameMode.CatchBabyAndDefendYourself:
                WinnedByHoldingLittleGuyForCertainPeriodOfTime(out ui_index);
                break;
        }

        UIManager.instance.ShowWinningOrLosingScreen(true, ui_index);
        ResetCountDownTimer();
    }

    private void WinnedByHigherKillCountAndHavingLittleGuy(out int UI_Index)
    {
        bool higherKillCount = !TeamHasLowerKillCount(MainPlayer.team);

        if(higherKillCount && superObject.owner == MainPlayer)
        {
            //Human Player Win!
            HumanPlayerWin();
            UI_Index = 1;
        }
        else
        {
            UI_Index = 2;
        }
    }

    private void HumanPlayerWin()
    {
        Debug.Log("HumanPlayer Win");
        bool levelUnlockedText = false;

        if (maxSceneCount < 12 && sceneIndex == maxSceneCount)
        {
            levelUnlockedText = true;
            maxSceneCount++;
            saveLoadSystem.SaveGame();
        }

        UIManager.instance.LevelUnlockedText(levelUnlockedText);
    }

    private void WinnedByHoldingLittleGuyForCertainPeriodOfTime(out int UI_Index)
    {
        if(superObject.owner == MainPlayer)
        {
            HumanPlayerWin();
            UI_Index = 1;
        }
        else
        {
            UI_Index = 2;
        }
    }

    private void UpdateCharacterState()
    {
        for(int x = 0; x < allTeams.Count; x++)
        {
            allTeams[x].UpdateCharacter();
        }
    }
    
    private void UpdateCharacterWeaponRecoil()
    {
        for (int i = 0; i < allTeams.Count; i++)
        {
            allTeams[i].LateUpdateCharacter();
        }
    }


    private int GetNoneFriendlyAICount()
    {
        int count = 0;

        for(int i = 0; i < allTeams.Count; i++)
        {
            if (allTeams[i].teamName == MainPlayer.faction.ToString())
                continue;

            count += allTeams[i].units.Count;
        }

        return count;
    }

    private void CreateSuperObject()
    {
        if (spawnManager.Count == 0) return;
        spawnPoints = new SpawnManager[FactionNames.Length];
        // spawnPoints[0] = spawnManager[Random.Range(0, spawnManager.Count)];

        SuperObject superObj = Instantiate(superObjectPrefab);
        superObj.agent.Warp(spawnManager[Random.Range(0, spawnManager.Count)].GetPos() + Vector3.up * 1.5f + Vector3.right * 3f);

        this.superObject = superObj;
        this.superObject.agent.enabled = true;
    }

    bool isFarFromOtherSpawnPoint(int limit, SpawnManager[] spawnPoints, Transform currentPoint)
    {
        bool val = true;

        for(int i = 0; i < limit; i++)
        {
            float dist = Vector3.Distance(spawnPoints[i].transform.position, currentPoint.position);
            if (dist < 70f)
                val = false;
        }

        return val;
    }

    public SpawnManager[] spawnPoints;
    private void ResetSpawnPoint()
    {
        for(int x = 0; x < spawnManager.Count; x++)
        {
            spawnManager[x].isOccupied = false;
        }
    }

    private void SpawnCharacter(bool initialSpawn)
    {
        if (spawnManager.Count == 0 || spawnPoints.Length == 0) return;

        if (initialSpawn || TeamHasNoUnit() || gameMode == GameMode.CatchBabyAndDefendYourself && TeamNeedMoreUnits())
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                float minDist = float.MinValue;

                for (int x = 0; x < spawnManager.Count; x++)
                {
                    if (spawnManager[x].isOccupied)
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(spawnManager[x].transform.position, superObject.transform.position);
                    if (dist > minDist)
                    {
                        minDist = dist;

                        //if (spawnPoints[i])
                        //{
                        //    spawnPoints[i].isOccupied = false;
                        //}

                        spawnPoints[i] = spawnManager[x];
                        spawnPoints[i].isOccupied = true;
                    }
                }
            }

            ResetSpawnPoint();

            if (cameraHolder == null)
            {
                GameObject camera = Spawn(PrefabManager.instance.prefabModel.CameraPrefab, spawnPoints[0].GetPos());
                cameraManager = camera.GetComponent<CameraManager>();
                cameraHolder = camera.GetComponent<MrThaw.CameraHolder>();
                cameraManager.SetUp();
            }
            if (playerIsDead)
            {
                playerIsDead = false;
                MainPlayer = null;
            }

            for (int x = 0; x < FactionNames.Length; x++)
            {
                if (gameMode == GameMode.CatchBabyAndKillMoreEnemyAsYouCan)
                {
                    if (allTeams[x].units.Count == 0)
                    {
                        CreateNewUnits(x);
                    }
                }
                else
                {
                    CreateNewUnits(x);
                }                
            }
        }
        
    }

    private void CreateNewUnits(int teamIndex)
    {
        for (int unit = allTeams[teamIndex].units.Count; unit < spawnLimit; unit++)
        {
            GameObject charac_obj = allTeams[teamIndex].prefabModel.gameObject;
            GameObject newUnit = Spawn(charac_obj, spawnPoints[teamIndex].GetPos());
            newUnit.name = newUnit.name + " " + unit;

            WeaponManager weaponManager = newUnit.GetComponent<WeaponManager>();
            weaponManager.prefab_1 = GetWeaponPrefab(teamIndex);
            //weaponManager.prefab_2 = weaponPrefabs[1];

            Character newCharacter = newUnit.GetComponent<Character>();

            if (/*y == 1 &&*/ unit == 0) // NOTE THERE IS CHANGES IN THIS LINE
            {
                if (teamIndex == 0 && MainPlayer == null)
                {
                    newUnit.name += "Player";
                    MainPlayer = newCharacter;
                    MainPlayer.SetUp(Character.ControlType.HumanControlled, Character.TeamUnitType.TeamLeader, this.superObject);
                    cameraHolder.SetUp(MainPlayer.humanPlayer, inputHandling, cameraSetting, sceneIndex);
                }
                else
                {
                    newCharacter.SetUp(Character.ControlType.ComputerControlled, Character.TeamUnitType.TeamLeader, this.superObject);
                }

            }
            else
            {
                newCharacter.SetUp(Character.ControlType.ComputerControlled, Character.TeamUnitType.TeamUnit, this.superObject);
            }

            AddAUnitToATeam(newCharacter); // ONLY REGISTER THE NEWUNIT FROM THE GAMEMANAGER CLASS

        }
    }

    private bool TeamHasNoUnit()
    {
        bool val = false;
        
        for(int x = 0; x < allTeams.Count; x++)
        {
            if (allTeams[x].units.Count == 0)
            {
                val = true;
                break;
            }
        }

        return val;
    }

    public float reforcementDelayTimer = 60;
    private bool TeamNeedMoreUnits()
    {
        bool val = false;

        if (reforcementDelayTimer > 0)
        {
            reforcementDelayTimer -= Time.deltaTime;
        }
        else
        {
            for (int x = 0; x < allTeams.Count; x++)
            {
                if (allTeams[x].units.Count < spawnLimit / 2)
                {
                    val = true;
                    break;
                }
            }
            reforcementDelayTimer = 60;
        }


        return val;
    }

    private GameObject Spawn(GameObject prefab, Vector3 position)
    {
        GameObject gO = Instantiate(prefab, position, Quaternion.identity);
        return gO;
    }

    private InputHandling m_inputHandling;
    public InputHandling inputHandling
    {
        get
        {
            if (m_inputHandling == null)
                m_inputHandling = gameObject.GetComponent<InputHandling>();
            return m_inputHandling;
        }
    }


    private WeaponManager m_weaponManager;

    public WeaponManager MainPlayerWeapons
    {
        get
        {
            if ( m_weaponManager == null || m_weaponManager.currentWeapon == null)
            {
                try
                {
                    m_weaponManager = MainPlayer.GetComponent<WeaponManager>(); 
                }
                catch(System.Exception e) { if (e != null) { } }
            }    

            return m_weaponManager;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void StartBattle()
    {
        if (!StartGame) StartGame = true;
        SetUpTeams();
        SceneLoader(sceneIndex);
    }


    private void SetUpTeams()
    {
        FactionNames = new string[GetNumOfTeamsToParticipate()];
        FactionNames[0] = PrefabManager.instance.GetAvailibleFactionName(currentPlayerSelectedTeamColor);

        for (int i = 1; i < FactionNames.Length; i++)
        {
            FactionNames[i] = PrefabManager.instance.GetAvailibleFactionName(0);
        }

        for(int y = 1; y < FactionNames.Length; y++)
        {
            while (isSameFactionName(y))
            {
                int randomNum = Random.Range(0, PrefabManager.instance.GetNumberOfColor());
                FactionNames[y] = PrefabManager.instance.GetAvailibleFactionName(randomNum);
            }
        }

        if (allTeams.Count > 0)
            allTeams.Clear();

        for (int x = 0; x < FactionNames.Length; x++)
        {
            string teamName = FactionNames[x];
            Team team = new Team();
            team.SetUpTeamNameAndAssignPrefabModel(teamName, PrefabManager.instance.GetCharacterPrefabModels(teamName), PrefabManager.instance.GetHelicopterPrefab(teamName));
            allTeams.Add(team);
        }

        GetOtherTeam();
    }

    bool isSameFactionName(int index)
    {
        bool val = false;
        for(int x = 0; x < FactionNames.Length; x++)
        {
            if(index == x)
            {
                continue;
            }
            else if(FactionNames[index] == FactionNames[x])
            {
                val = true;
                break;
            }
        }

        return val;
    }

    int GetNumOfTeamsToParticipate()
    {
        int numofTeam;

        if(sceneIndex <= 6)
        {
            numofTeam = 3;
        }
        else
        {
            numofTeam = 4;
        }

        return numofTeam;
    }

    public void ReloadCurrentScene()
    {
        SceneLoader(sceneIndex);
    }

    public void LoadNextScene()
    {
        if(sceneIndex < maxSceneCount)
            sceneIndex++;

        StartBattle();
    }

    public void SceneLoader(int index)
    {
        switch (index)
        {
	    	case 0:
		     	PreLoad_PlayerSelection_Scene ();
                SceneManager.LoadScene(index);
                break;
		    case 1:
			    PreLoad_GamePlay_Scene ();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 2:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 3:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 4:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 5:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 6:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 7:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 8:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 9:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 10:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 11:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
            case 12:
                PreLoad_GamePlay_Scene();
                SceneManager.LoadScene(index);
                StartCoroutine(SetUp()); // Extremely importance, call with coroutine because it has to wait for a frame before it is being called
                break;
        }
    }

	private void PreLoad_PlayerSelection_Scene()
	{
		if (StartGame)
			StartGame = false;// AI and player setUp are depend on this bool so if you set it to true before loading the playGround scene, the game loop will skip the setUp functions for them especially when reloading the playGround scene

        SetGameSpeed(false);
	}

	private void PreLoad_GamePlay_Scene()//IMPORTANT NOTE: NEVER SET THE "StartGame" BOOLEAN TO TRUE BEFORE LOADING THE PLAYGROUND SCENE
	{
        if (MainPlayer) MainPlayer = null;

        ResetGameplayState();
        SetCursor(CursorLockMode.Locked, false);
        SetGameSpeed(false);
        SetUpGameMode();
        CheckSceneIndex();
	}

    private void SetUpGameMode()
    {
        if(isCatchBabyAndKillMoreEnemy())
        {
            gameMode = GameMode.CatchBabyAndKillMoreEnemyAsYouCan;
        }
        else
        {
            gameMode = GameMode.CatchBabyAndDefendYourself;
        }
    }

    private bool isCatchBabyAndKillMoreEnemy()
    {
        bool val = false;

        val = sceneIndex == 4 || sceneIndex == 5 || sceneIndex == 6 || sceneIndex == 10 || sceneIndex == 11 || sceneIndex == 12;
   
        return val;
    }

    private void CheckSceneIndex()
    {
        switch (sceneIndex)
        {
            case 1:
                AIDifficulty(4.5f, 3.2f, 4.4f, 120f);
                TimeSetUp(60);
                SetUpUnitLimitAndUnitDamage(6, 15, 2);
                break;
            case 2:
                AIDifficulty(4.5f, 3.2f, 4.4f, 120f);
                TimeSetUp(50);
                SetUpUnitLimitAndUnitDamage(8, 15, 2);
                break;
            case 3:
                AIDifficulty(4.5f, 3.2f, 5f, 120f);
                TimeSetUp(80);
                SetUpUnitLimitAndUnitDamage(10, 15, 2);
                break;
            case 4:
                AIDifficulty(5f, 3.4f, 6f, 999f);
                TimeSetUp(180);
                SetUpUnitLimitAndUnitDamage(6, 15, 2);
                break;
            case 5:
                AIDifficulty(5f, 3.2f, 6f, 999f);
                TimeSetUp(180);
                SetUpUnitLimitAndUnitDamage(8, 15, 2);
                break;
            case 6:
                AIDifficulty(5f, 3.4f, 6f, 999f);
                TimeSetUp(300);
                SetUpUnitLimitAndUnitDamage(10, 15, 2);
                break;
            case 7:
                AIDifficulty(4.5f, 3.2f, 5f, 120f);
                TimeSetUp(60);
                SetUpUnitLimitAndUnitDamage(6, 15, 2);
                break;
            case 8:
                AIDifficulty(4.5f, 3.2f, 5f, 120f);
                TimeSetUp(35);
                SetUpUnitLimitAndUnitDamage(8, 15, 2);
                break;
            case 9:
                AIDifficulty(4.5f, 3.2f, 5f, 120f);
                TimeSetUp(40);
                SetUpUnitLimitAndUnitDamage(10, 15, 2);
                break;
            case 10:
                AIDifficulty(5f, 3.4f, 6f, 999f);
                TimeSetUp(180);
                SetUpUnitLimitAndUnitDamage(6, 15, 2);
                break;
            case 11:
                AIDifficulty(5f, 3.4f, 6f, 999f);
                TimeSetUp(180);
                SetUpUnitLimitAndUnitDamage(8, 15, 2);
                break;
            case 12:
                AIDifficulty(5f, 3.4f, 6f, 999f);
                TimeSetUp(300);
                SetUpUnitLimitAndUnitDamage(10, 15, 2);
                break;
        }
    }

    private void ResetGameplayState()
    {
        spawnManager.Clear();
        RefreshAllTeamsList(); // WAIT!!! Don't forget to refresh the team lists when starting a new game
    }

    private void SetUpUnitLimitAndUnitDamage(int spawnLimit, int baseEnemyDamage, int humanPlayerWeaponDMGMultiplier)
    {
        this.spawnLimit = spawnLimit;
        base_enemy_damage = baseEnemyDamage;
        weaponDMGMultiplier_allies = humanPlayerWeaponDMGMultiplier;
    }

    private void AIDifficulty(float ai_moveSpeed, float ai_moveSpeedWhileAiming, float superObjectMoveSpeed, float superObjectAngularSpeed)
    {
        this.ai_moveSpeed = ai_moveSpeed;
        this.ai_moveSpeedWhileAiming = ai_moveSpeedWhileAiming;

        this.superObjectMoveSpeed = superObjectMoveSpeed;
        this.superObjectAngularSpeed = superObjectAngularSpeed;
    }

    private void TimeSetUp(float timeLimit)
    {
        TimeLimit = timeLimit;
    }

    public int enemyHitByPlayer;

    public int weaponDMGMultiplier_allies, weaponDMGMultiplier_enemies;


    public List<Team> allTeams = new List<Team>();

    private void RefreshAllTeamsList()
    {
        for (int x = 0; x < allTeams.Count; x++)
        {
            allTeams[x].RefreshListForNextGameplay();
            allTeams[x].ResetKillCount();
        }

    }

    private void GetOtherTeam()
    {
        for(int x = 0; x < allTeams.Count; x++)
        {
            allTeams[x].GetOtherTeams();
        }
    }

    public Team GetTeam(string currentUnitTeamName)
    {
        Team t = null; 
        for (int x = 0; x < allTeams.Count; x++)
        {
            if(allTeams[x].teamName == currentUnitTeamName)
            {
                t = allTeams[x];
                break;
            }
        }

        return t;
    }

    public void AddAUnitToATeam(Character c)
    {
        for (int x = 0; x < allTeams.Count; x++)
        {
            if (c.faction.ToString() == allTeams[x].teamName)
            {
                allTeams[x].AddToTeam(c);
                break;
            }
        }
    }

    public void RemoveUnit(string teamName, Character currentUnit)
    {
        for(int z = 0; z < allTeams.Count; z++)
        {
            if(teamName == allTeams[z].teamName)
            {
                allTeams[z].RemoveFromTeam(currentUnit);
                break;
            }
        }
    }

    public Character GetAnyEnemy(Character currentUnit, float range)
    {
        Character c = null; 
        float maxDist = float.MaxValue;

        for(int i = 0; i < allTeams.Count; i++)
        {
            if (currentUnit.faction.ToString() == allTeams[i].teamName)
                continue;

            for(int y = 0; y < allTeams[i].units.Count; y++)
            {
  
                float tempDist = Vector3.Distance(currentUnit.transform.position, allTeams[i].units[y].transform.position);

                if(tempDist < maxDist)
                {
                    maxDist = tempDist;

                    Vector3 direction = (allTeams[i].units[y].aimPivot.position - currentUnit.aimPivot.position).normalized;

                    bool behindWall = Physics.Raycast(currentUnit.aimPivot.position, direction, tempDist, LayerMask.GetMask("Obstacle"));
                    if (tempDist < range && !behindWall)
                    {
                        c = allTeams[i].units[y];
                    }
                }
            }
        }

        return c;
    }

    public Character GetEnemyWhoHasSuperObject(Character currentUnit)
    {
        Character c = null;
        float maxDist = float.MaxValue;

        for (int y = 0; y < TeamHasSuperObject.units.Count; y++)
        {
            float tempDist = Vector3.Distance(currentUnit.transform.position, TeamHasSuperObject.units[y].transform.position);

            if (tempDist < maxDist)
            {
                maxDist = tempDist;

                if (tempDist < 25)
                {
                    c = TeamHasSuperObject.units[y];
                }
            }
        }

        return c;
    }

    public bool KillCountNotEnough()
    {
        bool val = TeamHasLowerKillCount(MainPlayer.team);

        return val;
    }

    public bool TeamHasLowerKillCount(Team team)
    {
        bool val = false;
        for (int x = 0; x < allTeams.Count; x++)
        {
            if (allTeams[x] == team)
                continue;

            if (team.killCount <= allTeams[x].killCount)
            {
                val = true;
                break;
            }
        }

        return val;
    }

    public void TeamCurrentlyHasHigherKillCount(Team team)
    {
        int maxCount = int.MinValue;

        for (int x = 0; x < allTeams.Count; x++)
        {
            int killCount = allTeams[x].killCount;
            if (killCount > maxCount)
            {
                maxCount = killCount;
            }
            if (x == allTeams.Count - 1)
            {
                for (int y = 0; y < allTeams.Count; y++)
                {
                    if (allTeams[y].killCount == maxCount && allTeams[y] == team)
                    {
                        UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext(5, allTeams[y].teamName);
                    }
                }
            }
        }
    }

    public bool PlayerHasLittleGuy()
    {
        bool val = false;

        if(superObject.owner == MainPlayer)
        {
            val = true;
        }

        return val;
    }

    public void SaveGameSetting()
    {
        saveLoadSystem.SaveCameraSetting(cameraSetting);
    }

    public void LoadGameSetting()
    {
        saveLoadSystem.LoadCameraSetting(ref cameraSetting);
    }
}


public class Team
{
    public string teamName;
    public int killCount;
    public float airStrikeTimer;
    public List<Character> units = new List<Character>();
    public Character prefabModel;
    public GameObject airStrikeTargetPrefab;
    public Character teamLeader;
    public List<Team> otherTeams;

    AirSupportSystem airSupportSystem;

    public void SetUpTeamNameAndAssignPrefabModel(string _teamName, Character _prefabModel, GameObject airStrikeTarget)
    {
        teamName = _teamName;
        prefabModel = _prefabModel;
        airStrikeTargetPrefab = airStrikeTarget;
    }

    public void AddToTeam(Character newUnit)
    {
        units.Add(newUnit);
        newUnit.team = this;

        if (newUnit.teamUnitType == Character.TeamUnitType.TeamLeader)
            teamLeader = newUnit;
    }

    public void RemoveFromTeam(Character currentUnit)
    {
        units.Remove(currentUnit);
    }

    public void RefreshListForNextGameplay()
    {
        otherTeams.Clear();
        units.Clear();
    }

    public void ResetKillCount()
    {
        killCount = 0;
    }

    public void ShowUnitCount()
    {
        Debug.Log(teamName + "   Count " + units.Count);
    }

  
    public void ShowUnitName()
    {
        for (int x = 0; x < units.Count; x++)
        {
            Debug.Log(teamName + " " + units[x].gameObject.name);
        }
    }

    public void AddKillCount()
    {
        killCount++;
        if(GameManager.instance.gameMode == GameManager.GameMode.CatchBabyAndKillMoreEnemyAsYouCan)
        {
            if (!GameManager.instance.TeamHasLowerKillCount(this))
            {
                UIManager.instance.HUD_handler.gameInstructionHUD.HelpPlayerWhatToDoNext(5, teamName);
            }
        }
    }

    public void UpdateCharacter()
    {
        if (units.Count == 0) return;

        for (int x = 0; x < units.Count; x++)
        {
            units[x].Tick();
        }
    }

    public void LateUpdateCharacter()
    {
        for (int x = 0; x < units.Count; x++)
        {
            units[x].UpdateCharacterRecoil(Time.deltaTime);
        }
    }

    public void GetOtherTeams()
    {
        if(otherTeams == null)
        {
            otherTeams = new List<Team>();

            for (int i = 0; i < GameManager.instance.allTeams.Count; i++)
            {
                if (this == GameManager.instance.allTeams[i])
                {
                    continue;
                }

                otherTeams.Add(GameManager.instance.allTeams[i]);
            }
        }
    }

    public void CallAirStrikeWhenItReady(bool useParam, Transform targetTransform_Param)
    {
        Transform targetTransform;

        targetTransform = targetTransform_Param;

        if(airSupportSystem == null)
        {
            airSupportSystem = GameManager.instance.airSupport;
        }
        if(airStrikeTimer > 24)
        {
            if (!useParam)
            {
                targetTransform = GameManager.instance.GetNearByAirStrikeTarget();
            }
            if (targetTransform != null)
            {
                airSupportSystem.ComfirmAirStrike(targetTransform, airStrikeTargetPrefab.transform);
            }

            airStrikeTimer = 0;
        }
        else
        {
            airStrikeTimer += Time.deltaTime;
        }
    }
}
