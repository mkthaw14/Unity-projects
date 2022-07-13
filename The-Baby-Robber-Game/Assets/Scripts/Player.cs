using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MrThaw;

public class Player 
{
    public InputHandling inp;
    public CameraManager camManager;
    public SniperScope scope;

    public GameObject mainCamera;   
    public Transform camHolder;
    public Transform head;
    public Character character;
    public Transform m_Transform;

    private CharacterController controller;
    private WeaponManager wepMan;
    private Transform camTran;
    private Vector3 mainCamPos;

    float forwardInput, rightInput;
    bool aim, crouch, reload;

    public Player(Character _thisCharacter, CharacterController _controller, Transform _currentTransform, WeaponManager _wepMan)
    {
        SetUp(_thisCharacter, _controller, _currentTransform, _wepMan);
        SetUpCamera();
    }

    public void SetUp(Character _thisCharacter, CharacterController _controller, Transform _currentTransform, WeaponManager _wepMan)
    {
        //WARING ONLY REGISTER THE NEWUNIT FROM GAMEMANAGER CLASS
        character = _thisCharacter;
        controller = _controller;
        controller.center = new Vector3(0, 1, 0);
        controller.radius = 0.4f;
        controller.height = 1.8f;
        controller.skinWidth = 0.08f;

        if (GameManager.instance.sceneIndex == 6 || GameManager.instance.sceneIndex == 12)
            controller.slopeLimit = 45;
        else
            controller.slopeLimit = 65;

        inp = GameManager.instance.inputHandling;
        character.hb = UIManager.instance.HUD_handler.playerHealthHUD;
        camManager = GameManager.instance.cameraManager;
        camHolder = camManager.transform;
        m_Transform = _currentTransform;
        wepMan = _wepMan;

        character.Health = 200;
        character.FullHealth = character.Health;
        character.weapSkill.weaponDMG = GameManager.instance.weaponDMGMultiplier_allies;
        UIManager.instance.HUD_handler.UpdateHealthBar(character.hb, character.Health, character.FullHealth);
        //WARING ONLY REGISTER THE NEWUNIT FROM GAMEMANAGER CLASS
    }
    public void SetUpCamera()
    {
        scope = camManager.scope;
        mainCamera = camManager.mainCam;
        camTran = mainCamera.transform.parent;

        mainCamPos = mainCamera.transform.localPosition;

        CameraShake.Instance.GetCameraTransform = mainCamera.transform;
        CameraShake.Instance.SetUp(wepMan);
    }


    public void AimingInput()
    {
        if (aim || character.DebugAim)
        {
            if (wepMan.currentWeapon.weaps == Weapon.weaponType.SniperRifle)
            {
                camManager.ChangeCameraPosition(true);
                scope.ZoomInOut(true);
            }

            character.aim = true;
        }
        else
        {
            if (wepMan.currentWeapon.weaps == Weapon.weaponType.SniperRifle)
            {
                camManager.ChangeCameraPosition(false);
                scope.ZoomInOut(false);
            }

            character.aim = false;
        }
    }

    void CrouchingInput()
    {
        if (crouch)
        {
            character.isCrouching = true;
        }
        else
        {
            character.isCrouching = false;
        }
    }



    public void Tick(float d)
    {

        if (!inp || GameManager.instance.playerIsDead) return;

        CheckInput();
        character.CheckCharacterMotion(character.mV.moveAmount);
        character.SomeoneIsPushing();
        CharacterAction();
        AdjustMoveSpeed();
        character.HandleAnimation(rightInput, forwardInput, character.mV.moveAmount, d, character.aim);
        CharacterLook(d);
        CommandAirStrike();
    }

    void CheckInput()
    {
        forwardInput = inp.vertical;
        rightInput = inp.horizontal;
        aim = inp.Aiming;
        crouch = inp.Crouch;
        reload = inp.reload;
    }

    void CheckAiming()
    {
        if (character.aim)
        {
            AimState();
        }
        else
        {
            NormalState();
        }
    }

    void CommandAirStrike()
    {
        if (GameManager.instance.sceneIndex == 2 || GameManager.instance.sceneIndex == 8)
        {
            character.team.CallAirStrikeWhenItReady(false, null);
        }
    }

    void CharacterLook(float d)// For Movement Direction
    {
        if (!camHolder) return;

        Vector3 freeDir = camHolder.forward * forwardInput + camHolder.right * rightInput; // If we are not aiming, we will made the character face toward directional key buttons

        freeDir.Normalize();
        character.mV.moveDirection = freeDir;

        Vector3 cameraForward = camHolder.forward; // Rotates the character toward the camera's forward

        if (!character.aim) // Checking for where to rotate the character
            character.mV.rotateDirection = character.mV.moveDirection;
        else
            character.mV.rotateDirection = cameraForward;

        Vector3 targetDir = character.mV.rotateDirection;
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
            targetDir = m_Transform.forward;

        Quaternion lookDir = Quaternion.LookRotation(targetDir);
        Quaternion targetRot = Quaternion.Slerp(m_Transform.rotation, lookDir, character.mV.rotateSpeed * d);
        m_Transform.rotation = targetRot;
    }

