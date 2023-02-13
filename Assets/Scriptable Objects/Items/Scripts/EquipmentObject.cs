using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Object", menuName = "Inventory System/Items/Equipment")]
public class EquipmentObject : ItemObject
{
    //public float atkBonus; //optional, you could use these if needed
    //public float defenceBonus; //optional, you could use these if needed
    public void Awake()
    {
        type = ItemType.Chest;
    }

}
