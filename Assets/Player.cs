using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem(); //reference to Mouse UI Item, need only ONE instance of this between the Static/Dynamic/UserInterfaces so we put in on the Player for now

    public InventoryObject inventory;

    public void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<GroundItem>();
        if(item) //check if we collided with type item
        {
            inventory.AddItem(new Item(item.item), 1); //add to inventory
            Destroy(other.gameObject); //destroy item we just interacted with
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightShift)) //test the save functionality
        {
            Debug.Log("Saving Inventory");
            inventory.Save();
        }
        if (Input.GetKeyDown(KeyCode.Return)) //test the save functionality
        {
            Debug.Log("Loading Inventory");
            inventory.Load();
        }
    }

    private void OnApplicationQuit()
    {
        //inventory.Container.Items.Clear(); //clear the items in the iventory when you exit play, for Parts 1-4
        inventory.Container.Items = new InventorySlot[24]; //clears the InventorySlots
    }
}
