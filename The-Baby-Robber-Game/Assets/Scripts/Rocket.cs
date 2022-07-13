using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Projectile
{
    public ExplosionForce explosion;
    public float velocity;

    void FixedUpdate()
    {
        Launching();
    }

    public override void SetUp()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //Physics.IgnoreLayerCollision(15, 9, true);
        gameObject.layer = 15;
    }

    public override void OnCollideWithObject(Collider collider)
    {
        Detonate(collider, 4f, 0);
    }

    void Launching()
    {
        if (!rb) 
            return;

        transform.rotation = Quaternion.LookRotation(rb.velocity);
        rb.AddForce(direction * velocity);
    }

    void Detonate(Collider hitCollider, float particleDuration, float objectDuration)
    {
        if(hitCollider.gameObject.layer == 10 || hitCollider.gameObject.layer == 9)
        {
            HitCharacter(hitCollider, attacker, weaponDMG);
        }

        ExplosionForce explode = Instantiate(explosion, transform.position, transform.rotation);
        explode.warHead = this;
        CameraShake.Instance.HitShake(0.3f, 1f);

        Destroy(explode.gameObject, particleDuration);
        Destroy(gameObject, objectDuration);
    }
}
