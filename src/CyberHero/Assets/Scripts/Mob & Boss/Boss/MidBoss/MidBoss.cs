using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MidBoss : MonoBehaviour
{
    [SerializeField] string midBossName;
    [SerializeField] TMP_Text midBossNameText;
    [SerializeField] Transform player;
    [SerializeField] bool isFlipped;

    [Header("UI")]
    private EnemyHealth health;
    [SerializeField] Image hpBar;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        hpBar.fillAmount = (float)health.CurrentHealth / (float)health.MaxHealth;
        midBossNameText.text = midBossName;
    }

    private void Update()
    {
        hpBar.fillAmount = (float)health.CurrentHealth / (float)health.MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        health.CurrentHealth -= damage;
        hpBar.fillAmount = (float)health.CurrentHealth / (float)health.MaxHealth;
    }


    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x > player.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;
        }
        else if (transform.position.x < player.position.x && !isFlipped)
        {

            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
        }
    }
}
