using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescription : MonoBehaviour
{
    public GameObject description;
    public Image itemSprite;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public void ViewDescription(Item item)
    {
        if(this.transform.childCount > 0)
        {
            description.gameObject.SetActive(true);
            itemSprite.sprite = item.image;
            titleText.text = item.itemName;
            descriptionText.text = item.itemDescription;
        }
    }
}
