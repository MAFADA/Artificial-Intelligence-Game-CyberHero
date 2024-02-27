using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExplosiveEnemy : MonoBehaviour
{
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] int damageAmount = 20;
    // [SerializeField] GameObject explosionEffect;
    [SerializeField] Animator animator;
    [SerializeField] float explosionTimer = 3f;
    public UnityEvent OnExplosion;
    private bool exploded = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!exploded && collision.CompareTag("Player"))
        {
            explosionTimer -= Time.deltaTime;
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null && explosionTimer <= 0)
            {
                player.TakeDamage(damageAmount);
                Explode();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!exploded && collision.CompareTag("Player"))
        {
            explosionTimer -= Time.deltaTime;
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null && explosionTimer <= 0)
            {
                player.TakeDamage(damageAmount);
                Explode();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player"))
        {
            explosionTimer = 3f;
        }
    }

    private void Start()
    {

    }

    private void Explode()
    {
        exploded = true;
        OnExplosion.Invoke();
        animator.SetTrigger("Explode");

        // Spawn explosion effect
        // Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Apply explosion force to nearby objects
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D collider in colliders)
        {
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce((collider.transform.position - transform.position).normalized * 10f, ForceMode2D.Impulse);
            }
        }

        // Destroy the explosive enemy
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
