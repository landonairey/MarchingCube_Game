using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; //needed for events related to the button on the SlotPrefab
using UnityEngine.EventSystems; //needed for events related to the button on the SlotPrefab

public class DynamicInterface : UserInterface
{
    public GameObject inventoryPrefab; //need to have one prefab that all inventory items use
    public int X_START;
    public int Y_START;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    public override void CreateSlots() //need to override the method in our base class
    {
        // For Parts 1-4:
        /*
        for (int i = 0; i < inventory.Container.Items.Count; i++)
        {
            InventorySlot slot = inventory.Container.Items[i]; //helps refactor code

            var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventory.database.GetItem[slot.item.Id].uiDisplay;
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0"); //n0 formats with commas above 1000
            itemsDisplayed.Add(slot, obj);
        }
        */

        itemsDisplayed = new Dictionary<GameObject, InventorySlot>(); //create new dictionary, shouldn't need to do this, just a precaution
        for (int i = 0; i < inventory.Container.Items.Length; i++)
        {
            var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            // inventoryPrefab is the prefab created in the editor that you want to spawn
            // Vector3.zero is giving an initial position of zero
            // Quaternion.identity gives it a rotation of zero
            // transform sets the parent of the object that we're spawning to the transform of the object our display inventory script is attached to

            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); }); //obj is the InventoryPrefab button object, delgate allow you to use a fuction that's also passing a variable
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            itemsDisplayed.Add(obj, inventory.Container.Items[i]); //need to add inventoryPrefab to the dictionary we have
        }
    }
    private Vector3 GetPosition(int i)
    {
        return new Vector3(X_START + X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN), Y_START - Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN), 0f);
    }
}
