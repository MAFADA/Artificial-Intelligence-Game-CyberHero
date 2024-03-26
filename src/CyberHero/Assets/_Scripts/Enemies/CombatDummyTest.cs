using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyTest : MonoBehaviour, IDamageable
{
    [SerializeField]
    private GameObject hitParticles;

    public void Damage(float amount)
    {
        Debug.Log(amount + "Damage Taken");

        Instantiate(hitParticles, transform.position, Quaternion.Euler(.0f, .0f, Random.Range(0.0f, 360.0f)));
        // anim.SetTrigger("damage");
    }
}
