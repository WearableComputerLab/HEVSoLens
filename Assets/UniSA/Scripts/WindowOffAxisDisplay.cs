using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Linq;

namespace HEVS.UniSA
{

    [CustomDisplay("trackedwindow")]
    public class TrackedWindowOffAxisDisplay : Tracked<WindowOffAxisDisplay> { }

    [CustomDisplay("window")]
    public class WindowOffAxisDisplay : Display
    {

        /// <summary>
        /// The basic display.
        /// </summary>
        public OffAxisDisplay display { get => _display; }
        private OffAxisDisplay _display = new OffAxisDisplay();

        /// <summary>
        /// The origin position of the camera
        /// </summary>
        public TransformConfig cameraOrigin { get => _cameraOrigin; }
        private TransformConfig _cameraOrigin = TransformConfig.identity;

        /// <summary>
        /// If objects infront of the camera should be visible;
        /// </summary>
        public bool cullInfront { get => _cullInfront; }
        private bool _cullInfront = false;

        public void DrawGizmo(DisplayConfig displayOwner)
        {
            _display.DrawGizmo(displayOwner);
        }

        /// <summary>
        /// Parse and use OffAxisDisplay's Parse.
        /// </summary>
        public bool Parse(JSONNode jsonNode)
        {
            _display.Parse(jsonNode);

            if (jsonNode["cull_infront"] != null) _cullInfront = jsonNode["cull_infront"].AsBool;
            if (jsonNode["camera_origin"] != null) { _cameraOrigin.Parse(jsonNode["camera_origin"]); }

            return true;
        }

        /// <summary>
        /// Use OffAxisDisplay's Raycast.
        /// </summary>
        public bool Raycast(DisplayConfig displayOwner, Ray ray, out float distance, out Vector2 hitPoint2D)
        {
            return _display.Raycast(displayOwner, ray, out distance, out hitPoint2D);
        }

        /// <summary>
        /// Setup and use OffAxisDisplay's Setup.
        /// </summary>
        public void Setup(DisplayConfig displayOwner, bool stereo)
        {
            displayOwner.displayRig = _display;
            _display.Setup(displayOwner, stereo);
            
            foreach (UnityEngine.Camera camera in _display.cameras)
            {
                // TODO: this is bad... should not have to do this
                camera.transform.localPosition = Vector3.zero;

                Configuration.OnPreUpdate += () =>
                {

                    // Move the camera
                    if (!cameraOrigin.isIdentity)
                        camera.transform.position = displayOwner.transformOffset.TransformPoint(_cameraOrigin.translate);

                    // Cull infront
                    if (cullInfront)
                        camera.nearClipPlane = Vector3.Distance(displayOwner.transformOffset.translate + _display.center + SceneOrigin.position, camera.transform.position);
                };
            }
        }

        /// <summary>
        /// Use OffAxisDisplay's ViewportPointToRay.
        /// </summary>
        public Ray ViewportPointToRay(DisplayConfig displayOwner, Vector2 displaySpacePoint)
        {
            return _display.ViewportPointToRay(displayOwner, displaySpacePoint);
        }

        public void OnPlatformActivated()
        {}
    }
}