// Copyright 2019 Varjo Technologies Oy. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varjo;

namespace VarjoExample
{
    /// <summary>
    /// When user presses application button or defined key send gaze calibration request.
    /// </summary>
    public class VarjoGazeCalibrationRequest : MonoBehaviour
    {
        [Header("Keyboard key to request calibration")]
        public KeyCode key = KeyCode.Space;

        [Header("Should application button request calibration")]
        public bool useApplicationButton = true;

        void Update()
        {
            if (VarjoManager.Instance.IsLayerVisible())
            {
                if (Input.GetKeyDown(key))
                {
                    VarjoPlugin.RequestGazeCalibration();
                }
            }

            if(VarjoManager.Instance.GetButtonDown() && useApplicationButton)
            {
                VarjoPlugin.RequestGazeCalibration();
            }
        }
    }
}
