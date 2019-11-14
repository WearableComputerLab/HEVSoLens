using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.Linq;

namespace HEVS.UniSA {

    [CustomDisplay("trackedoffaxis")]
    public class TrackedOffAxisDisplay : Tracked<OffAxisDisplay> { }

    [CustomDisplay("trackedcurved")]
    public class TrackedCurvedDisplay : Tracked<CurvedDisplay> { }

    [CustomDisplay("trackeddome")]
    public class TrackedDomeDisplay : Tracked<DomeDisplay> { }

    [CustomDisplay("tracked")]
    public class TrackedDisplay : Tracked<StandardDisplay> { }

    /// <summary>
    /// Config connecting a display to a tracker.
    /// </summary>
    public class Tracked<T> : Display where T : Display, new() {

        #region Config Variables
        
        /// <summary>
        /// The display to be transformed.
        /// </summary>
        public T display { get => _display; }
        private T _display;

        /// <summary>
        /// The tracker that will transform the display.
        /// </summary>
        public TrackerConfig tracker { get => _tracker; }
        private TrackerConfig _tracker;

        private TrackerManager _manager;

        #endregion

        #region Create the Dynamic Display Config

        /// <summary>
        /// Use display's draw gizmo.
        /// </summary>
        public void DrawGizmo(DisplayConfig displayOwner) {
            _display.DrawGizmo(displayOwner);
        }

        /// <summary>
        /// Parses the config from a json.
        /// </summary>
        /// <param name="jsonNode">The dynamic display json.</param>
        public bool Parse(JSONNode jsonNode)
        {
            // Create the original display
            _display = new T();
            if (!_display.Parse(jsonNode)) return false;

            if (PlatformConfig.current != null)
            {// TODO: is not read in inspector
                if (jsonNode["tracker"] != null) _tracker = PlatformConfig.current.trackers.Find(i => i.id == jsonNode["tracker"].Value);

                if (_tracker != null)
                {
                    _manager = new GameObject(_tracker.id + "-Tracker").AddComponent<TrackerManager>();
                    _manager.transform.SetParent(Camera.main.transform);
                    _manager.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    _manager.id = _tracker.id;
                }
            }

            return true;
        }

        /// <summary>
        /// Use display's ray cast.
        /// </summary>
        public bool Raycast(DisplayConfig displayOwner, Ray ray, out float distance, out Vector2 hitPoint2D) {
            return _display.Raycast(displayOwner, ray, out distance, out hitPoint2D);
        }

        /// <summary>
        /// Set up the display and add tracking.
        /// </summary>
        public void Setup(DisplayConfig displayOwner, bool stereo) {
            displayOwner.displayRig = _display;
            _display.Setup(displayOwner, stereo);
            
            // Store the initial transform
            TransformConfig originalOffset = displayOwner.transformOffset;

            // TODO: should need to do this.
            _tracker = PlatformConfig.current.trackers.Find(i => i.id == displayOwner.json["tracker"].Value);

            // Update the display owner every frame
            Configuration.OnPreUpdate += () =>
            {
                displayOwner.transformOffset.translate = _manager.transform.position - SceneOrigin.position + originalOffset.translate;
                displayOwner.transformOffset.rotate = _manager.transform.rotation * originalOffset.rotate;
                displayOwner.transformOffset.scale = Vector3.Scale(_manager.transform.lossyScale, originalOffset.scale);
            };
        }

        /// <summary>
        /// Use display's ray.
        /// </summary>
        public Ray ViewportPointToRay(DisplayConfig displayOwner, Vector2 displaySpacePoint) {
            return _display.ViewportPointToRay(displayOwner, displaySpacePoint);
        }

        public void OnPlatformActivated()
        {}
        #endregion
    }
}