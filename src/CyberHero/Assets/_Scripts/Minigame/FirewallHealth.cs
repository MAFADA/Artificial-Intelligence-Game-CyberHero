using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FirewallHealth : MonoBehaviour
{
    [SerializeField] float health = 30f;

    [SerializeField] float counterAtk = 30f;
    [SerializeField] Image healthBar;
    public UnityEvent OnFirewallHit;

    private void Start()
    {
        
        healthBar.fillAmount = health / 100f;
    }

    private void Update()
    {
        if (health <= 0)
            Destroy(this.gameObject);

        healthBar.fillAmount = health / 100f;

    }

    // private void OnCollisionEnter2D(Collision2D other) {
    //     if (other.gameObject.CompareTag("Virus"))
    //     {
    //         other.gameObject.GetComponent<Virus>().TakeDamage(counterAtk);
    //     }
    // }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Virus")
        {
            other.gameObject.GetComponent<Virus>().TakeDamage(counterAtk);
        }
    }

    public void TakeDamage(float damage)
    {
        OnFirewallHit.Invoke();
        health -= damage;
        healthBar.fillAmount = health / 100;
    }


    public void addHealth()
    {
        this.health += 30f;
    }

}
