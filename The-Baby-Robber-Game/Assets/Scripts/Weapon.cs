using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponManager User;
    public WeaponProperties weapPro;
    public CharacterIK IK;
    public ParticleSystem[] particles;
    public AudioSource gunSound;
    public AudioClip clip;


    public bool isreloading;
    public bool resettingCatridge;
    public bool unlockTrigger;

    public weaponType weaps;
    public enum weaponType
    {
        Pistol, AssaultRifle, RocketLaucher, SniperRifle, ShotGun, BurstFireRifle
    }

    public Transform firePoint;
    public Transform SmokeEscapePoint;
    public Transform leftHandTarget;
    public Transform EquipT;
    public Transform UnequipT;
    public Collider[] cols;
    public Collider mainCol;

    public GameObject[] Effects;
    public Rigidbody rigid;
    public Rocket RocketPrefab;


    public int backUpAmmo;

    public int fullAmmo;

    public int currentAmmo;

    public int fireCount;

    
    float Timer;

    public void SetUp(CharacterIK ik, WeaponManager user)
    {
        IK = ik;
        User = user;
        EquipT = IK.equipT;

        SetUpComponents();
    }

    void SetUpComponents()
    {
        if (clip == null || particles == null || rigid == null)
        {
            gunSound = GetComponent<AudioSource>();
            clip = gunSound.clip;
            particles = this.GetComponentsInChildren<ParticleSystem>();
            rigid = GetComponent<Rigidbody>();
            rigid.isKinematic = true;
            rigid.maxDepenetrationVelocity = 0.01f;
            cols = GetComponentsInChildren<Collider>();
            mainCol = GetComponentInChildren<Collider>();
            for(int x = 0; x < cols.Length; x++)
            {
                cols[x].isTrigger = true;
            }

            Physics.IgnoreLayerCollision(17, 10);
        }


    }

    public float triggerUnlockTimer;

    private void Update()
    {
        if(User == null)
        {
            Timer += Time.deltaTime;
            if (Timer > 30)
                Destroy(gameObject);
        }
        else if (weaps == weaponType.BurstFireRifle && !User.User.aim || weaps == weaponType.BurstFireRifle && User.User.reloading)
        {
            unlockTrigger = false;
            ResetFireCount();
        }
        else
        {
            Timer = 0;
        }

    }

    public Rigidbody GetRigidBody()
    {
        return GetComponent<Rigidbody>();
    }

    public void EnbleCollider()
    {
        mainCol.isTrigger = false;
    }

    public void Reload(ref bool reload)
    {
        if (reload)
        {
            int reloadAmmo = fullAmmo - currentAmmo; // amount of ammo the need to reload

            if (backUpAmmo < reloadAmmo)
            {
                currentAmmo = backUpAmmo;
                backUpAmmo = 0;
            }
            else
            {
                currentAmmo += reloadAmmo;
                backUpAmmo -= reloadAmmo;
            }
            reload = false;
        }
    }

    public void Equip(WeaponManager owner, CharacterIK ik)
    {
        SetUp(ik, owner);
        SetUpLeftHandIK();
        SetUpEquipTrans();
        GetFirePoint();
        owner.GetCurrentWeapAmmo();
    }

    public void UnEquip()
    {
        SetUpUnEquipTrans();
        GetFirePoint();
    }

    void SetUpLeftHandIK()
    {
        leftHandTarget = transform.GetChild(2);
        IK.l_HandTarget = leftHandTarget;
    }

    void SetUpEquipTrans()
    {
        transform.SetParent(EquipT);
        transform.localPosition = weapPro.equipSettings.equipPos;
        transform.localEulerAngles = weapPro.equipSettings.equipRot;
        IK.l_HandTarget = leftHandTarget;
    }

    void SetUpUnEquipTrans()
    {
        transform.SetParent(UnequipT);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }

    void GetFirePoint()
    {
        Transform firePnt = transform.GetChild(0);
        firePoint = firePnt;
    }

    public void RocketProjectile(Character owner,  Transform spawnPoint, Vector3 origin, Vector3 dir)
    {
        if (!User || currentAmmo <= 0 || resettingCatridge) return;

        Projectile bullet = Instantiate(RocketPrefab, firePoint.position, Quaternion.LookRotation(dir));

        bullet.transform.position = origin;
        bullet.transform.rotation = spawnPoint.rotation;

        float projectileSpeed;

        projectileSpeed = 10000;
        bullet.LauchProjectile(dir, owner, weapPro.GetWeaponDMG(owner.weapSkill.weaponDMG), projectileSpeed);

        gunSound.PlayOneShot(clip);
        owner.StartRecoil();
        OtherEffect();
        currentAmmo--;
        LoadingNextAmmunition();       
    }


    public void BulletProjectile(Character owner, Vector3 origin, Vector3 dir, Quaternion startRotation, float spreadX, float spreadY, int bulletCount)
    {
        if (!User || currentAmmo <= 0 || resettingCatridge) return;

        currentAmmo = Mathf.Clamp(currentAmmo, 0, fullAmmo);

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Clear();
            particles[i].Emit(1);
        }

        for (int x = 1; x <= bulletCount; x++)
        {
            float RandomX = Random.Range(-spreadX, spreadX);
            float RandomY = Random.Range(-spreadY, spreadY);

            Vector3 RandomDir = dir + new Vector3(RandomX, RandomY);

            Projectile bullet = Instantiate(ObjectPool.instance.bulletPrefab);

            bullet.transform.position = origin;
            bullet.transform.rotation = startRotation;

            float bulletSpeed;
            if (weaps == weaponType.SniperRifle)
                bulletSpeed = 40000;
            else
                bulletSpeed = 20000;

            bullet.LauchProjectile(RandomDir, owner, weapPro.GetWeaponDMG(owner.weapSkill.weaponDMG), bulletSpeed);

        }

        if (weaps == weaponType.BurstFireRifle)
        {
            fireCount++;
        }

        gunSound.PlayOneShot(clip);
        owner.StartRecoil();
        currentAmmo--;
        LoadingNextAmmunition();
    }


    void LoadingNextAmmunition()
    {
        resettingCatridge = true;
        StartCoroutine(LoadNextBullet());
    }

    IEnumerator LoadNextBullet()
    {
        yield return new WaitForSeconds(weapPro.weapSetting.bulletLoadTime);
        resettingCatridge = false;
    }

    public void BurstFire(Character owner, Vector3 origin, Vector3 dir, Quaternion startRotation, float spreadX, float spreadY, int bulletCount)
    {
        UnlockTrigger();

        if (TriggerIsUnlock())
        {
            BulletProjectile(owner, origin, dir, startRotation, spreadX, spreadY, bulletCount);
        }
    }

    void ResetFireCount()
    {
        fireCount = 0;
        triggerUnlockTimer = 0;
    }

    void OtherEffect()
    {
        for(int x = 0; x < Effects.Length; x++)
        {
            GameObject effect = Instantiate(Effects[x], SmokeEscapePoint.position, SmokeEscapePoint.rotation);
            Destroy(effect, 0.3f);
        }
    }


    public void SelfDestruction(float lifeTime)
    {
        Destroy(this.gameObject, lifeTime);
    }

    public void UnlockTrigger()
    {
        unlockTrigger = true;;
    }

    public bool TriggerIsUnlock()
    {
        if(fireCount > 2)
        {
            if(triggerUnlockTimer > 0.4f)
            {
                ResetFireCount();
            }
            else
            {
                triggerUnlockTimer += Time.deltaTime;
                unlockTrigger = false;
            }
        }

        return unlockTrigger;
    }


}
