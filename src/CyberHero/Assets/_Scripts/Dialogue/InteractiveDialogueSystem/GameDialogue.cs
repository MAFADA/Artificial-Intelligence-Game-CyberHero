using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameDialogue
{
    public Sprite charaSprite;

    public string name;

    [TextArea(3,10)]
    public string sentences;
}
