using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//About Unity Serialization:
// Serialization is the automatic process of transforming data structures or GameObject states into a format that Unity can store and reconstruct later.
// https://docs.unity3d.com/Manual/script-Serialization.html

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]

//Used so that we don't have to drag and drop an item database into every single scene that we want to use it is
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] Items; //Array of all object that exist in our game
    public Dictionary<ItemObject, int> GetId = new Dictionary<ItemObject, int>(); //dictionary to input an item and easily get a key of that item, ItemObject is the key and int is the value
    public Dictionary<int, ItemObject> GetItem = new Dictionary<int, ItemObject>(); //double dictionary used to retrieve an item when putting in an ID

    //Code to fire before and after Unity Serializes an object
    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<ItemObject, int>(); //clears dictionary so we don't duplicate items
        GetItem = new Dictionary<int, ItemObject>();
        for (int i = 0; i < Items.Length; i++)
        {
            GetId.Add(Items[i], i);
            GetItem.Add(i, Items[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        //No using
    }
}
