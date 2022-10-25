﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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
        if(Input.GetKeyDown(KeyCode.Space)) //test the save functionality
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
        inventory.Container.Items.Clear(); //clear the items in the iventory when you exit play
    }
}
