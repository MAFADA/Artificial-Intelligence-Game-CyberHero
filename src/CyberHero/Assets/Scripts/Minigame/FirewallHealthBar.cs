using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirewallHealthBar : MonoBehaviour
{
    [SerializeField] Slider slider;

    public void SetHealth(float health)
    {
        slider.value = health;
    }
}
