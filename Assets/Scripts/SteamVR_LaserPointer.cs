//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
//using Varjo;
//using Varjo.Valve.VR;

namespace Valve.VR.Extras
{
    public class SteamVR_LaserPointer : MonoBehaviour
    {
        public bool _triggerPressed;
        public bool pressEvent;
        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        public bool addRigidBody = false;
        public Transform reference;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;
        public event PointerEventHandler PointerClick;
        public event PointerEventHandler PointerDown;

        public GameObject bigViewRef;
        private bool bigViewInstantiated = false;
        private bool pointingAtPanel = false;
        private bool pointingAtView = false;
        private float bigViewY = 3.4f;

        private bool isActive = false;

        private Ray raycast;
        private RaycastHit hit;
        private bool bHit;

        Transform previousContact = null;

        private void Start()
        {
            holder = new GameObject();
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);
        }

        public virtual void OnPointerClick(PointerEventArgs e)
        {
            if (PointerClick != null)
                PointerClick(this, e);
        }

        public virtual void OnPointerDown(PointerEventArgs e)
        {
            if (PointerDown != null)
                PointerDown(this, e);
        }

        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);
        }

        private void Update()
        {
            if (!isActive)
            {
                isActive = true;
                this.transform.GetChild(0).gameObject.SetActive(true);
            }

            float dist = 100f;

            raycast = new Ray(transform.position, transform.forward);
            bHit = Physics.Raycast(raycast, out hit);

            if (previousContact && previousContact != hit.transform)
            {
                //ON POINTER OUT
                PointerEventArgs args = new PointerEventArgs();
                args.controllerSource = this.gameObject;
                args.distance = 0f;
                args.flags = 0;
                args.target = previousContact;
                OnPointerOut(args);
                previousContact = null;
                pointingAtPanel = false;
                pointingAtView = false;
            }
            if (bHit && previousContact != hit.transform)
            {
                //ON POINTER IN
                PointerEventArgs argsIn = new PointerEventArgs();
                argsIn.controllerSource = this.gameObject;
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;

                if (hit.transform.tag == "panel") pointingAtPanel = true;
                if (hit.transform.tag == "bigview") pointingAtView = true;
            }
            if (!bHit)
            {
                previousContact = null;
            }
            if (bHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }
            if (_triggerPressed)
            {
                pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
                pointer.GetComponent<MeshRenderer>().material.color = clickColor;
            }
            else
            {
                pointer.transform.localScale = new Vector3(thickness, thickness, dist);
                pointer.GetComponent<MeshRenderer>().material.color = color;
            }
            pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        }

        public void TriggerDown()
        {
            if (bHit)
            {
                PointerEventArgs argsDown = new PointerEventArgs();
                argsDown.controllerSource = this.gameObject;
                argsDown.distance = hit.distance;
                argsDown.flags = 0;
                argsDown.target = hit.transform;
                OnPointerDown(argsDown);
            }
        }

        public void TriggerClick()
        {
            if (bHit)
            {
                PointerEventArgs argsClick = new PointerEventArgs();
                argsClick.controllerSource = this.gameObject;
                argsClick.distance = hit.distance;
                argsClick.flags = 0;
                argsClick.target = hit.transform;
                OnPointerClick(argsClick);
            }
        }

        public void OnTouchpadDown()
        {
            //If the tickets did have an extensive description string in their panel metadata script, then you'd set bigViewRef's text component to
            // args.target.GetComponent<PanelMetadata>().description (or whatever the var is called) from the onpointerin section above.
            if (!bigViewInstantiated && pointingAtPanel)
            {
                GameObject.Instantiate(bigViewRef, new Vector3(0, bigViewY, 0), new Quaternion(0, 0, 0, 0));
                bigViewInstantiated = true;
            }
            if (pointingAtView && bigViewRef != null)
            {
                Destroy(GameObject.FindGameObjectWithTag("bigview"));
                bigViewInstantiated = false;
            }

        }

    }

    public struct PointerEventArgs
    {
        public GameObject controllerSource;
        public uint flags;
        public float distance;
        public Transform target;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);
}