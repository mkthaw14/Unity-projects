using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image img;

    void Start()
    {
        img = GetComponent<Image>(); 
    }


    public void UpdateHealthBar(float fullHealth, float health)
    {
        img.fillAmount = health / fullHealth;
    }

    public void TakeDamage(float fullHealth, float health)
    {
        health = Mathf.Clamp(health, 0, fullHealth);
        health--;
    }
    
}
