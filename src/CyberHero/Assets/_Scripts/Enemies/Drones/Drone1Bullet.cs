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
        rb.velocity = transform.right * speed;
    }

    void Update()
    {

        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));


        if (transform.position.x < bottomLeft.x || transform.position.x > topRight.x ||
            transform.position.y < bottomLeft.y || transform.position.y > topRight.y)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Combat player = collision.GetComponentInChildren<Combat>();
            if (player != null)
            {
                player.Damage(damage);
                Instantiate(laserImpact, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            Instantiate(laserImpact, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
