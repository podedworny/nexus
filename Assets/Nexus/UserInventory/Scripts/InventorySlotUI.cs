using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int slotIndex;
    public InventoryUI inventoryUI;
    
    private Image iconImage;
    private GameObject draggedIcon;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();

        Transform iconTransform = transform.Find("Icon");
        if (iconTransform != null)
        {
            iconImage = iconTransform.GetComponent<Image>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI != null) inventoryUI.SelectSlot(slotIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotIndex == 0) return; 
        
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null) iconImage = iconTransform.GetComponent<Image>();
        }

        if (iconImage == null || iconImage.sprite == null) return; 

        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(canvas.transform, false);
        draggedIcon.transform.SetAsLastSibling(); 

        Image draggedImage = draggedIcon.AddComponent<Image>();
        draggedImage.sprite = iconImage.sprite;
        draggedImage.raycastTarget = false; 
        
        draggedImage.rectTransform.sizeDelta = new Vector2(iconImage.rectTransform.rect.width, iconImage.rectTransform.rect.height);
        draggedImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        draggedImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        draggedImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        draggedImage.rectTransform.localScale = Vector3.one;
        
        draggedIcon.transform.position = Mouse.current.position.ReadValue();

        iconImage.color = new Color(1, 1, 1, 0.5f); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = Mouse.current.position.ReadValue();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null) Destroy(draggedIcon);
        
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null) iconImage = iconTransform.GetComponent<Image>();
        }

        if (iconImage != null && iconImage.sprite != null)
        {
            iconImage.color = new Color(1, 1, 1, 1f); 
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (slotIndex == 0) return; 

        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null)
        {
            InventorySlotUI sourceSlot = droppedObject.GetComponent<InventorySlotUI>();
            if (sourceSlot != null && sourceSlot.slotIndex != 0)
            {
                inventoryUI.RequestSwap(sourceSlot.slotIndex, this.slotIndex);
            }
        }
    }
}