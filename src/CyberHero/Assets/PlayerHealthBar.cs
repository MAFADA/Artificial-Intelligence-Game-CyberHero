using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    public HealthBar healthBar;
    protected Stats stat;
    void Start()
    {
        stat = GetComponentInChildren<Stats>();
        healthBar.SetMaxHealth(stat.MaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.SetHealth(stat.CurrentHealth);
    }
}
