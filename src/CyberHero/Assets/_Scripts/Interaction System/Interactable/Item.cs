using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    [Header("Only Gameplay")]
    [SerializeField] public ItemType type;
    [SerializeField] public Vector2Int range = new Vector2Int(5, 4);

    [Header("Both")]
    [SerializeField] public Sprite image;
    [SerializeField] public string itemName;
    [TextArea(15,20)]
    [SerializeField] public string itemDescription;
}

public enum ItemType
{
    UseableItem,
    Tool,
    ItemHint
}
