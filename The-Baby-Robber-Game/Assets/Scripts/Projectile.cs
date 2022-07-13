using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int weaponDMG;

    protected Rigidbody rb;

    public TeamFaction attacker;
    private Character theSameGuyWhoHadShootYou;
    protected Vector3 direction;
    int damageTaken;

    public abstract void SetUp();
    public abstract void OnCollideWithObject(Collider collider);

    private void Awake()
    {
        SetUp();
        Invoke(nameof(DestroyProjectile), 3f);
    }

    void OnCollisionEnter(Collision c)
    {
        OnCollideWithObject(c.collider);      
    }

    public void LauchProjectile(Vector3 direction, TeamFaction _owner, int _weaponDMG, float speed)
    {
        attacker = _owner;
        weaponDMG = _weaponDMG;
        this.direction = direction;
        rb.velocity = direction * speed * Time.fixedDeltaTime;
    }

    public void DestroyProjectile()
    {
        Destroy(this.gameObject);
    }

    public void HitCharacter(Collider collider, TeamFaction attacker, int weaponDMG)
    {
        if (collider.GetComponentInParent<Character>())
        {
            Character target = collider.GetComponentInParent<Character>();
            bool damagebleCharacter = target.faction != attacker.faction;

            if (damagebleCharacter)
            {
                HitPosition hP = collider.GetComponent<HitPosition>();
                DamageCharacter(attacker, target, hP, weaponDMG);
            }
        }
    }


    void DamageCharacter(TeamFaction attacker, Character target, HitPosition hP, int weaponDMG)
    {
        damageTaken++;

        if (theSameGuyWhoHadShootYou != null && theSameGuyWhoHadShootYou == target && damageTaken >= 4)
        {
            damageTaken = 0;

            Collider[] col = Physics.OverlapSphere(attacker.transform.position, 15f, LayerMask.GetMask("Target"));

            for (int x = 0; x < col.Length; x++)
            {
                if (col[x].GetComponent<Character>())
                {
                    Character otherUnit = col[x].GetComponent<Character>();

                    if (otherUnit.faction == attacker.faction)
                    {
                        if (otherUnit.AIPlayer == null)
                            continue;
                        otherUnit.AIPlayer.aiManager.primaryThreat = target;
                    }
                }
            }
        }

        target.TakeDamage(weaponDMG, attacker, hP);

        theSameGuyWhoHadShootYou = target;
    }
}
