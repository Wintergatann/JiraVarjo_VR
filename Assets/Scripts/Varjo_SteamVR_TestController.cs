//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Test SteamVR_Controller support.
//
//=============================================================================

// Modified By Varjo

using UnityEngine;
using System.Collections.Generic;
using Varjo.Valve.VR;
//using Valve.VR.Extras;

namespace Varjo
{
    public class Varjo_SteamVR_TestController : MonoBehaviour
    {
        List<int> controllerIndices = new List<int>();
        public bool triggerPressed = false;

        //public SteamVRLaserWrapper laserScript;
        //public GameObject laserObj;
        //public SteamVR_LaserPointer laserScr;

        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform reference;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;
        public event PointerEventHandler PointerClick;
        public event PointerEventHandler PointerDown;

        Transform previousContact = null;

        private void Start()
        {
            //laserScr = laserObj.GetComponent<SteamVR_LaserPointer>();

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

        private void OnDeviceConnected(int index, bool connected)
        {
            var system = OpenVR.System;
            if (system == null || system.GetTrackedDeviceClass((uint)index) != ETrackedDeviceClass.Controller)
                return;

            if (connected)
            {
                Debug.Log(string.Format("Controller {0} connected.", index));
                PrintControllerStatus(index);
                controllerIndices.Add(index);
            }
            else
            {
                Debug.Log(string.Format("Controller {0} disconnected.", index));
                PrintControllerStatus(index);
                controllerIndices.Remove(index);
            }
        }

        void OnEnable()
        {
            Varjo_SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
        }

        void OnDisable()
        {
            Varjo_SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
        }

        void PrintControllerStatus(int index)
        {
            var device = Varjo_SteamVR_Controller.Input(index);
            Debug.Log("index: " + device.index);
            Debug.Log("connected: " + device.connected);
            Debug.Log("hasTracking: " + device.hasTracking);
            Debug.Log("outOfRange: " + device.outOfRange);
            Debug.Log("calibrating: " + device.calibrating);
            Debug.Log("uninitialized: " + device.uninitialized);
            Debug.Log("pos: " + device.transform.pos);
            Debug.Log("rot: " + device.transform.rot.eulerAngles);
            Debug.Log("velocity: " + device.velocity);
            Debug.Log("angularVelocity: " + device.angularVelocity);

            var l = Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Leftmost);
            var r = Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Rightmost);
            Debug.Log((l == r) ? "first" : (l == index) ? "left" : "right");
        }

        EVRButtonId[] buttonIds = new EVRButtonId[] {
        EVRButtonId.k_EButton_ApplicationMenu,
        EVRButtonId.k_EButton_Grip,
        EVRButtonId.k_EButton_SteamVR_Touchpad,
        EVRButtonId.k_EButton_SteamVR_Trigger
    };

        EVRButtonId[] axisIds = new EVRButtonId[] {
        EVRButtonId.k_EButton_SteamVR_Touchpad,
        EVRButtonId.k_EButton_SteamVR_Trigger
    };

        void Update()
        {
            if (!isActive)
            {
                isActive = true;
                this.transform.GetChild(0).gameObject.SetActive(true);
            }

            float dist = 100f;

            Ray raycast = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            bool bHit = Physics.Raycast(raycast, out hit);

            if (previousContact && previousContact != hit.transform)
            {
                PointerEventArgs args = new PointerEventArgs();
                //args.fromInputSource = pose.inputSource;
                args.distance = 0f;
                args.flags = 0;
                args.target = previousContact;
                OnPointerOut(args);
                previousContact = null;
            }
            if (bHit && previousContact != hit.transform)
            {
                PointerEventArgs argsIn = new PointerEventArgs();
                //argsIn.fromInputSource = pose.inputSource;
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;
            }
            if (!bHit)
            {
                previousContact = null;
            }
            if (bHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }

            foreach (var index in controllerIndices)
            {
                foreach (var buttonId in buttonIds)
                {
                    if (Varjo_SteamVR_Controller.Input(index).GetPressDown(buttonId))
                        //Debug.Log("get press down");
                        //Debug.Log("trig: " + Varjo_SteamVR_Controller.Input(index).GetPressDown(buttonId));
                        //laserScript._triggerPressed = true;
                        //laserScr._triggerPressed = true;

                        triggerPressed = true;
                        /*PointerEventArgs argsDown = new PointerEventArgs();
                        //argsDown.fromInputSource = pose.inputSource;
                        argsDown.distance = hit.distance;
                        argsDown.flags = 0;
                        argsDown.target = hit.transform;
                        OnPointerDown(argsDown);*/

                    //Debug.Log(buttonId + " press down");
                    if (Varjo_SteamVR_Controller.Input(index).GetPressUp(buttonId))
                    {
                        //Debug.Log("get press up");
                        triggerPressed = false;
                        /*PointerEventArgs argsClick = new PointerEventArgs();
                        //argsClick.fromInputSource = pose.inputSource;
                        argsClick.distance = hit.distance;
                        argsClick.flags = 0;
                        argsClick.target = hit.transform;
                        OnPointerClick(argsClick);*/

                        //Debug.Log("trig: " + Varjo_SteamVR_Controller.Input(index).GetPressUp(buttonId));
                        //triggerPressed = false;
                        //laserScr._triggerPressed = false;
                        //Debug.Log(buttonId + " press up");
                        if (buttonId == EVRButtonId.k_EButton_SteamVR_Trigger)
                        {
                            Varjo_SteamVR_Controller.Input(index).TriggerHapticPulse();
                            PrintControllerStatus(index);
                        }
                    }
                    if (Varjo_SteamVR_Controller.Input(index).GetPress(buttonId))
                        Debug.Log(buttonId);
                }

                foreach (var buttonId in axisIds)
                {
                    if (Varjo_SteamVR_Controller.Input(index).GetTouchDown(buttonId))
                        Debug.Log(buttonId + " touch down");
                    if (Varjo_SteamVR_Controller.Input(index).GetTouchUp(buttonId))
                        Debug.Log(buttonId + " touch up");
                    if (Varjo_SteamVR_Controller.Input(index).GetTouch(buttonId))
                    {
                        var axis = Varjo_SteamVR_Controller.Input(index).GetAxis(buttonId);
                        Debug.Log("axis: " + axis);
                    }
                }

                if (triggerPressed)
                {
                    Debug.Log("trigger bool is true");
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
        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            //Debug.Log("in");
            if (PointerIn != null)
                PointerIn(this, e);
        }

        public virtual void OnPointerClick(PointerEventArgs e)
        {
            //Debug.Log("laserrrr");
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
    }

    public struct PointerEventArgs
    {
        //public SteamVR_Input_Sources fromInputSource;
        public uint flags;
        public float distance;
        public Transform target;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);
}

