using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
    TrailRenderer trail;

    private float trailTime;

    public override void SetUp()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();

        rb.maxDepenetrationVelocity = 2f;
        trailTime = trail.time;
        //Physics.IgnoreLayerCollision(15, 15, true);
    }

    public override void OnCollideWithObject(Collider collider)
    {
        if (collider.gameObject.layer == 10)
        {
            HitCharacter(collider, attacker, weaponDMG);
        }

        if (collider.gameObject.layer != 15)
        {
            DestroyProjectile();
        }
    }

}
