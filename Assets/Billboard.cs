using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Billboard effect to make the sprite face the camera however the camera is rotated
public class Billboard : MonoBehaviour
{
    public Camera _camera; //provides a more optimal way to find the camera object. If we didn't have this, the Camera.main call below would have to do an object.find constantly in our scene to find the camera
    private void LateUpdate()
    {
        
        transform.forward = _camera.transform.forward;
    }
}
