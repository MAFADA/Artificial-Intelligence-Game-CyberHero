using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDialogueTrigger : MonoBehaviour
{
    public GameDialogue[] dialogues;

    void Start()
    {
        FindObjectOfType<DialogueController>().StartDialogue(dialogues);
    }
}
