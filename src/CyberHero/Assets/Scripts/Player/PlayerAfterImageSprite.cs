using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{
    [SerializeField]
    private float activeTime = .1f;
    private float timeActivated;
    private float alpha;
    [SerializeField]
    private float alphaSet = .8f;
    private float alphaMultiplier = .85f;

    private Transform player;

    private SpriteRenderer SR;
    private SpriteRenderer playerSR;

    private Color color;

    private void OnEnable()
    {
        SR.GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();
        alpha = alphaSet;
        SR.sprite = playerSR.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        timeActivated = Time.time;

    }

    private void Update()
    {
        alpha *= alphaMultiplier;
        color = new Color(1f, 1f, alpha);

        if (Time.time >= (timeActivated + activeTime))
        {

        }
    }
}
