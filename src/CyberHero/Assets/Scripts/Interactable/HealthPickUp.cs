using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickUp : ItemPickUp
{
    [SerializeField] private PlayerHealth player;

    public override void PickUp()
    {
        AddHealth(20);

        Destroy(this.gameObject);
    }

    private void AddHealth(int amount)
    {
        player.CurrentHealth += amount;

        if(player.CurrentHealth > player.MaxHealth )
        {
            player.CurrentHealth = player.MaxHealth;
        }
    }
}
