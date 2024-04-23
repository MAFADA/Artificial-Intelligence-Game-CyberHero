using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveGameDialogueTrigger : MonoBehaviour
{
    public GameDialogue[] dialogues;

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueController>().StartDialogue(dialogues);
    }
}
