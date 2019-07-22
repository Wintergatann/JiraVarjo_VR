// Copyright 2019 Varjo Technologies Oy. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Varjo
{
    public class VarjoManager : VarjoLayer
    {
        private static VarjoManager _instance = null;
        public static VarjoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("Varjo VR Manager")
                    {
                        hideFlags = HideFlags.DontSave
                    };
                    Instance = go.AddComponent<VarjoManager>();
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        /// <summary>
        /// Have we called BeginFrame without matching EndFrame.
        /// </summary>
        private bool beginFrameCalled = false;

        /// <summary>
        /// Thread that Unity is running. Used for making sure the loggings are displayed correctly even if they
        /// are called from other threads via plugin. This is set in Awake() to guarantee it's in main thread.
        /// </summary>
        private static Thread unityThread = null;
        /// <summary>
        /// A queue of log messages sent from other threads. Displayed and cleared per frame in main thread.
        /// </summary>
        private static Queue<string> logMessages = new Queue<string>();

        /// <summary>
        /// Cache the end of frame yield to reduce GC trashing.
        /// </summary>
        private YieldInstruction yieldEndOfFrame = new WaitForEndOfFrame();

        [NonSerialized]
        private bool debug = true;

        /// <summary>
        /// Last polled event type
        /// </summary>
        private ulong eventType = 0;

        /// <summary>
        /// Visibility status of application's layer in Varjo Compositor.
        /// </summary>
        public bool layerVisible = true;

        public delegate void VisibilityEvent(bool visible);
        public static event VisibilityEvent OnVisibilityEvent;

        public delegate void StandbyEvent(bool onStandby);
        public static event StandbyEvent OnStandbyEvent;

        public delegate void ForegroundEvent(bool onForeground);
        public static event ForegroundEvent OnForegroundEvent;

        private bool inStandBy = false;

        private static List<VarjoLayer> layers = new List<VarjoLayer>();

        public static void RegisterLayer(VarjoLayer layer)
        {
            layers.Add(layer);
        }

        public static void UnregisterLayer(VarjoLayer layer)
        {
            layers.Remove(layer);
        }

        /// <summary>
        /// List of events stored this frame.
        /// </summary>
        private List<VarjoPlugin.EventButton> buttonEvents = new List<VarjoPlugin.EventButton>();

        public bool VarjoSystemPresent
        {
            get;
            private set;
        }

        /// <summary>
        /// Return reference to VarjoPlugin
        /// </summary>
        public VarjoPlugin Plugin
        {
            get;
            private set;
        }

        private new void Awake()
        {
            unityThread = Thread.CurrentThread;

            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Debug.LogError("Multiple instances of VarjoManager. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Plugin = new VarjoPlugin();

            VarjoSystemPresent = VarjoPlugin.IsVarjoSystemInstalled();
            if (!VarjoSystemPresent)
            {
                Debug.LogWarning("Varjo system not found.");
                return;
            }

            base.Awake();

            // Check that the varjoCamera is valid.
            if (varjoCamera == null)
            {
                varjoCamera = Camera.main;
                if (varjoCamera == null)
                {
                    LogWarning("No camera attached to VarjoManager. Attach a camera or make sure there is main camera in the scene.");
                    enabled = false;
                    return;
                }
            }

            StartCoroutine(InitializeSession());
        }

        private void Start()
        {
            OnVisibilityEvent += VisibilityChange;
            OnStandbyEvent += StandbyChange;
            OnForegroundEvent += ForegroundChange;
        }

        IEnumerator InitializeSession()
        {
            Plugin.AttemptSessionInit();

            // If we failed to initialize the session, attempt to initialize a valid session until we get one.
            if (!VarjoPlugin.SessionValid)
            {
                Debug.LogWarning("Failed to initialize a Varjo session. Entering into poll mode...");

                EnableAllViewportCameras(false);

                while (!VarjoPlugin.SessionValid)
                {
                    bool wait = true;
                    Plugin.AttemptSessionInitThreaded(() => wait = false);
                    while (wait) yield return new WaitForSecondsRealtime(0.5f);
                }

                EnableAllViewportCameras(true);
            }

            Debug.Log("Varjo session init successful");

            StartCoroutine(EndFrameCoroutine());
        }

        private void OnApplicationQuit()
        {
            if (Plugin != null)
            {
                Plugin.Destroy();
            }
        }

        private VarjoPlugin.FramePoseData latestFramePose = new VarjoPlugin.FramePoseData
        {
            views = new VarjoPlugin.ViewInfo[4] {
                new VarjoPlugin.ViewInfo
                { projectionMatrix = new double[16], invViewMatrix = new double[16] },
                new VarjoPlugin.ViewInfo
                { projectionMatrix = new double[16], invViewMatrix = new double[16] },
                new VarjoPlugin.ViewInfo
                { projectionMatrix = new double[16], invViewMatrix = new double[16] },
                new VarjoPlugin.ViewInfo
                { projectionMatrix = new double[16], invViewMatrix = new double[16] }
            },
            eyePoses = new VarjoPlugin.Matrix[3] { new VarjoPlugin.Matrix(), new VarjoPlugin.Matrix(), new VarjoPlugin.Matrix() }
        };

        public void GetPose(VarjoPlugin.PoseType type, out VarjoPlugin.Matrix matrix)
        {
            matrix = latestFramePose.eyePoses[(int)type - 1];
        }

        public Vector3 GetHMDPosition(VarjoPlugin.PoseType type)
        {
            VarjoPlugin.Matrix matrix;
            GetPose(type, out matrix);

            var unityView = VarjoMatrixUtils.WorldMatrixToUnity(matrix.value);
            return unityView.GetColumn(3);
        }

        public Quaternion GetHMDOrientation(VarjoPlugin.PoseType type)
        {
            VarjoPlugin.Matrix matrix;
            GetPose(type, out matrix);

            var unityView = VarjoMatrixUtils.WorldMatrixToUnity(matrix.value);
            return Quaternion.LookRotation(unityView.GetColumn(2), unityView.GetColumn(1));
        }

        public VarjoPlugin.ViewInfo GetViewInfo(int viewIndex)
        {
            return latestFramePose.views[viewIndex];
        }

        private void Update()
        {
            // Empty the lock message queue
            lock (logMessages)
            {
                while (logMessages.Count > 0)
                {
                    var txt = logMessages.Dequeue();
                    if (debug) Debug.Log(txt);
                }
            }

            if (!VarjoPlugin.SessionValid)
                return;

            // Call waitsync and sync the render thread
            if (layerVisible && !inStandBy)
            {
                Profiler.BeginSample("Varjo.WaitSync");
                Plugin.IssuePluginEvent(VarjoPlugin.VARJO_RENDER_EVT_WAITSYNC);
                VarjoPlugin.SyncRenderThread();
                Profiler.EndSample();
            }

            // Fetch fresh pose data and cache it
            VarjoPlugin.GetFramePoseData(ref latestFramePose);

            // Update events
            buttonEvents.Clear();
            Profiler.BeginSample("Varjo.PollEvents");
            while (VarjoPlugin.PollEvent(ref eventType))
            {
                switch ((VarjoPlugin.EventType)eventType)
                {
                    // Keep track of application visibility and standby status
                    // and enable and disable rendering based on that
                    case VarjoPlugin.EventType.EVENT_VISIBILITY:
                        layerVisible = VarjoPlugin.GetEventVisibility().visible != 0;
                        OnVisibilityEvent(layerVisible);
                        break;

                    case VarjoPlugin.EventType.EVENT_HEADSET_STANDBY_STATUS:
                        inStandBy = VarjoPlugin.GetEventHeadsetStandbyStatus().onStandby != 0;
                        OnStandbyEvent(inStandBy);
                        break;

                    case VarjoPlugin.EventType.EVENT_FOREGROUND:
                        OnForegroundEvent(VarjoPlugin.GetEventForeground().isForeground != 0);
                        break;

                    // Update headset button states
                    case VarjoPlugin.EventType.EVENT_BUTTON:
                        buttonEvents.Add(VarjoPlugin.GetEventButton());
                        break;
                }
            }
            Profiler.EndSample();

            // Update layers
            foreach (var layer in layers)
                layer.UpdateFromManager();

            // Start rendering only if layer is visible
            if (layerVisible && !inStandBy)
            {
                EnableAllViewportCameras(true);
                Plugin.IssuePluginEvent(VarjoPlugin.VARJO_RENDER_EVT_BEGIN_FRAME);
                beginFrameCalled = true;
            }
            else
            {
                EnableAllViewportCameras(false);
            }
        }

        void VisibilityChange(bool visible)
        {
            EnableAllViewportCameras(visible && !inStandBy);
        }

        void StandbyChange(bool onStandby)
        {
            EnableAllViewportCameras(layerVisible && !inStandBy);
        }

        void ForegroundChange(bool inForeground)
        {
            // Nothing to do here, just make sure the delegate is not null
        }

        // Enable/disable all viewport cameras in all layers
        void EnableAllViewportCameras(bool enable)
        {
            foreach (var layer in layers)
                layer.EnableCameras(enable);
        }

        private static List<VarjoPlugin.MultiProjLayer> submission = new List<VarjoPlugin.MultiProjLayer>();

        private IEnumerator EndFrameCoroutine()
        {
            while (true)
            {
                yield return yieldEndOfFrame;

                if (!VarjoPlugin.SessionValid || !beginFrameCalled)
                {
                    Profiler.BeginSample("Varjo.EndOfFrame.ThrottleFor100ms");
                    // Sleep for 100ms so that we won't hog the CPU
                    Thread.Sleep(100);
                    // Still poll events if we have a session
                    if (VarjoPlugin.SessionValid)
                        Plugin.IssuePluginEvent(VarjoPlugin.VARJO_RENDER_EVT_POLL_EVENTS);
                    GL.Flush();
                    Profiler.EndSample();
                    continue;
                }

                Profiler.BeginSample("Varjo.EndOfFrame");

                if (VarjoManager.Instance.viewportCameras == null || VarjoManager.Instance.viewportCameras.Count != 4)
                {
                    VarjoManager.LogError("VarjoViewCombiner can't access a proper viewport array.");
                    continue;
                }

                GL.sRGBWrite = true;

                Profiler.BeginSample("Varjo.Submit");

                submission.Clear();
                // Sort the layers according to layer depth
                foreach (var varjoLayer in layers.OrderBy(l => l.layerOrder))
                {
                    if (varjoLayer.layerEnabled)
                        submission.Add(varjoLayer.PrepareForSubmission());
                }

                var subArray = submission.ToArray();
                VarjoPlugin.QueueSubmission(subArray.Length, subArray);

                Profiler.EndSample();

                // Blit to screen if SRPs are in use
                if (GraphicsSettings.renderPipelineAsset != null)
                {
                    Profiler.BeginSample("Varjo.BlitToScreen");
                    // Blit left context of the main layer to screen
                    if (flipY)
                        Graphics.Blit(GetRenderTextureForCamera(VarjoViewCamera.CAMERA_ID.CONTEXT_LEFT), (RenderTexture)null, new Vector2(contextDisplayFactor, contextDisplayFactor), new Vector2(0.0f, 1.0f - contextDisplayFactor));
                    else
                        Graphics.Blit(GetRenderTextureForCamera(VarjoViewCamera.CAMERA_ID.CONTEXT_LEFT), (RenderTexture)null, new Vector2(contextDisplayFactor, -1.0f * contextDisplayFactor), new Vector2(0.0f, contextDisplayFactor));

                    Profiler.EndSample();

                }

                Plugin.IssuePluginEvent(VarjoPlugin.VARJO_RENDER_EVT_SUBMIT);
                GL.InvalidateState();

                beginFrameCalled = false;
                Profiler.EndSample();

            }
        }

        public static void Log(string txt)
        {
            txt = DateTime.Now.Ticks + ": " + txt;

            // If keepLogOrder is set to true, the order of the logs will be real. The downside is that all the logs are
            // postponed until VarjoManager.Update is called - also those called from the main thread. This can cause you
            // to lose some logs if Unity happens to crash or freeze.
            bool keepLogOrder = false;

            if (keepLogOrder || Thread.CurrentThread != unityThread)
            {
                lock (logMessages)
                {
                    logMessages.Enqueue(txt);
                }
                return;
            }

            if (Instance.debug) Debug.Log(txt);
            //Console.WriteLine(txt);
        }

        /// <summary>
        /// Returns true when headset button gets pressed.
        /// </summary>
        /// <param name="buttonId">Id of headset button. 0 is application button.</param>
        /// <returns></returns>
        public bool GetButtonDown(int buttonId = 0)
        {
            for (int i = buttonEvents.Count - 1; i >= 0; --i)
            {
                if (buttonEvents[i].buttonId == buttonId)
                {
                    return buttonEvents[i].pressed != 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true when headset button gets released.
        /// </summary>
        /// <param name="buttonId">Id of headset button. 0 is application button.</param>
        /// <returns></returns>
        public bool GetButtonUp(int buttonId = 0)
        {
            for (int i = buttonEvents.Count - 1; i >= 0; --i)
            {
                if (buttonEvents[i].buttonId == buttonId)
                {
                    return buttonEvents[i].pressed == 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Is this application currently visible in Varjo Compositor.
        /// </summary>
        /// <returns></returns>
        public bool IsLayerVisible()
        {
            return layerVisible;
        }

        /// <summary>
        /// Is headset currently in stand by.
        /// </summary>
        /// <returns></returns>
        public bool IsInStandBy()
        {
            return inStandBy;
        }

        public static void LogWarning(string txt) { Debug.LogWarning(txt); }
        public static void LogError(string txt) { Debug.LogError(txt); }
    }
}
