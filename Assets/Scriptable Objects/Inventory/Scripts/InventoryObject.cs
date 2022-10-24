using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary; //used for save/load
using System.IO; //used for Filestream in save/load
using UnityEditor;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject, ISerializationCallbackReceiver
{
    public string savePath; //using a string because we can have multiple inventories and save them to different paths, used "/inventory.Save" as a test
    private ItemDatabaseObject database; //made private so that the Json Utility will skip over it
    public List<InventorySlot> Container = new List<InventorySlot>();

    private void OnEnable()
    {
#if UNITY_EDITOR //only use this code IF we are in the editor, othewise it will break when trying to build this project
        database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath("Assets/Resources/Database.asset", typeof(ItemDatabaseObject)); //make sure database is set
#else
        database = Resources.Load<ItemDatabaseObject>("Database"); //database has to be in the Assets/Resources folder
#endif
    }

    public void AddItem(ItemObject _item, int _amount)
    {
        //using a list now but may switch to a dictionary later
        //first check if you have the item in your inventory

        for (int i = 0; i < Container.Count; i++)
        {
            if(Container[i].item == _item)
            {
                Container[i].AddAmount(_amount); //add amount to existing item in container
                return; //don't need to continue going through container
            }
        }
        Container.Add(new InventorySlot(database.GetId[_item],_item, _amount)); //add new item to inventory slot with defined amount, pulls item ID and populates into inventory slot
    }

    public void Save()
    {

        //use json utility to serialize our scriptable object
        //use binary formatter and filestream in json utility to write string into that file and save to a given location

        string saveData = JsonUtility.ToJson(this, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        bf.Serialize(file, saveData);
        file.Close();

    }

    public void Load()
    {
        if(File.Exists(string.Concat(Application.persistentDataPath, savePath))) //check is file exists
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this); //convert file back to scriptable object
            file.Close();
        }
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < Container.Count; i++)
        {
            Container[i].item = database.GetItem[Container[i].ID]; //need to put an ID in and retreive an item
        }
    }

    public void OnBeforeSerialize()
    {
    }
}

[System.Serializable]
public class InventorySlot
{
    public int ID;
    public ItemObject item;
    public int amount;
    public InventorySlot(int _int, ItemObject _item, int _amount)
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