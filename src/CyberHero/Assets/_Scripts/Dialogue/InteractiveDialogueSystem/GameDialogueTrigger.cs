using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDialogueTrigger : MonoBehaviour
{
    //public GameObject canvas;
    public GameDialogue[] dialogues;

    void OnEnable()
    {
        FindObjectOfType<DialogueController>().StartDialogue(dialogues);
    }
}
