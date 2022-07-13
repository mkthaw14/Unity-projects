using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    public Bullet bulletPrefab;
    public List<Bullet> bullets;

    public int bulletCount;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CreateProjectiles();
    }

    private void CreateProjectiles()
    {
        for(int x = 0; x < bulletCount; x++)
        {
            Bullet b = Instantiate(bulletPrefab);
            bullets.Add(b);
            bullets[x].gameObject.SetActive(false);
        }
    }

    public Bullet GetBullet()
    {
        Bullet b = null;

        for(int x = 0; x < bulletCount; x++)
        {
            if (!bullets[x].gameObject.activeInHierarchy)
            {
                bullets[x].gameObject.SetActive(true);
                b = bullets[x];
                break;
            }
        }

        return b;
    }
}
