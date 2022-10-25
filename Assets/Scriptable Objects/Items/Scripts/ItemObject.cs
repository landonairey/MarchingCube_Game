using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Food,
    Equipment,
    Default
}

public enum Attributes
{
    Agility,
    Intellect,
    Stamina,
    Strength
}

public abstract class ItemObject : ScriptableObject
{
    public int Id;
    public Sprite uiDisplay; //sprite for UI display
    public ItemType type;
    [TextArea(15,10)] //makes it easier to read inside the editor
    public string description;
    public ItemBuff[] buffs; //array of buffs for each item

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
}

[System.Serializable]
public class Item
{
    public string Name;
    public int Id;
    public ItemBuff[] buffs;
    public Item(ItemObject item) //constructor for Item. Passing in item object, that should have the values of the items
    {
        Name = item.name;
        Id = item.Id;
        buffs = new ItemBuff[item.buffs.Length];
        for (int i = 0; i < buffs.Length; i++)
        {
            buffs[i] = new ItemBuff(item.buffs[i].min, item.buffs[i].max);
            buffs[i].attribute = item.buffs[i].attribute;
        }
    }
}

[System.Serializable]
public class ItemBuff
{
    public Attributes attribute;
    public int value;
    public int min;
    public int max;
    public ItemBuff(int _min, int _max) //constructor for ItemBuff
    {
        min = _min;
        max = _max;
        GenerateValue();
    }

    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }
}