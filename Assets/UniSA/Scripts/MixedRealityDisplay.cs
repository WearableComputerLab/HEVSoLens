using System;
using System.Collections;
using UnityEngine;
using System.IO;
using SimpleJSON;
#if UNITY_WSA
using UnityEngine.XR;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;
#endif

namespace HEVS.UniSA {

    /// <summary>
    /// Config for a HoloLens display type.
    /// </summary>
    [CustomDisplay("mixedreality")]
    public class MixedRealityDisplay : Display {

        // The current display config (if it exists)
        internal static DisplayConfig currentDisplayConfig;

        // The active display (if it exists)
        internal static MixedRealityDisplay currentDisplay
            => currentDisplayConfig != null ? (MixedRealityDisplay)currentDisplayConfig.displayRig : null;

        #region Config Variables 

        /// <summary>
        /// How the Mixed Reality to HEVS origin is set.
        /// </summary>
        public OriginType origin { get => _origin; }
        private OriginType _origin = OriginType.START_LOCATION;

        /// <summary>
        /// If the world anchor should be shared between other devices (Exlusive to HoloLens).
        /// </summary>
        public bool shareOrigin { get => _shareOrigin; }
        private bool _shareOrigin = true;

        /// <summary>
        /// Should the previous origin be loaded if it exists (Exlusive to HoloLens).
        /// </summary>
        public bool usePreviousOrigin { get => _usePreviousOrigin; }
        private bool _usePreviousOrigin = false;

        /// <summary>
        /// If the background should be hidden.
        /// </summary>
        public bool disableBackground { get => _disableBackground; }
        private bool _disableBackground = true;

        /// <summary>
        /// If real world objects should cull.
        /// </summary>
        public bool cullSpatialMesh { get => _cullSpatialMesh; }
        private bool _cullSpatialMesh = false;

        /// <summary>
        /// The clipping plane for the camera.
        /// </summary>
        public float clippingPlane { get => _clippingPlane; }
        private float _clippingPlane = 0.85f;

        /// <summary>
        /// The information for holographic remoting. Can be null.
        /// </summary>
        public HolographicRemoteConfig remote { get => _remote; }
        private HolographicRemoteConfig _remote;

        /// <summary>
        /// The tracker to be used as the holoLens. Can be null.
        /// </summary>
        public TrackerConfig tracker { get => _tracker; }
        private TrackerConfig _tracker;

        /// <summary>
        /// The tracker to be used as the left hand. Can be null.
        /// </summary>
        public TrackerConfig leftHandTracker { get => _leftHandTracker; }
        private TrackerConfig _leftHandTracker;

        /// <summary>
        /// The tracker to be used as the right. Can be null.
        /// </summary>
        public TrackerConfig rightHandTracker { get => _rightHandTracker; }
        private TrackerConfig _rightHandTracker;

        /// <summary>
        /// The tracker to be used as the curser. Can be null.
        /// </summary>
        public TrackerConfig cursorTracker { get => _cursorTracker; }
        private TrackerConfig _cursorTracker;

        #endregion

        #region Interface Functions

        /// <summary>
        /// Parses json data into the mixed reality display.
        /// </summary>
        public bool Parse(JSONNode json) {
            
            // Basic variables
            if (json["clipping_plane"] != null) _clippingPlane = json["clipping_plane"].AsFloat;
            if (json["disable_background"] != null) _disableBackground = json["disable_background"].AsBool;
            if (json["share_origin"] != null) _shareOrigin = json["share_origin"].AsBool;
            if (json["use_previous_origin"] != null) _usePreviousOrigin = json["use_previous_origin"].AsBool;
            if (json["cull_spatial_mesh"] != null) _cullSpatialMesh = json["cull_spatial_mesh"].AsBool;

            // Choose how the origin is set
            if (json["origin"] != null) {
                switch (json["origin"].Value) {
                    case "StartLocation":
                    _origin = OriginType.START_LOCATION;
                    break;

                    case "ChooseOrigin":
                    _origin = OriginType.CHOOSE_ORIGIN;
                    break;

                    case "FindOrigin":
                    _origin = OriginType.FIND_MARKER;
                    break;
                }
            }

            // Is holographic remoting being used?
            if (json["remote"] != null) {

                // Create the remote if it did not already exist
                if (remote == null) { _remote = new HolographicRemoteConfig(); }

                _remote.Parse(json["remote"]);
            }

            // Get all the relevant trackers
            if (PlatformConfig.current != null)
            {// TODO: is not read in inspector
                if (json["tracker"] != null) _tracker = PlatformConfig.current.trackers.Find(i => i.id == json["tracker"].Value);
                if (json["left_hand_tracker"] != null) _leftHandTracker = PlatformConfig.current.trackers.Find(i => i.id == json["left_hand_tracker"].Value);
                if (json["right_hand_tracker"] != null) _rightHandTracker = PlatformConfig.current.trackers.Find(i => i.id == json["right_hand_tracker"].Value);
                if (json["cursor_tracker"] != null) _cursorTracker = PlatformConfig.current.trackers.Find(i => i.id == json["cursor_tracker"].Value);
            }
            // TODO: check if tracker is OSC

            return true;
        }

