using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public EnemyHealth enemyHealth;
    [SerializeField] private int damage;

    private void OnCollisionEnter2D(Collision2D target)
    {
        if (target.gameObject.tag == "Enemy")
        {
            damage = Random.Range(5, 20);
            enemyHealth.TakeDamage(damage);
        }
    }
}
