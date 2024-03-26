using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurretMob : MonoBehaviour
{
    [SerializeField] float health = 100f;
    public UnityEvent<GameObject> OnHit;

    public void TakeDamage(float damage)
    {
        // OnHit.Invoke();
        health -= damage;
    }
}
