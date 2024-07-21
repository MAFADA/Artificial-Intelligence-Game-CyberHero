using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : CoreComponent
{
    public event Action OnHealthZero;


    [SerializeField] private float maxHealth;
    private float currentHealth;

    [SerializeField] private float barrierHealth;
    private float currentBarrierHealth;
    private bool barrierActive = false;

    public bool BarrierActive { get => barrierActive; set => barrierActive = value; }
    public float CurrentBarrierHealth { get => currentBarrierHealth; set => currentBarrierHealth = value; }
    public float BarrierHealth { get => barrierHealth; }
    public float CurrentHealth { get => currentHealth; }
    public float MaxHealth { get => maxHealth; }

    /*
    public Image healthBar;
    public Image barrierBar;*/

    protected override void Awake()
    {
        base.Awake();

        currentHealth = maxHealth;
    }

    public void DecreaseHealth(float amount)
    {
        if (barrierActive)
        {
            currentBarrierHealth -= amount;
            if (currentBarrierHealth <= 0)
            {
                barrierActive = false;
               /* barrierBar.gameObject.SetActive(false);*/
            }
            /*UpdateBarrierBar();*/
        }
        else
        {
            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                currentHealth = 0;

                OnHealthZero?.Invoke();

                Debug.Log("health is zero");
            }
        }
        
    }

    public void IncreaseHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

   /* private void UpdateHealthBar()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void UpdateBarrierBar()
    {
        barrierBar.fillAmount = currentBarrierHealth / barrierHealth;
    }*/


    public bool GetIsDead()
    {
        return currentHealth <= 0;
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}