    void AdjustMoveSpeed()
    {
        if (character.isCrouching)
        {
            character.mV.speed = character.mV.crouchSpeed;
        }
        else
        {
            if (character.aim)
                character.mV.speed = character.mV.aimSpeed;
            else if(character.superObject.owner == character)
                character.mV.speed = character.mV.moveAmount >= 1? 3.5f: character.mV.walkSpeed;
            else
                character.mV.speed = character.mV.moveAmount >= 1 ? character.mV.runSpeed : character.mV.walkSpeed;
        }
    }

    void MovementNormal()
    {
        character.mV.moveAmount = Mathf.Clamp01(Mathf.Abs(rightInput) + Mathf.Abs(forwardInput));// return between 0 to 1 

        Vector3 dir = Vector3.zero;
        dir = m_Transform.forward * (character.mV.speed * character.mV.moveAmount);
        controller.SimpleMove(dir);
    }

    void MovementAiming()
    {
        float speed = character.mV.speed;
        Vector3 dir = character.mV.moveDirection * speed;

        controller.SimpleMove(dir);
    }


    public void NormalState()
    {
        MovementNormal();
    }

    public void AimState()
    {
        MovementAiming();
        character.AimPosition(camTran, camTran.forward);
        CheckGunFire();
    }

    void CheckGunFire()
    {
        bool singleFireWeapon = wepMan.currentWeapon.weaps == Weapon.weaponType.Pistol || wepMan.currentWeapon.weaps == Weapon.weaponType.SniperRifle;
        bool burstFireWeapon = wepMan.currentWeapon.weaps == Weapon.weaponType.BurstFireRifle;
        bool userInput;

        if (singleFireWeapon)
        {
            userInput = inp.FireOneShot;
            if (userInput && !character.reloading)
            {
                Fire();
            }
        }
        else if (burstFireWeapon)
        {
            userInput = inp.FireOneShot;

            if (userInput && !character.reloading)
            {
                wepMan.currentWeapon.UnlockTrigger();
            }
            if (wepMan.currentWeapon.TriggerIsUnlock())
            {
                Fire();
            }
        }
        else
        {
            userInput = inp.Fire;
            if (userInput && !character.reloading)
            {
                Fire();
            }
        }

    }

    void CheckPlayerReloading()
    {
		if (wepMan.currentWeapon.backUpAmmo == 0)
			return;
        if (reload)
        {
            character.reloading = wepMan.currentWeapon.weaps != Weapon.weaponType.RocketLaucher;
            character.ReloadWeapon(character.reloading);
        }
		else if (wepMan.currentWeapon.currentAmmo <= 0)
        {
            character.reloading = wepMan.currentWeapon.weaps != Weapon.weaponType.RocketLaucher;
            character.ReloadWeapon(character.reloading);
        }
    }

    public void Fire()
    {
        if (!character.aim || !character.canFireWeapon) return;

        Transform origin;

        if (wepMan.IsSniper())
            origin = mainCamera.transform;
        else
            origin = wepMan.currentWeapon.firePoint;

        wepMan.FireWeapon(origin.position, character.firingDirection, origin.rotation);
    }

    public void Death()
    {
        inp = null;
        camManager.MoveToOriginalPosition();
        scope.ZoomOut();

        if (character.team.units.Count > 0)
        {
            character.CreateNewTeamLeader();
        }
        else
        {
            GameManager.instance.MainPlayer = null;
        }

    }


    public void TakeDamage(Character attacker)
    {      
        //hit.HitBodyPart(null, ref character.isTakingDamage);

        if(attacker)
            UI_DamageSystem.SpawnANewPointer(attacker.transform);

        CameraShake.Instance.HitShake(0.1f, 0.3f);

        //character.AlertAllTeamUnit(attacker);
    }

    private void ChangeWeapon()
    {
        if (inp.weaponSwitch && !character.aim)
        {
            character.SwitchWeapon(true);
        }
    }

    private void CharacterAction()
    {
        AimingInput();
        CrouchingInput();
        CheckPlayerReloading();
        ChangeWeapon();
        CheckAiming();
    }


    public void WeaponRecoil(float recoilT) // Put it inside the function of weaponRecoil in character class
    {
        bool spreadLogic = character.recoilIsInit;

        UIManager.instance.HUD_handler.CrossHair.CrosshairSpread(spreadLogic);
        CameraShake.Instance.RecoilShake(recoilT);
    }

    bool ismoving
    {
        get
        {
            if (character.mV.moveAmount != 0
            || inp.inputX != 0 || inp.inputY != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public float regenerationTimer;
    public float regenerationRate;

    void HealthRegeneration()
    {
        regenerationTimer = Mathf.Clamp(regenerationTimer, 0, 3f);
        if (character.isTakingDamage)
        {
            regenerationTimer += Time.deltaTime;
        }
        else
        {
            regenerationTimer -= Time.deltaTime;
        }

        if (character.Health < character.FullHealth)
        {
            if (regenerationTimer <= 0.0f)
            {
                character.Health++;
                UIManager.instance.HUD_handler.UpdateHealthBar(character.hb, character.Health, character.FullHealth);
            }
            
        }
    }
}
