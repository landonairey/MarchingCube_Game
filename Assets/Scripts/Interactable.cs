using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class that all items the Player will interact with will be derived from
//Items/ Chests/ Enemies

public class Interactable : MonoBehaviour
{
    public float radius = 3f; //how close the player needs to be from the item to interact with it
    public Transform interactionTransform; //i.e. can only interact with a chest from the front

    bool isFocus = false;
    Transform player;
    bool hasInteracted = false;

    public virtual void Interact() //we want this to be different for different items, that's why it's virtual. It allows you to call this method but it can be overrided
    {
        //this method is meant to be overwritten
        Debug.Log("Interacting with " + transform.name);
    }

    void Update()
    {
        if (isFocus && !hasInteracted) //only do distance check if you're within distance and haven't already interacted
        {
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            if (distance <= radius) //is player is within allowed interactable distance with item
            {
                Debug.Log("INTERACT");
                Interact();
                hasInteracted = true;
            }
        }
    }

    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false; //make sure to interact once when focused on an item
    }

    public void OnDeFocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;
    }

    //visualize the interactable radius with this sphere
    private void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
        {
            interactionTransform = transform;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionTransform.position, radius);
    }
}
