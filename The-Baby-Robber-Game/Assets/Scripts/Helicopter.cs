using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : TeamFaction
{
    public Target UITarget;
    public float speed;
    public float closeRange;
    public bool fired = false;

    public Transform Striketarget;
    public Transform SpawnPoint;
    public Transform StrikePoint;

    public Animator[] rotorAnim; 
    public GameObject Missle;
    public GameObject[] missleSpawnPoint;

	private const float farDist = 1500f;
    private Rigidbody chopperEngine;
    private AudioSource audioSource;

    private void Awake()
    {
        unitType = UnitType.AirCraft;
        UITarget = GetComponent<Target>();
        chopperEngine = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        audioSource.minDistance = 100;

        if (faction == GameManager.instance.MainPlayer.faction)
            UITarget.TargetColor = Color.cyan;
    }

    void FixedUpdate()
    {
        if (isFarFromSpawnPoint)
        {
            Destroy(gameObject);
        }

        chopperEngine.AddForce(transform.forward * speed);

        FireMissle();
    }


    void FireMissle()
    {
        if (!fired)
        {
            if (isCloser)
            {
                foreach (var missle in missleSpawnPoint)
                {
                    GameObject FireMissle = Instantiate(Missle, missle.transform.position, missle.transform.rotation);
                    Vector3 dir = (Striketarget.position - missle.transform.position).normalized;
                    Rocket rocket = FireMissle.GetComponent<Rocket>();
                    rocket.LauchProjectile(dir, this, 1000, 12000f);
                    Destroy(FireMissle, 5f);
                }

                fired = true;
                UITarget.enabled = false;
            }

        }
    }

    bool isCloser
    {
        get
        {
            return Vector3.Distance(transform.position, StrikePoint.position) < closeRange;
        }
    }

    bool isFarFromSpawnPoint
    {
        get
        {
            return Vector3.Distance(transform.position, SpawnPoint.position) > farDist;
        }
    }
}
