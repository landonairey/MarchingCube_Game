using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//from https://answers.unity.com/questions/943444/how-to-see-mesh-in-game-mode.html
[RequireComponent(typeof(Camera))]
public class WireframeViewer : MonoBehaviour
{
    public Color wireframeBackgroundColor = Color.black;
    public bool isInWireframeMode;
    private Camera cam;
    private CameraClearFlags origCameraClearFlags;
    private Color origBackgroundColor;
    private bool previousMode;

    private void Start()
    {
        cam = GetComponent<Camera>();
        origCameraClearFlags = cam.clearFlags;
        origBackgroundColor = cam.backgroundColor;
        previousMode = isInWireframeMode;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isInWireframeMode = !isInWireframeMode;
        }

        if (isInWireframeMode == previousMode)
        {
            return;
        }

        previousMode = isInWireframeMode;
        if (isInWireframeMode)
        {
            origCameraClearFlags = cam.clearFlags;
            origBackgroundColor = cam.backgroundColor;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = wireframeBackgroundColor;
        }
        else
        {
            cam.clearFlags = origCameraClearFlags;
            cam.backgroundColor = origBackgroundColor;
        }
    }

    private void OnPreRender()
    {
        if (isInWireframeMode)
        {
            GL.wireframe = true;
        }
    }

    private void OnPostRender()
    {
        if (isInWireframeMode)
        {
            GL.wireframe = false;
        }
    }
}
