using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

public class TriggerEvent : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{

    //Much of the commented code in here is from my snapping system, which ended up too buggy for me to get into the MVP. Basically, when you drag a panel into the child object (called
    // a snappoint) of another panel, the panel you dragged would snap to that snappoint and then all overlapping snappoints would be disabled to allow for more panels to be snapped
    //to the joined panels around the outside.

    public Plane plane;
    public GameObject controllerR;
    public GameObject controllerL;

    private GameObject currentController;
    private Ray raycast;
    private bool dragging = false;
    private bool collided = false;
    private Collider collidedWith;
    private SnapPointParam otherParam;
    //private bool isLerp = false;
    GameObject empty;

    //InitialZ is the place panels will rest when not being dragged
    private float initialZ;
    //PlaneZ is the plane that panels are dragged along (always in front of initialZ)
    private float planeZ;

    // Start is called before the first frame update
    void Start()
    {
        planeZ = 0f;
        //initialZ = transform.position.z;
        initialZ = -0.042f;
        //plane = new Plane(Vector3.up, Vector3.up * planeY);
        plane = new Plane(Vector3.forward, Vector3.forward * planeZ);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("collided: " + collided);
        if (!dragging) return;
        else
        {
            //Drag along plane
            raycast = new Ray(currentController.transform.position, currentController.transform.forward);
            if (plane.Raycast(raycast, out float distance))
            {
                this.gameObject.transform.position = raycast.GetPoint(distance);
            }
        }
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (!dragging)
        {
            dragging = true;

            //Parent all children to an empty for now
            //empty = new GameObject("empty");
            //empty.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, empty.transform.position.z);
            /*Debug.Log("0: " + this.gameObject.transform.GetChild(0).gameObject);
            Debug.Log("1: " + this.gameObject.transform.GetChild(1).gameObject);
            Debug.Log("2: " + this.gameObject.transform.GetChild(2).gameObject);
            Debug.Log("3: " + this.gameObject.transform.GetChild(3).gameObject);
            Debug.Log("4: " + this.gameObject.transform.GetChild(4).gameObject);
            Debug.Log("5: " + this.gameObject.transform.GetChild(5).gameObject);*/
            //int childCount = this.gameObject.transform.childCount;
            //for (int i = 0; i < childCount; i++)
            //{
                //Debug.Log("Child: " + this.gameObject.transform.childCount);
            //    GameObject snapPoint = this.gameObject.transform.GetChild(0).gameObject;
                //Debug.Log("snap: " + snapPoint);
            //    if (snapPoint.tag == "snappoint")
            //    {
            //        snapPoint.transform.parent = empty.transform;
            //    }
            //}

            //Vector3 currentPos = transform.position;
            //currentPos.z += -1;
            //StartCoroutine(PickupPanel(currentPos, 1f));
            //Debug.Log("Coroutine finished.");
        }
        Debug.Log("clicked!");

        //need to reset snappoint positions
        //for (int i = 0; i < this.gameObject.transform.childCount; i++)
        //{
        //    GameObject snapPoint = this.gameObject.transform.GetChild(i).gameObject;
        //    if (snapPoint.tag == "snappoint")
        //    {
        //        snapPoint.transform.localPosition = new Vector3(snapPoint.transform.position.x, snapPoint.transform.position.y, snapZ);
        //    }
        //}
    }

    public void ReceiveController(PointerEventArgs e)
    {
        Debug.Log("receiving controller");
        currentController = e.controllerSource;
    }

    //Happens when you release the trigger
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Debug.Log("I am c l i c k");
        //Letting go of the trigger
        if (dragging) dragging = false;

        //Snap to point if panel is still colliding
        /*if (collided)
        {
            Debug.Log("snapping");
            if (!otherParam.getSnapped())
            {
                this.transform.position = collidedWith.transform.position;
                otherParam.setSnapped(true);

                //Check for overlapping snappoints and disable them
                SnappointState(false);
            }
        }      

        //Loop through empty object's snappoints and parent them back to panel
        int childCount = empty.gameObject.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            //Debug.Log("Child: " + this.gameObject.transform.childCount);
            GameObject snapPoint = empty.gameObject.transform.GetChild(0).gameObject;
            //Debug.Log("snap: " + snapPoint);
            snapPoint.transform.parent = this.transform;
            //snapPoint.transform.position = new Vector3(snapPoint.transform.position.x, snapPoint.transform.position.y, this.transform.position.z);
        }
        Destroy(empty);
        */

        transform.position = new Vector3(transform.position.x, transform.position.y, initialZ);
    }

    //Handle snapping collisions
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "snappoint")
        {
            otherParam = other.GetComponent<SnapPointParam>();
            //if (!collided)
            //{
            collided = true;
            collidedWith = other;
            //}
            Debug.Log("Entering," + collided);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "snappoint")
        {
            collided = false;
            otherParam.setSnapped(false);

            //When no longer colliding with a snappoint, reenable all snappoints
            //SnappointState(true);
        }
    }

    //These functions were for animating the panels when you hovered over them; I believe they were too slow to feel tactile and made the panel dragging more cumbersome.
    //Leaving them in incase anyone wants to take a look at them.

    /*public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Float outwards
        //if (transform.position.z != planeZ && coroutine == null) coroutine = StartCoroutine(HoverAnimation(new Vector3(transform.position.x, transform.position.y, planeZ), 0.05f, Time.time));
        if (transform.position.z != planeZ) StartCoroutine(HoverAnimation(new Vector3(transform.position.x, transform.position.y, planeZ), 0.05f, Time.time));
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //Float back inwards
        //if (transform.position.z != initialZ && coroutine == null) coroutine = StartCoroutine(HoverAnimation(new Vector3(transform.position.x, transform.position.y, initialZ), 0.05f, Time.time));
        if (transform.position.z != initialZ) StartCoroutine(HoverAnimation(new Vector3(transform.position.x, transform.position.y, initialZ), 0.05f, Time.time));
    }*/

    
    /*private IEnumerator PickupPanel(Vector3 endingPos, float time)
    {
        Vector3 startingPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, endingPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            Debug.Log("pos " + transform.position);
            yield return new WaitForEndOfFrame();
        }
    }



    private IEnumerator HoverAnimation(Vector3 endingPos, float time, float timeStarted)
    {
        Vector3 startingPos = transform.position;
        float timeElapsed = 0.0f;
        float percentDone = 0.0f;

        while (percentDone <= 1.0f)
        {
            transform.position = Vector3.Slerp(startingPos, endingPos, percentDone);
            timeElapsed = Time.time - timeStarted;
            percentDone = timeElapsed / time;
            if (percentDone >= 0.95f)
            {
                transform.position = endingPos;
            }
            yield return new WaitForEndOfFrame();
        }
    }


    //This was part of the snapping system, used for enabling and disabling snappoints when a snap is made.
    private void SnappointState(bool state)
    {
        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
            GameObject snapPoint = this.gameObject.transform.GetChild(i).gameObject;
            if (snapPoint.tag == "snappoint")
            {
                Debug.Log("snap: " + snapPoint + " overlap: " + snapPoint.GetComponent<SnapPointParam>().getOverlapping());
                if (snapPoint.GetComponent<SnapPointParam>().getOverlapping())
                {
                    if (!state)
                    {
                        //If there is a snappoint overlapping with another snappoint then disable it
                        snapPoint.SetActive(false);
                    } else
                    {
                        snapPoint.SetActive(true);
                    }
                    
                }
            }
        }
    }*/
}
