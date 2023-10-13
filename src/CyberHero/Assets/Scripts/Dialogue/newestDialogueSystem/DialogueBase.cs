using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueBase : MonoBehaviour
{
    public bool finished { get; private set; }
    protected IEnumerator WriteText(string input,
                                    string charName,
                                    TMP_Text textHolder,
                                    TMP_Text charText,
                                    Color textColor,
                                    float delay,
                                    // AudioClip sound,
                                    float delayBetweenLines)
    {
        textHolder.color = textColor;
        charText.text = charName;
        for (int i = 0; i < input.Length; i++)
        {
            textHolder.text += input[i];
            // AudioManager.instance.PlaySFX(sound);
            yield return new WaitForSeconds(delay);
        }
        //auto wait between lines
        yield return new WaitForSeconds(delayBetweenLines);

        // wait until mouse input
        // yield return new WaitUntil(()=>Input.GetMouseButton(0));

        finished = true;
    }
}
