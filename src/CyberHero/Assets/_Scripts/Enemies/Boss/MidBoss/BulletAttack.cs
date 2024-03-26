using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BulletAttack : MonoBehaviour
{
    [Header("Shoot Attack")]
    
    [SerializeField] Vector3 attackOffset;
    [SerializeField] float attackRange = 1f;

    [Header("Bullet")]
    public GameObject bullet;
    public Transform bulletPos;
    private float timer = 5f;

    private Rigidbody2D rb;
    private GameObject player;
    // private Collider2D colInfo;
    public UnityEvent OnShoot;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance < attackRange)
        {
            timer += Time.deltaTime;
            if (timer > 2)
            {
                timer = 0;
                Shoot();
            }
        }

    }

    private void Shoot()
    {
        OnShoot.Invoke();
        Instantiate(bullet, bulletPos.position, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);      
    }

}
