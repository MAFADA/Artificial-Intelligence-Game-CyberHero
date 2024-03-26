using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLine : DialogueBase
{
    private TMP_Text textHolder;

    [Header("Text Options")]
    [SerializeField] private string input;
    [SerializeField] private Color textColor;

    [Header("Text Time Parameter")]
    [SerializeField] private float textSpeed;
    [SerializeField] private float textDelayBetweenLines;

    [Header("SFX")]
    // [SerializeField] private AudioClip sound;

    [Header("Character Image")]
    [SerializeField] private Sprite characterSprite;
    [SerializeField] private Image imageHolder;
    [SerializeField] private string charcaterName;
    [SerializeField] private TMP_Text charcaterText;
    private void Awake()
    {
        textHolder = GetComponent<TMP_Text>();
        textHolder.text = "";
        charcaterText.text = "";

        imageHolder.sprite = characterSprite;
        imageHolder.preserveAspect = true;
    }

    private void Start()
    {
        StartCoroutine(WriteText(input,
                               charcaterName ,
                                 textHolder,
                                 charcaterText,
                                 textColor,
                                 textSpeed,
                                 //  sound,
                                 textDelayBetweenLines));
    }
}
