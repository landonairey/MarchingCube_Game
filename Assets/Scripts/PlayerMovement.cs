using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController controller;
    public float speed = 12f;
    Vector3 velocity;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;
    public bool isTerrainEdit = false;
    public GameObject EditSpherePrefab;
    GameObject EditSphere;
    public float editScale = 4; //diameter of Edit Sphere
    public float editDistance = 8; //distance that the EditSphere can be used.
    float raycastDistance;
    GameObject EditVerticesCollection;
    public GameObject EditVerticesCollectionPrefab;
    public GameObject EditVertexPrefab;

    Camera cam;
    public WorldGenerator world;
    public LayerMask itemMask;
    public Interactable focus;

    public float volume = 0f;

    private void Start()
    {
        cam = Camera.main;
        //this.EditSphere = new GameObject();
        this.EditSphere = Instantiate(EditSpherePrefab, Vector3.zero, Quaternion.identity);
        this.EditSphere.transform.localScale = editScale * Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        //If the cursor is over the canvas UI then don't control the player
        /*
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        */

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; //becasue this check may happen just before the play rewached the ground, this forces them to the ground
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); //multiply by time again because physics: x = 1/2g*t^2

        if (Input.GetKeyDown(KeyCode.E))
        {
            isTerrainEdit = !isTerrainEdit;

            //If going from true to false then hide EditSphere and reset transform
            if (isTerrainEdit == false)
            {
                this.EditSphere.GetComponent<MeshRenderer>().enabled = false;
                this.EditSphere.transform.position = Vector3.zero;
            }

            //If going from false to true then render EditSphere
            if (isTerrainEdit == true)
            {
                this.EditSphere.GetComponent<MeshRenderer>().enabled = true;
            }

            //var sphere = Instantiate(EditSpherePrefab, new Vector3(0,0,0), Quaternion.identity);
            //this.EditSpherePrefab = Instantiate(this.EditSpherePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }

        if (isTerrainEdit)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;

            //Adjust Edit Sphere size at run time
            this.EditSphere.transform.localScale = editScale * Vector3.one;

            List<Vector3Int> EditVerticesPositions = new List<Vector3Int>();

            if (Physics.Raycast(ray, out hit))
            {
                //Check how far away the player is attempting to use the EditSphere
                raycastDistance = Vector3.Distance(cam.transform.position, hit.point);
                //Debug.Log(distance);

                //if the raycast hit is within editable distance then draw the sphere at the hit point
                if (raycastDistance < editDistance)
                {
                    if (hit.transform.tag == "Terrain")
                    {
                        //Debug.Log("Hit Terrain");

                        //Move the EditSphere as you move the camera 
                        this.EditSphere.transform.position = hit.point;

                        //Simple approach, redraw edit vertices on update, and delete afterwards
                        //this EditVerticesCollection will be the parent to all instantiated vertices
                        this.EditVerticesCollection = Instantiate(EditVerticesCollectionPrefab, Vector3.zero, Quaternion.identity);

                        //Find positions of all of the active vertices
                        //EditVerticesPositions = hit.transform.GetComponent<Chunk>().FindEditVertices(hit.point, editScale); //won't work wihtou Chunk being MonoBehavior
                        //EditVerticesPositions = world.GetChunkFromVector3(hit.transform.position).FindEditVertices(hit.point, editScale);
                        EditVerticesPositions = world.FindEditVertices(hit.point, editScale); //change to use method from WorldGenerator instead of Chunk


                        //Create debug spheres at each vertex location under the EditVerticesCollection parent object
                        foreach (Vector3Int vertLocation in EditVerticesPositions)
                        {
                            //Instantiate what, where, rotation
                            var editVertex = Instantiate(
                                EditVertexPrefab,
                                vertLocation,
                                Quaternion.identity);
                            //sphere.transform.parent = DebugCube.transform;
                            editVertex.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            editVertex.transform.parent = this.EditVerticesCollection.transform;
                        }

                        // Left click to edit and place terrain
                        if (Input.GetMouseButtonDown(0))
                        {
                            //Debug.Log(string.Format("Hit Position: {0}, {1}, {2} ", hit.transform.position.x, hit.transform.position.y, hit.transform.position.z));
                            float deltaVol = world.HandleModifyTerrain(hit.point, editScale, 0);

                            volume = volume + deltaVol; //deltaVol should come through as negative here
                        }

                        // Left click to edit and remove terrain
                        if (Input.GetMouseButtonDown(1))
                        {
                            //Debug.Log("Hit Terrain");
                            Debug.Log(string.Format("Hit Position: {0}, {1}, {2} ", hit.transform.position.x, hit.transform.position.y, hit.transform.position.z));
                            //Edit terrain at every vertex position within the Edit Sphere position
                            //float deltaVol = hit.transform.GetComponent<Chunk>().RemoveManyTerrain(hit.point, editScale); //won't work wihtou Chunk being MonoBehavior
                            //float deltaVol = world.GetChunkFromVector3(hit.transform.position).RemoveManyTerrain(hit.point, editScale);
                            float deltaVol = world.HandleModifyTerrain(hit.point, editScale, 1);

                            volume = volume + deltaVol;
                        }
                    }
                    else
                    {
                        //Debug.Log("Hit Something Other than Terrain");
                    }
                }
                //if the raycast hit is outside editable distance then draw the sphere at the EditDistance
                else
                {
                    this.EditSphere.transform.position = ray.GetPoint(editDistance);
                }

            }
            //if the player is not looking at anything (i.e. the sky) then draw the sphere at the EditDistance
            else
            {
                //Debug.Log("Hit Nothing");

                this.EditSphere.transform.position = ray.GetPoint(editDistance);
            }
        }

        //Middle Mouse Button DEBUG
        if (Input.GetMouseButtonDown(2))
        {
            //creating ray from camera viewport position
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Terrain")
                {
                    Debug.Log("Hit Terrain");
                    //hit.transform.GetComponent<Chunk>().DebugTerrain(hit.point); //won't work wihtou Chunk being MonoBehavior
                    //world.GetChunkFromVector3(hit.transform.position).DebugTerrain(hit.point);
                    world.DebugTerrain(hit.point); //moved DebugTerrain function to WorldGenerator script
                }
                else
                    Debug.Log("Not Terrain Clicked");

            }
            else
                Debug.Log("Nothing Clicked");
        }

        void SetFocus(Interactable newFocus)
        {
            if (newFocus != focus)
            {
                //If you already had something focused before and you click on a new item, you need to defocus the previous one before focusing on the next one

                if (focus != null)
                {
                    focus.OnDeFocused();
                }
                focus = newFocus;

            }

            newFocus.OnFocused(transform);
        }

        void RemoveFocus()
        {
            if (focus != null)
            {
                focus.OnDeFocused();
            }

            focus = null;
        }


        // Delete vertices group
        Destroy(this.EditVerticesCollection, 0.05f);            
    }
}


