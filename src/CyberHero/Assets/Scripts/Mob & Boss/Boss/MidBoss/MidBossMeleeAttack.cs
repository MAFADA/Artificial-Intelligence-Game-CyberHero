using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MidBossMeleeAttack : MonoBehaviour
{
    [Header("Melee Attack")]
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] LayerMask playerLayers;
    [SerializeField] int attackDamage = 40;

    private Rigidbody2D rb;
    public UnityEvent OnMidBossAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {

    }

    public void Attack()
    {
        OnMidBossAttack.Invoke();
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.GetComponent<PlayerHealth>() == null)
            {
                return;
            }

            // Debug.Log("Enemy Hit");
            enemy.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }


}
