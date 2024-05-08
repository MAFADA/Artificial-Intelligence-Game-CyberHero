using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueEndSubscriber : MonoBehaviour
{
    public UnityEvent someEvent; 

    private void OnEnable()
    {
        // Subscribe to the OnDialogueEnd event when this object is enabled
        DialogueController.OnDialogueEnd += HandleDialogueEnd;
    }

    private void OnDisable()
    {
        // Unsubscribe from the OnDialogueEnd event when this object is disabled
        DialogueController.OnDialogueEnd -= HandleDialogueEnd;
    }

    void HandleDialogueEnd()
    {
       someEvent.Invoke();
    }
}
