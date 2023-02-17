using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; //needed for obj.transform.GetChild(0).GetComponentInChildren<Image>()
using UnityEngine.Events; //needed for events related to the button on the InventoryPrefab
using UnityEngine.EventSystems; //needed for events related to the button on the InventoryPrefab

public abstract class UserInterface : MonoBehaviour //we will always either use StaticInterface or DynamicInterface classes which are extensions of UserInterface. Abstract means we won't be able to directly use this UserInterface class on an object
{
    public Player player; //now we'll have to drag and drop the Player onto each instance

    public InventoryObject inventory;
    public Dictionary<GameObject, InventorySlot> itemsDisplayed = new Dictionary<GameObject, InventorySlot>(); //Use gameobject as the key and return the slot that's linked to that object 

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < inventory.Container.Items.Length; i++) //loop through all items in interface
        {
            inventory.Container.Items[i].parent = this; //link all items in the database to this interface as the parent so we can figure out which interface an inventory slot and items belongs to
        }
        CreateSlots();
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
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
            if (_slot.Value.ID >= 0) //if the slot has an item in it that we're looking at, then update the slot to display that item
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

    public abstract void CreateSlots();

    //adding events to our button that exists on the InventoryPrefab object
    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action) //protected makes this available to our extended classes but otherwise private
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>(); //grabbing even trigger from game object (built in Unity object)
        var eventTrigger = new EventTrigger.Entry(); //entry to event triggers
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj) //when we enter an inventory slot button
    {
        player.mouseItem.hoverObj = obj; //set our mouse hover object to the object of the inventory slot button
        if (itemsDisplayed.ContainsKey(obj)) //check that the object we are hovering over is actually an items displayed object
        {
            player.mouseItem.hoverItem = itemsDisplayed[obj]; //set the mouse item to that displayed item
        }
    }
    public void OnExit(GameObject obj)
    {
        //same as OnEnter but just null out what we set
        player.mouseItem.hoverObj = null;
        player.mouseItem.hoverItem = null;
    }

    public void OnEnterInterface(GameObject obj)
    {
        //Setup the UI that's on the player's item mouse
        player.mouseItem.ui = obj.GetComponent<UserInterface>();
    }

    public void OnExitInterface(GameObject obj)
    {
        //Un-Setup the UI that's on the player's item mouse
        player.mouseItem.ui = null;
    }

    public void OnDragStart(GameObject obj)
    {
        //visual representation of you clicking and dragging an item around the UI, the inventory system won't change until the move gesture is completed
        var mouseObject = new GameObject(); //object following the mouse, new gameobject instantiates an empty game object into the scene
        var rt = mouseObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50); //makes sure the sprite we are displaying is the same size as the item we are clicking on inside the inventory
        mouseObject.transform.SetParent(transform.parent);
        if (itemsDisplayed[obj].ID >= 0) ; // check if larger than 0 which means there is an item on that slot
        {
            var img = mouseObject.AddComponent<Image>();
            img.sprite = inventory.database.GetItem[itemsDisplayed[obj].ID].uiDisplay;
            img.raycastTarget = false; //set raycast of the object we are creating to false otherwise the sprite image recttransform we are creating will be in the way of our mouse and will never fire the mouse pointer enter on other inventory slot locations
        }
        player.mouseItem.obj = mouseObject;
        player.mouseItem.item = itemsDisplayed[obj];
    }
    public void OnDragEnd(GameObject obj)
    {
        //need to setup functions to make sure that the item can go into that slot (i.e. only helmets allowed in the top of the equipment menu)
        //also needs to handle cases where you are dropping an item on some over items that's already in the slot
        var itemOnMouse = player.mouseItem;
        var mouseHoverItem = itemOnMouse.hoverItem;
        var mouseHoverObj = itemOnMouse.hoverObj;
        var getItemObject = inventory.database.GetItem; //links it to the dictionary that's on the interface

        if (itemOnMouse.ui != null)
        {
            if (mouseHoverObj) //if the slot exists and is not null then the mouse if over a slot that the item can go into, this has to reach into our system
            {
                //Check if the item on the mouse and drop into the inventroy slot, i.e. if is the same type
                //Additional checks: if there's not an item it's fine to swap, if there is an item you need to make sure the item your swaping with can go into the slot
                if (mouseHoverItem.CanPlaceInSlot(getItemObject[itemsDisplayed[obj].ID]) && (mouseHoverItem.item.Id <= -1 || (mouseHoverItem.item.Id >= 0 && itemsDisplayed[obj].CanPlaceInSlot(getItemObject[mouseHoverItem.item.Id]))))
                {
                    //Debug.Log("UserInterface OnDragEnd");
                    inventory.MoveItem(itemsDisplayed[obj], mouseHoverItem.parent.itemsDisplayed[mouseHoverObj]); //hoverObj is the object we clicked on and are about to drop. Changed to access this overobj from the moust item's parent
                }
            }
        }
        else //the mouse is hovering over an invalid position
        {
            //remove the item, needs to reach into the system. If you drag the item out of the inventory onto the ground this is going to delete it. Here is where you need to make a different method if you want to then populate that item on the ground!
            inventory.RemoveItem(itemsDisplayed[obj].item);
        }
        Destroy(itemOnMouse.obj); //regardless of if you drop the item into a valid inventory slot or not, destroy the mouse UI Item
        itemOnMouse.item = null; //clear the item on the mouse item

    }
    public void OnDrag(GameObject obj)
    {
        //all this needs to do is update the mouseObject position to the position where our mouse is so that it follows
        if (player.mouseItem.obj != null) //if you have an item on your mouse then update the position of that item
        {
            player.mouseItem.obj.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
}

//separate class to have a reference to the mouse UI item so that all the button trigger methods can interact with it 
public class MouseItem
{
    public UserInterface ui;
    public GameObject obj;
    public InventorySlot item;
    public InventorySlot hoverItem;
    public GameObject hoverObj;
}