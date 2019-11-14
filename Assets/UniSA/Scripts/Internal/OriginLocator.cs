using UnityEngine;

namespace HEVS.UniSA
{

    /// <summary>
    /// Used to find the origin using gaze.
    /// </summary>
    internal class OriginLocator : OriginFinder
    {
        
        #region Variables

        // The HoloLens' transform
        private Transform _holoLens;
        
        // The text appearing on the HoloLens.
        private TextMesh _text;
        
        #endregion

        public OriginLocator(Transform holoLens)
        {
            _holoLens = holoLens;

            // Create the text
            _text = CreateText();
            _text.text = "Look at the marker to set the origin.";
        }

        #region For Start Up

        // Create the text and its object.
        private TextMesh CreateText()
        {
            GameObject gameObject = new GameObject("Text");
            gameObject.transform.parent = _holoLens;
            gameObject.transform.position = new Vector3(0f, 1f, 10f);
            gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            //gameObject.layer = int.MaxValue;

            TextMesh text = gameObject.AddComponent<TextMesh>();
            text.alignment = TextAlignment.Center;

            return text;
        }

        // Create a marker object.
        private Transform CreateMarker()
        {
            // Create the marker object
            GameObject marker = new GameObject("Marker");
            marker.transform.parent = _holoLens;
            marker.layer = int.MaxValue;

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, new Vector3(-0.1f, 0f, -0.1f), new Vector3(0.1f, 0f, -0.1f) };
            mesh.triangles = new int[] { 0, 1, 2 };

            // Create the renderer and filter
            MeshRenderer renderer = _holoLens.gameObject.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            MeshFilter filter = _holoLens.gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            return marker.transform;
        }
        #endregion

        // Returns the transform of the marker if the origin has been set.
        public bool TryGetOrigin(out Vector3 position, out Quaternion rotation)
        {
            // Do the updating
            
            position = Vector3.zero;
            rotation = new Quaternion();
            return false;
        }

        public void Disable()
        {
            Object.Destroy(_text.gameObject);
        }
    }
}