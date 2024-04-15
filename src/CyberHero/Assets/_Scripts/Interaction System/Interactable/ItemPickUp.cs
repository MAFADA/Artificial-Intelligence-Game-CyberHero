using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }

    public virtual void PickUp()
    {
        Debug.Log("Item Pickup");
    }
}
