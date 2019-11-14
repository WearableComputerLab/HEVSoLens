#if UNITY_WSA
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HEVS.UniSA
{

    /// <summary>
    /// Used to find the origin using gaze.
    /// </summary>
    internal class OriginPointer : OriginFinder
    {
        
        #region Variables

        // The HoloLens' transform
        private Transform _holoLens;

        // The marker at the end of the gaze.
        private Transform _marker;
        private CursorLocator _cursor;

        // The text appearing on the HoloLens.
        private TextMesh _text;

        // If the origin was set.
        private bool _found;

        // Recognises gestures performed on the HoloLens.
        private GestureRecognizer _gestureRecognizer;
        #endregion
        
        public OriginPointer(Transform holoLens)
        {
            _holoLens = holoLens;
            _cursor = new CursorLocator(_holoLens);

            // Create the text
            _text = CreateText();
            _text.text = "Place the origin and then tap the air.";
            
            // Listen to HoloLens tap
            _gestureRecognizer = new GestureRecognizer();
            _gestureRecognizer.Tapped += HoloLensTap;
            _gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);
            _gestureRecognizer.StartCapturingGestures();

            // Create the marker
            _marker = CreateMarker(); 
        }

        #region For Start Up

        // Create the text and its object.
        private TextMesh CreateText()
        {
            GameObject gameObject = new GameObject("Text");
            gameObject.transform.SetParent(_holoLens, false);
            gameObject.transform.localPosition = new Vector3(0f, 0.12f, 1f);
            gameObject.transform.localScale = Vector3.one * 0.005f;

            //gameObject.layer = int.MaxValue;
            
            TextMesh text = gameObject.AddComponent<TextMesh>();
            text.fontSize = 32;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;

            return text;
        }

        // Create a marker object.
        private Transform CreateMarker()
        {
            // Create the marker object
            GameObject marker = new GameObject("Marker");
            marker.transform.parent = _holoLens;
            //marker.layer = 0;

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, new Vector3(-0.05f, 0f, -0.05f), new Vector3(0.05f, 0f, -0.05f) };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 1 };

            // Create the renderer and filter
            MeshRenderer renderer = marker.gameObject.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            MeshFilter filter = marker.gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            return marker.transform;
        }
        #endregion

        // Called when HoloLens taps.
        private void HoloLensTap(TappedEventArgs args)
        { _found = true; }

        // Returns the transform of the marker if the origin has been set.
        public bool TryGetOrigin(out Vector3 position, out Quaternion rotation)
        {
            // Update the markers transform
            _marker.transform.position = _cursor.transform.translate;
            _marker.transform.eulerAngles = new Vector3(0f, _holoLens.eulerAngles.y, 0f);
    
            if (!_found)
            {
                position = Vector3.zero;
                rotation = new Quaternion();
                return false;
            }

            position = _marker.position;
            rotation = _marker.rotation;
            
            return true;
        }

        public void Disable() {
            _cursor.Close();
            Object.DestroyImmediate(_marker.gameObject);
            Object.DestroyImmediate(_text.gameObject);
        }
    }
}
#endif