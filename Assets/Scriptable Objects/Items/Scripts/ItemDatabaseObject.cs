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
    public Dictionary<int, ItemObject> GetItem = new Dictionary<int, ItemObject>(); //double dictionary used to retrieve an item when putting in an ID

    //Code to fire before and after Unity Serializes an object
    public void OnAfterDeserialize()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].Id = i; //makes Items Id always set during serialization
            GetItem.Add(i, Items[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        GetItem = new Dictionary<int, ItemObject>();//clears dictionary so we don't duplicate items
    }
}
