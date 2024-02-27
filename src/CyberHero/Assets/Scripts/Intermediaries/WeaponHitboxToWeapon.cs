using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitboxToWeapon : MonoBehaviour
{
    private AggressiveWeapon weapon;

    private void Awake()
    {
        weapon = GetComponentInParent<AggressiveWeapon>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        weapon.AddToDetected(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        weapon.RemoveFromDetected(other);

    }
}
