using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogueText;
    public UnityEvent OnDialogueEnd;

    private Queue<string> sentences;

    void Start()
    {
        sentences = new Queue<string>();        
    }

    public void StartDialogue(Dialogue dialogue){
        nameText.text = dialogue.name;

        sentences.Clear();

        foreach (string sentece in dialogue.sentences)
        {
            sentences.Enqueue(item: sentece);
        }

        DisplayNextSentences();
    }

    public void DisplayNextSentences()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence){
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    public void EndDialogue()
    {
        OnDialogueEnd.Invoke();
    }
}
