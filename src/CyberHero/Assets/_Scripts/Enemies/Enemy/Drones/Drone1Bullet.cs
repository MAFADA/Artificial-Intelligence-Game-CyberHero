using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone1Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 20;
    public Rigidbody2D rb;
    public GameObject laserImpact;
 
    void Start()
    {
        rb.velocity = transform.right*speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Combat player = collision.GetComponentInChildren<Combat>();
        if (player != null)
        {
            player.Damage(damage);
        }

        Instantiate(laserImpact,transform.position, transform.rotation);

        Destroy(gameObject);
    }

}
