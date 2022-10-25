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

        if(_item.buffs.Length > 0) //to make an item not stackable we are just checking if it has buffs, i.e. if it already exists and then won't increase the amount but make a new item entry
        {
            //if the item has a buff it will add it to the container and return, if it doesn't it will follow the original code below
            Container.Items.Add(new InventorySlot(_item.Id, _item, _amount)); //add new item to inventory slot with defined amount, pulls item ID and populates into inventory slot
            return;
        }

        for (int i = 0; i < Container.Items.Count; i++)
        {
            if(Container.Items[i].item.Id == _item.Id)
            {
                Container.Items[i].AddAmount(_amount); //add amount to existing item in container
                return; //don't need to continue going through container
            }
        }
        Container.Items.Add(new InventorySlot(_item.Id,_item, _amount)); //add new item to inventory slot with defined amount, pulls item ID and populates into inventory slot
    }

    [ContextMenu("Save")] //Able to go to inventory object > inspector > cog wheel > then can hit 'Save'
    public void Save()
    {
        //Saved to 'inventory.Save' file located here 'C:\Users\Landon Airey\AppData\LocalLow\DefaultCompany\MarchingCubeGame'

        // use json utility to serialize our scriptable object
        // use binary formatter and filestream in json utility to write string into that file and save to a given location
        string saveData = JsonUtility.ToJson(this, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        bf.Serialize(file, saveData);
        file.Close();

        // IFormatter used to prevent a user from easily modifying a json file to edit inventory information
        /*
        IFormatter formatter = new BinaryFormatter(); 
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, Container);
        stream.Close(); //close stream so that there are no memory leaks
        */
    }

    [ContextMenu("Load")]
    public void Load()
    {
        if(File.Exists(string.Concat(Application.persistentDataPath, savePath))) //check is file exists
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this); //convert file back to scriptable object
            file.Close();

            /*
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            Container = (Inventory)formatter.Deserialize(stream); //casting as type Inventory
            stream.Close(); //close stream so that there are no memory leaks
            */
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Container = new Inventory();
    }
}

[System.Serializable]
public class Inventory
{
    //Moved Container to its own class so that we can pull just the data we need from the inventory slots instead of everything that's on the inventory object
    public List<InventorySlot> Items = new List<InventorySlot>();
}

[System.Serializable]
public class InventorySlot
{
    public int ID;
    public Item item; //changing this to hold items and not item objects
    public int amount;
    public InventorySlot(int _int, Item _item, int _amount)
    {
        ID = _int;
        item = _item;
        amount = _amount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
}