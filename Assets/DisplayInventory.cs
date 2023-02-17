using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; //needed for obj.transform.GetChild(0).GetComponentInChildren<Image>()
using UnityEngine.Events; //needed for events related to the button on the InventoryPrefab
using UnityEngine.EventSystems; //needed for events related to the button on the InventoryPrefab

public class DisplayInventory : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem(); //reference to Mouse UI Item

    public GameObject inventoryPrefab; //need to have one prefab that all inventory items use
    public InventoryObject inventory;
    public int X_START;
    public int Y_START;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;
    Dictionary<GameObject, InventorySlot> itemsDisplayed = new Dictionary<GameObject, InventorySlot>(); //Use gameobject as the key and return the slot that's linked to that object 

    // Start is called before the first frame update
    void Start()
    {
        CreateSlots();
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateDisplay(); // For Parts 1-4:

        UpdateSlots(); //Should only need to update the display of the inventory whenever you modify it, this is just for prototyping to update the inventory in the Update() function
    }

    public void UpdateSlots()
    {
        foreach (KeyValuePair<GameObject, InventorySlot> _slot in itemsDisplayed)
        {
            if(_slot.Value.ID >= 0) //if the slot has an item in it that we're looking at, then update the slot to display that item
            {
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventory.database.GetItem[_slot.Value.item.Id].uiDisplay; //only finding sprite when making display updates, otherwise you would need to save the sprite data when you save the inventory
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1); //white with 100% alpha, will be using alpha to turn on/off this white square when we have an item or have an empty space
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = _slot.Value.amount == 1 ? "" : _slot.Value.amount.ToString("n0"); //if our slot value is equal to 1 then don't show any text, if above 1 then show the text, see ternary operators in c#
            }
            else //otherwise there isn't an item present and we need to clear that slot out to display no items
            {
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null; //don't display any sprites
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0); //white with 0% alpha to turn off this white square when we have an empty space
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = ""; //no text to display when there are no items

            }
        }
    }

    public void UpdateDisplay()
    {
        // For Parts 1-4:
        /*
        for (int i = 0; i < inventory.Container.Items.Count; i++)
        {
            
            
            InventorySlot slot = inventory.Container.Items[i]; //helps refactor code

            if (itemsDisplayed.ContainsKey(slot)) //if that item already in our inventory
            {
                itemsDisplayed[slot].GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0");
            }
            else //if it's not already in our inventory then create it
            {
                var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventory.database.GetItem[slot.item.Id].uiDisplay;
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0"); //n0 formats with commas above 1000
                itemsDisplayed.Add(slot, obj);
            }
            
        }
        */
    }

    public void CreateSlots()
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

    //adding events to our button that exists on the InventoryPrefab object
    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>(); //grabbing even trigger from game object (built in Unity object)
        var eventTrigger = new EventTrigger.Entry(); //entry to event triggers
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj) //when we enter an inventory slot button
    {
        mouseItem.hoverObj = obj; //set our mouse hover object to the object of the inventory slot button
        if(itemsDisplayed.ContainsKey(obj)) //check that the object we are hovering over is actually an items displayed object
        {
            mouseItem.hoverItem = itemsDisplayed[obj]; //set the mouse item to that displayed item
        }
    }
    public void OnExit(GameObject obj)
    {
        //same as OnEnter but just null out what we set
        mouseItem.hoverObj = null; 
        mouseItem.hoverItem = null; 
    }
    public void OnDragStart(GameObject obj)
    {
        //visual representation of you clicking and dragging an item around the UI, the inventory system won't change until the move gesture is completed
        var mouseObject = new GameObject(); //object following the mouse, new gameobject instantiates an empty game object into the scene
        var rt = mouseObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50); //makes sure the sprite we are displaying is the same size as the item we are clicking on inside the inventory
        mouseObject.transform.SetParent(transform.parent);
        if (itemsDisplayed[obj].ID >= 0); // check if larger than 0 which means there is an item on that slot
        {
            var img = mouseObject.AddComponent<Image>();
            img.sprite = inventory.database.GetItem[itemsDisplayed[obj].ID].uiDisplay;
            img.raycastTarget = false; //set raycast of the object we are creating to false otherwise the sprite image recttransform we are creating will be in the way of our mouse and will never fire the mouse pointer enter on other inventory slot locations
        }
        mouseItem.obj = mouseObject;
        mouseItem.item = itemsDisplayed[obj];
    }
    public void OnDragEnd(GameObject obj)
    {
        Debug.Log("DisplayInventory OnDragEnd");
        if (mouseItem.hoverObj) //if the slot exists and is not null then the mouse if over a slot that the item can go into, this has to reach into our system
        {
            inventory.MoveItem(itemsDisplayed[obj], itemsDisplayed[mouseItem.hoverObj]);
        }
        else //the mouse is hovering over an invalid position
        {
            //remove the item, needs to reach into the system. If you drag the item out of the inventory onto the ground this is going to delete it. Here is where you need to make a different method if you want to then populate that item on the ground!
            inventory.RemoveItem(itemsDisplayed[obj].item);
        }
        Destroy(mouseItem.obj); //regardless of if you drop the item into a valid inventory slot or not, destroy the mouse UI Item
        mouseItem.item = null; //clear the item on the mouse item

    }
    public void OnDrag(GameObject obj)
    {
        //all this needs to do is update the mouseObject position to the position where our mouse is so that it follows
        if(mouseItem.obj != null) //if you have an item on your mouse then update the position of that item
        {
            mouseItem.obj.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    public Vector3 GetPosition(int i)
    {
        return new Vector3(X_START + X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN), Y_START - Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN), 0f);
    }

}