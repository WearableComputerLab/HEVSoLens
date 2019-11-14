using UnityEngine;
#if UNITY_WSA
using UnityEngine.XR.WSA;
#endif

namespace HEVS.UniSA
{
    /// <summary>
    /// Used to track the position of the cursor in real world space.
    /// </summary>
    internal class CursorLocator
    {

        /// <summary>
        /// The current transform of the cursor.
        /// </summary>
        public TransformConfig transform;

        // The HoloLens' transform.
        private Transform _holoLens;

#if UNITY_WSA
        // The spatial mapping collider for the HoloLens.
        private SpatialMappingCollider _collider;
#endif

        // The distance from the HoloLens to the cursor.
        private float _distance;

        public CursorLocator(Transform holoLens)
        {
            _holoLens = holoLens;
#if UNITY_WSA
            _collider = Camera.main.gameObject.AddComponent<SpatialMappingCollider>();
#endif
            HEVS.Configuration.OnPreUpdate += Update;
        }

        private void Update()
        {
#if UNITY_WSA
            // Raycast to hit the spatial mesh
            RaycastHit[] hits = Physics.RaycastAll(_holoLens.localPosition, _holoLens.localRotation * Vector3.forward);
            if (hits != null && hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    // If this is the surface 
                    if (hit.transform.parent != null && hit.transform.parent == _collider.surfaceParent.transform)
                    {
                        // Store the distance, as it is more useful than position
                        _distance = Vector3.Distance(_holoLens.localPosition, hit.point);
                        transform.rotate = Quaternion.LookRotation(hit.normal, _holoLens.up);
                        break;
                    }
                }
            }

            // Update the markers transform
            transform.translate = _holoLens.position + _holoLens.forward * _distance;
#endif
        }

        /// <summary>
        /// Called to destroy the spatial mapping collider
        /// </summary>
        public void Close()
        {
#if UNITY_WSA
            Object.Destroy(_collider);
#endif
        }
    }
}