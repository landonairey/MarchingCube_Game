using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GroundItem : MonoBehaviour, ISerializationCallbackReceiver
{
    public ItemObject item;

    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = item.uiDisplay; //Set the sprite renderer of the child component to the ui display
        EditorUtility.SetDirty(GetComponentInChildren<SpriteRenderer>()); //Let's Unity know that something changed on this object and you are able to save it 
    }
}
