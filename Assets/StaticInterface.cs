using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; //needed for events related to the button on the SlotPrefab
using UnityEngine.EventSystems; //needed for events related to the button on the SlotPrefab

public class StaticInterface : UserInterface
{
    //the order of the equipment SlotPrefabs matter here, they prefab 1,2,3 etc need to match the array elements for this static inventory
    public GameObject[] slots;

    public override void CreateSlots()
    {
        //this is just to have a matching method for the abstract CreateSlots method in the UserInterface class
        itemsDisplayed = new Dictionary<GameObject, InventorySlot>(); //create new dictionary of items displayed. Makes sure there are no links between the equipment database and the equiment display
        
        for (int i = 0; i < inventory.Container.Items.Length; i++) //loop through all equipment in out database, which is the scriptable object
        {
            var obj = slots[i]; //create object that links to our array (slots) that will link to the actual game objects in our scene. Those slot prefabs will then be linked to the same slots in our database

            //add pointer events to the slots
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); }); //obj is the InventoryPrefab button object, delgate allow you to use a fuction that's also passing a variable
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            itemsDisplayed.Add(obj, inventory.Container.Items[i]); //add them to the items displayed
        }
    }
}
