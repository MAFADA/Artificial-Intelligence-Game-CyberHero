using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;

    public ItemDescription itemDescription;

    [Header("UI")]
    public Image image;
    public TMP_Text countText;

    [HideInInspector] public Transform parentAfterDrag;

    private void Awake()
    {
        itemDescription = GetComponentInParent<ItemDescription>();
    }

    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    public void ViewDescription()
    {
        itemDescription.ViewDescription(item);
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }
}
