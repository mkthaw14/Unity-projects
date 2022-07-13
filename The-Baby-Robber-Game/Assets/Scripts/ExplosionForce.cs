using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    public Rocket warHead;
    public LayerMask target;
    public float explosionForce;
    public float upwardForce;
    public float radius;



    IEnumerator Start()
    {
        yield return null;

        Collider[] cols = Physics.OverlapSphere(transform.position, radius, target);
        List<Rigidbody> rigids = new List<Rigidbody>();

        for (int x = 0; x < cols.Length; x++ )
        {
            RaycastHit hit;
            Collider exposedCollider;

            if (cols[x].attachedRigidbody != null && !rigids.Contains(cols[x].attachedRigidbody))
            {
                Vector3 dir = (cols[x].transform.position) - transform.position;
                dir.Normalize();
                float dist = Vector3.Distance((cols[x].transform.position), transform.position);
                if (Physics.Raycast(transform.position, dir, out hit, dist, LayerMask.GetMask("Obstacle", "CoverObject", "Target", "Ragdoll")))
                {
                    exposedCollider = hit.collider;
                    warHead.HitCharacter(exposedCollider, warHead.attacker, warHead.weaponDMG);
                    rigids.Add(exposedCollider.attachedRigidbody);
                }
            }

        }

        for(int x = 0; x < rigids.Count; x++)
        {
            if (rigids[x])
            {
                rigids[x].AddExplosionForce(explosionForce, transform.position, radius, upwardForce, ForceMode.Impulse);
            }
        }
    }
}
