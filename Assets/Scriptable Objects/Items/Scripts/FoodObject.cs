using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Object", menuName = "Inventory System/Items/Food")]
public class FoodObject : ItemObject
{
    //public int restoreHealthValue; //optional, you could use these if needed
    public void Awake()
    {
        type = ItemType.Food;
    }
}
