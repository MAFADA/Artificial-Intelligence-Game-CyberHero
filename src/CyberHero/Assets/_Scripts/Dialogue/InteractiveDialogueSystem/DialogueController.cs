using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public Image charaSprite;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public Animator animator;

    public Queue<Sprite> charaSprites;
    public Queue<string> nameChara;
    public Queue<string> sentences;
    
    int index = 1;

    public delegate void DialogueEndAction();
    public static event DialogueEndAction OnDialogueEnd;

    void OnEnable()
    {
        charaSprites = new Queue<Sprite>();
        nameChara = new Queue<string>();
        sentences = new Queue<string>();
    }

    public void StartDialogue(GameDialogue[] dialogue)
    {
        animator.SetBool("isOpen", true);

        sentences.Clear();

      /*  nameText.text = dialogue[0].name;
        charaSprite = dialogue[0].charaSprite;*/

        for (int i = 0; i < dialogue.Length; i++)
        {
            nameChara.Enqueue(dialogue[i].name);
            sentences.Enqueue(dialogue[i].sentences);
            charaSprites.Enqueue(dialogue[i].charaSprite);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count==0)
        {
            EndDialogue();
            return;
        }
        Sprite spriteDialogue = charaSprites.Dequeue();
        charaSprite.sprite = spriteDialogue;

        string charaName =nameChara.Dequeue();
        nameText.text = charaName;

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter  in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        animator.SetBool("isOpen", false);

        OnDialogueEnd?.Invoke();
    }
}
