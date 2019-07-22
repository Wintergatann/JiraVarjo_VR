using UnityEngine;
using System.Collections.Generic;
using Varjo.Valve.VR;
using Valve.VR.Extras;
using UnityEngine.Events;

namespace Varjo
{
    public class VarjoTesting : MonoBehaviour
    {
        //This is responsible for all of the inputs from the controllers. Even though this was originally built with SteamVR scripts,
        //no SteamVR scripts are used for getting inputs. Instead, I just invoke an event that connects to SteamVR_LaserPointer, which
        //is an adaptation of the SteamVR script to just use the raycasting functionality and none of the input functionality.

        List<int> controllerIndices = new List<int>();

        public GameObject laserObjR;
        public SteamVR_LaserPointer laserScrR;
        public GameObject laserObjL;
        public SteamVR_LaserPointer laserScrL;

        public UnityEvent onTriggerDownR;
        public UnityEvent onTriggerClickR;
        public UnityEvent onTriggerDownL;
        public UnityEvent onTriggerClickL;
        public UnityEvent onTouchpadDownR;
        public UnityEvent onTouchpadDownL;

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

        private void Start()
        {
            laserScrR = laserObjR.GetComponent<SteamVR_LaserPointer>();
            laserScrL = laserObjL.GetComponent<SteamVR_LaserPointer>();
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
            /*Debug.Log("hasTracking: " + device.hasTracking);
            Debug.Log("outOfRange: " + device.outOfRange);
            Debug.Log("calibrating: " + device.calibrating);
            Debug.Log("uninitialized: " + device.uninitialized);
            Debug.Log("pos: " + device.transform.pos);
            Debug.Log("rot: " + device.transform.rot.eulerAngles);
            Debug.Log("velocity: " + device.velocity);
            Debug.Log("angularVelocity: " + device.angularVelocity);*/

            var l = Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Leftmost);
            var r = Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Rightmost);
            Debug.Log((l == r) ? "first" : (l == index) ? "left: " + l : "right " + r);
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
            foreach (var index in controllerIndices)
            {
                foreach (var buttonId in buttonIds)
                {
                    if (Varjo_SteamVR_Controller.Input(index).GetPressDown(buttonId))
                    {
                        if (buttonId == EVRButtonId.k_EButton_SteamVR_Trigger)
                        {
                            //index 3 is left and index 4 is right.
                            if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Leftmost))
                            {
                                Debug.Log("indexxxx3");
                                laserScrL._triggerPressed = true;
                                onTriggerDownL.Invoke();
                            }
                            else if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Rightmost))
                            {
                                Debug.Log("indexxxx4");
                                laserScrR._triggerPressed = true;
                                onTriggerDownR.Invoke();
                            }
                        }
                        if (buttonId == EVRButtonId.k_EButton_SteamVR_Touchpad)
                        {
                            if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Leftmost))
                            {
                                laserScrL._triggerPressed = true;
                                onTouchpadDownL.Invoke();
                            }
                            else if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Rightmost))
                            {
                                laserScrR._triggerPressed = true;
                                onTouchpadDownR.Invoke();
                            }
                        }
                    }

                    if (Varjo_SteamVR_Controller.Input(index).GetPressUp(buttonId))
                    {
                        
                        //Debug.Log(buttonId + " press up");
                        if (buttonId == EVRButtonId.k_EButton_SteamVR_Trigger)
                        {
                            if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Leftmost))
                            {
                                laserScrL._triggerPressed = false;
                                onTriggerClickL.Invoke();
                            }
                            else if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Rightmost))
                            {
                                laserScrR._triggerPressed = false;
                                onTriggerClickR.Invoke();
                            }
                            //laserScr.pressEvent = true;
                            Varjo_SteamVR_Controller.Input(index).TriggerHapticPulse();
                            PrintControllerStatus(index);
                        }
                        if (buttonId == EVRButtonId.k_EButton_SteamVR_Touchpad)
                        {
                            if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Leftmost))
                            {
                                laserScrL._triggerPressed = false;
                            }
                            else if (index == Varjo_SteamVR_Controller.GetDeviceIndex(Varjo_SteamVR_Controller.DeviceRelation.Rightmost))
                            {
                                laserScrR._triggerPressed = false;
                            }
                        }
                    }
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
                        //Debug.Log("axis: " + axis);
                    }
                }
            }
        }
    }
}

