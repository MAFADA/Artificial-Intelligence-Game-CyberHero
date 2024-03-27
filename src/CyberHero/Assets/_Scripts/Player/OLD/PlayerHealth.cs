using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth = 100;

    [SerializeField] private Image healthBar;

    [SerializeField] TMP_Text titleTextGameOver;
    public UnityEvent OnPlayerHit;


    // [SerializeField] private TMP_Text damageText;
    public UnityEvent OnPlayerDie;

    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int MaxHealth { get => maxHealth; }

    private void InitVariables()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        InitVariables();
    }

    private void Update()
    {
        healthBar.fillAmount = (float) currentHealth / maxHealth;
    }

    public void TakeDamage(int amount)
    {
        OnPlayerHit.Invoke();
        currentHealth -= amount;

        // damageText.text = Convert.ToString(amount);

        if (currentHealth <= 0)
        {
            // Destroy(gameObject);
            OnPlayerDie.Invoke();
        }
    }
    
    public void GameOverPanel(){
        titleTextGameOver.text = "Health pointmu habis!\nWaspada terhadap serangan dari musuh.";
    }

    public bool IsDead()
    {
        return currentHealth < maxHealth;
    }
}
