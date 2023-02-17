using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary; //used for save/load
using System.IO; //used for Filestream in save/load
using UnityEditor;
using System.Runtime.Serialization; //used for IFormatter

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public string savePath; //using a string because we can have multiple inventories and save them to different paths, used "/inventory.Save" as a test
    public ItemDatabaseObject database; //can make private if you want the Json Utility will skip over it (only when setting the database from within the Unity editor)
    public Inventory Container;


    public void AddItem(Item _item, int _amount)
    {
        //using a list now but may switch to a dictionary later
        //first check if you have the item in your inventory

        if (_item.buffs.Length > 0) //to make an item not stackable we are just checking if it has buffs, i.e. if it already exists and then won't increase the amount but make a new item entry
        {
            //if the item has a buff it will add it to the container and return, if it doesn't it will follow the original code below
            SetEmptySlot(_item, _amount); //add new item to inventory slot with defined amount, need to find the first empty item slot to add an item to
            return;
        }

        for (int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].ID == _item.Id)
            {
                Container.Items[i].AddAmount(_amount); //add amount to existing item in container
                return; //don't need to continue going through container
            }
        }
        SetEmptySlot(_item, _amount); //add new item to inventory slot with defined amount, need to find the first empty item slot to add an item to
    }

    public InventorySlot SetEmptySlot(Item _item, int _amount) //sets the first empty inventory slot
    {
        for (int i = 0; i < Container.Items.Length; i++) //loop through all slots
        {
            if(Container.Items[i].ID <= -1) //check if slot is -1 which means it's empty
            {
                Container.Items[i].UpdateSlot(_item.Id, _item, _amount);
                return Container.Items[i];
            }
        }
        //setup functionality for when inventory is full
        return null;
    }

    public void MoveItem(InventorySlot item1, InventorySlot item2)
    {
        InventorySlot temp = new InventorySlot(item2.ID, item2.item, item2.amount);//temp inventoryslot will have the same values as item2
        item2.UpdateSlot(item1.ID, item1.item, item1.amount);
        item1.UpdateSlot(temp.ID, temp.item, temp.amount);

    }

    public void RemoveItem(Item _item)
    {
        for (int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].item == _item) //if the item in our inventory is equal to the item that we are trying to remove
            {
                Container.Items[i].UpdateSlot(-1, null, 0); //clear the item from our inventory
            }
        }

    }

    [ContextMenu("Save")] //Able to go to inventory object > inspector > cog wheel > then can hit 'Save'
    public void Save()
    {
        //Saved to 'inventory.Save' file located here 'C:\Users\Landon Airey\AppData\LocalLow\DefaultCompany\MarchingCubeGame'

        // use json utility to serialize our scriptable object
        // use binary formatter and filestream in json utility to write string into that file and save to a given location
        /*
        string saveData = JsonUtility.ToJson(this, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        bf.Serialize(file, saveData);
        file.Close();
        */

        // IFormatter used to prevent a user from easily modifying a json file to edit inventory information
        //*
        IFormatter formatter = new BinaryFormatter(); 
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, Container);
        stream.Close(); //close stream so that there are no memory leaks
        //*/
    }

    [ContextMenu("Load")]
    public void Load()
    {
        if(File.Exists(string.Concat(Application.persistentDataPath, savePath))) //check is file exists
        {
            /*
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this); //convert file back to scriptable object
            file.Close();
            */
            
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream); //casting as type Inventory
            for (int i = 0; i < Container.Items.Length; i++)
            {
                Container.Items[i].UpdateSlot(newContainer.Items[i].ID, newContainer.Items[i].item, newContainer.Items[i].amount); //look through new container and update our current container
            }
            stream.Close(); //close stream so that there are no memory leaks
            
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Container.Clear();
    }
}

[System.Serializable]
public class Inventory
{
    //Moved Container to its own class so that we can pull just the data we need from the inventory slots instead of everything that's on the inventory object
    //public List<InventorySlot> Items = new List<InventorySlot>(); //changing from list to an array, a list can be modified at runtime but an array needs to initialized
    public InventorySlot[] Items = new InventorySlot[24]; //default to 24 slots, can define a different size though

    //Clears all of the items in an inventory object while maintaining the size and allowed item settings
    public void Clear()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].UpdateSlot(-1, new Item(), 0);
        }
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemType[] AllowedItems = new ItemType[0]; //array of itemtypes is a list of allowed items in an inventory slot, zero means any item is allowed
    public UserInterface parent; //reference to the parent of the user interface. Used when you drag an item from the player's UI to the equipment UI, when you drop it on an inventory slot in the equipment UI it needs to look up in the player's database of items to tell which item to populate in the equipment database
    public int ID = -1; //set outside of the bounds of actual item IDs so that -1 means an empty inventory slot
    public Item item; //changing this to hold items and not item objects
    public int amount;
    public InventorySlot() //default descriptor that will fire and will be used for initializing empty inventory slot inventory
    {
        ID = -1;
        item = null;
        amount = 0;
    }
    public InventorySlot(int _int, Item _item, int _amount)
    {
        ID = _int;
        item = _item;
        amount = _amount;
    }
    public void UpdateSlot(int _int, Item _item, int _amount)
    {
        ID = _int;
        item = _item;
        amount = _amount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
    public bool CanPlaceInSlot(ItemObject _item)
    {
        Debug.Log("CanPlaceInSlot HERE");
        if (AllowedItems.Length <= 0)
        {
            Debug.Log("AllowedItems.Length" + AllowedItems.Length.ToString());
            return true;
        }
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if (_item.type == AllowedItems[i])
            {
                Debug.Log(_item.type.ToString());
                return true;
            }
        }
        return false; //if it never hit a true above then return false
    }

}