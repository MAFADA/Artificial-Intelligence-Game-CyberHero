using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizePickUp : ItemPickUp
{
    [SerializeField] private PlayerMovement player;

    public override void PickUp()
    {
        player.SizeUp(5);
        Destroy(this.gameObject);
    }
}