        /// <summary>
        /// Set up the main camera to be a Mixed Reality Headset.
        /// </summary>
        public void Setup(DisplayConfig displayOwner, bool stereo)
        {
            // Get all the relevant trackers
            if (PlatformConfig.current != null)
            {// TODO: is not read in inspector
                if (displayOwner.json["tracker"] != null) _tracker = PlatformConfig.current.trackers.Find(i => i.id == displayOwner.json["tracker"].Value);
                if (displayOwner.json["left_hand_tracker"] != null) _leftHandTracker = PlatformConfig.current.trackers.Find(i => i.id == displayOwner.json["left_hand_tracker"].Value);
                if (displayOwner.json["right_hand_tracker"] != null) _rightHandTracker = PlatformConfig.current.trackers.Find(i => i.id == displayOwner.json["right_hand_tracker"].Value);
                if (displayOwner.json["cursor_tracker"] != null) _cursorTracker = PlatformConfig.current.trackers.Find(i => i.id == displayOwner.json["cursor_tracker"].Value);
            }

            currentDisplayConfig = displayOwner;

#if UNITY_WSA
            // Wait for remoting or start
            if (remote != null && remote.connected == false) {
                RemoteSetup.StartRemoting();
                return;
            };
#endif
            // Create camera
            var camera = new GameObject("MixedRealityCamera").AddComponent<UnityEngine.Camera>();
            camera.transform.SetParent(displayOwner.gameObject.transform, false);
            displayOwner.gameObject.transform.localPosition = Vector3.zero;
            displayOwner.gameObject.transform.localRotation = Quaternion.identity;

            // Other Camera setup
            if (disableBackground) {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
            }

            camera.nearClipPlane = clippingPlane;

            // Adjust the quality if running locally
            if (remote == null) QualitySettings.SetQualityLevel(0);

#if UNITY_WSA
            if (cullSpatialMesh) {
                SpatialMappingRenderer renderer = new GameObject("SpatialMeshRenderer").AddComponent<SpatialMappingRenderer>();
                renderer.occlusionMaterial = new Material(Shader.Find("VR/SpatialMapping/Occlusion"));
            }
#endif
            // Create the origin controller
            OriginController.FindAsync(InputController.Start);
        }

        /// <summary>
        /// Mixed Reality Displays cannot raycast, always returns false.
        /// </summary>
        public bool Raycast(DisplayConfig displayOwner, Ray ray, out float distance, out Vector2 hitPoint2D) {
            hitPoint2D = new Vector2();
            distance = 0f;

            // Mixed reality cannot raycast
            return false;
        }

        /// <summary>
        /// Mixed Reality Displays cannot ViewportPointToRay, always returns new Ray().
        /// </summary>
        public Ray ViewportPointToRay(DisplayConfig displayOwner, Vector2 displaySpacePoint) {
            // TODO: Is this relevent for holoLens
            return new Ray(SceneOrigin.position, Vector3.forward);
        }

        /// <summary>
        /// Draw gizmo at displays origin.
        /// </summary>
        public void DrawGizmo(DisplayConfig displayOwner) {
            // TODO: Decide how holoLens should be drawn if at all.
        }

        public void OnPlatformActivated()
        { }

        #endregion

        /// <summary>
        /// The ways that the origin can be set.
        /// </summary>
        public enum OriginType {
            START_LOCATION,
            CHOOSE_ORIGIN,
            FIND_MARKER,
        }
    }
}