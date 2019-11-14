using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;
#if UNITY_WSA
using UnityEngine.XR;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;
#endif

namespace HEVS.UniSA {

    internal static class OriginController {

        #region Variables

        // Used to find the origin
        private static OriginFinder _originFinder;

#if UNITY_WSA
        // The world anchor for saving and storing world anchors
        private static WorldAnchorStore _worldAnchorStore;

        // The world anchor to use
        internal static WorldAnchor worldAnchor;
#endif
        private static Action _finishAction;
        
        #endregion

        /// <summary>
        /// Constructs an origin controller object.
        /// </summary>
        /// <param name="holoLens">The holoLens device.</param>
        public static void FindAsync(Action finishAction) {
            OriginController._finishAction = finishAction;
            
#if UNITY_WSA
            if (MixedRealityDisplay.currentDisplay.shareOrigin) _shareData = new MemoryStream();

            // Load world anchors store
            WorldAnchorStore.GetAsync(WorldAnchorStoreLoaded);
#else
            StartSearching();
#endif
        }

        /// <summary>
        /// Updates the Origin Controller.
        /// </summary>
        private static void UpdateOriginFinder() {
            if (_originFinder != null) {
                // If the origin finder worked
                if (_originFinder.TryGetOrigin(out Vector3 position, out Quaternion rotation)) {
#if UNITY_WSA
                    SetWorldAnchor(CreateWorldAnchor(position, rotation));
#else
                    SetOrigin(position, rotation);
#endif
                    Configuration.OnPreUpdate -= UpdateOriginFinder;
                }
            }
        }

        // Starts searching for the origin
        private static void StartSearching() {

            // Set the origin finder
            switch (MixedRealityDisplay.currentDisplay.origin) {

#if UNITY_WSA
                case MixedRealityDisplay.OriginType.CHOOSE_ORIGIN:
                _originFinder = new OriginPointer(MixedRealityDisplay.currentDisplayConfig.gameObject.transform.GetChild(0));
                Configuration.OnPreUpdate += UpdateOriginFinder;
                return;
#endif

                case MixedRealityDisplay.OriginType.FIND_MARKER:
                _originFinder = new OriginLocator(MixedRealityDisplay.currentDisplayConfig.gameObject.transform.GetChild(0));
                Configuration.OnPreUpdate += UpdateOriginFinder;
                return;

                default:
#if UNITY_WSA
                SetWorldAnchor(CreateWorldAnchor(MixedRealityDisplay.currentDisplayConfig.gameObject.transform.position,
                    Quaternion.Euler(0f, MixedRealityDisplay.currentDisplayConfig.gameObject.transform.localEulerAngles.y, 0f)));
#else
                SetOrigin(MixedRealityDisplay.currentDisplayConfig.gameObject.transform.position,
                    Quaternion.Euler(0f, MixedRealityDisplay.currentDisplayConfig.gameObject.transform.localEulerAngles.y, 0f));
#endif
                    return;
            }
        }

#if UNITY_WSA

        #region Set World Anchor

        // Sets the origin and direction for the holoLens
        private static void SetWorldAnchor(WorldAnchor worldAnchor) {
            if (worldAnchor == null) { return; }
            OriginController.worldAnchor = worldAnchor;
            //worldAnchor.transform.SetParent(MixedRealityDisplay.currentDisplayConfig.gameObject.transform, true);

            if (_worldAnchorStore != null)
                _worldAnchorStore.Save(MixedRealityDisplay.currentDisplayConfig.id, OriginController.worldAnchor);
            
            SetOrigin(worldAnchor.transform.position, worldAnchor.transform.rotation);
        }

        // Sets the origin and creates a world anchor
        private static WorldAnchor CreateWorldAnchor(Vector3 position, Quaternion rotation) {

            // Create world anchor
            GameObject anchorObject = new GameObject("Origin");
            anchorObject.transform.position = position;
            anchorObject.transform.rotation = rotation;

            WorldAnchor worldAnchor = anchorObject.AddComponent<WorldAnchor>();

#if !UNITY_EDITOR
            if (MixedRealityDisplay.currentDisplay.shareOrigin)
            {
                // Share the world anchor
                WorldAnchorTransferBatch transferBatch = new WorldAnchorTransferBatch();
                transferBatch.AddWorldAnchor(MixedRealityDisplay.currentDisplayConfig.id, worldAnchor);

                WorldAnchorTransferBatch.ExportAsync(transferBatch, (byte[] data) => {
                    RPCManager.CallOnMaster(UniSAConfig.current, NodeConfig.current.id, "ShareOriginData", Encoding.ASCII.GetString(data));
                },
                (SerializationCompletionReason completionReason) => {
                    RPCManager.CallOnMaster(UniSAConfig.current, "ShareOriginComplete", NodeConfig.current.id,
                        completionReason == SerializationCompletionReason.Succeeded);
                });
            }
#endif

            return worldAnchor;
        }
        #endregion

        #region Shared World Anchor

#if UNITY_WSA
        // The first holoLens to share data
        private static string shareID;

        // The stream to hold data in
        private static MemoryStream _shareData;

        /// <summary>
        /// Adds to the world anchor data.
        /// </summary>
        /// <param name="data">A world anchor as data.</param>
        public static void ShareOriginData(string clusterID, string data) {
            if (shareID == null) shareID = clusterID;

            if (_shareData == null || shareID != clusterID) return;

            _shareData.Write(Encoding.ASCII.GetBytes(data), 0, data.Length);
        }

        /// <summary>
        /// Sets the world anchor at the origin from data if succeeded.
        /// </summary>
        public static void ShareOriginComplete(string clusterID, bool succeeded) {
            if (succeeded && clusterID != shareID && _shareData != null) {

                if (MixedRealityDisplay.currentDisplay.shareOrigin) {
                    WorldAnchorTransferBatch.ImportAsync(_shareData.ToArray(), (SerializationCompletionReason completionReason, WorldAnchorTransferBatch batch) => {

                        if (completionReason != SerializationCompletionReason.Succeeded) return;

                        // Disable the origin finder if it exists
                        if (_originFinder != null) {
                            _originFinder.Disable();
                            _originFinder = null;
                        }

                        // Create the world anchor
                        SetWorldAnchor(batch.LockObject(MixedRealityDisplay.currentDisplayConfig.id, worldAnchor == null ?
                            new GameObject("WorldAnchor") : worldAnchor.gameObject));
                    });
                }
            }

            _shareData = null;
        }

#endif

#endregion

        #region Store World Anchor

        // Once the store is loaded, try to load the world anchor
        private static void WorldAnchorStoreLoaded(WorldAnchorStore store) {
            _worldAnchorStore = store;

            if (MixedRealityDisplay.currentDisplay.usePreviousOrigin) {
                WorldAnchor worldAnchor = _worldAnchorStore.Load(MixedRealityDisplay.currentDisplayConfig.id, new GameObject("WorldAnchor"));

                if (worldAnchor) {
                    SetWorldAnchor(worldAnchor);
                    return;
                }
            }

            // Origin could already be found if sharing was used
            if (worldAnchor)
                _worldAnchorStore.Save(MixedRealityDisplay.currentDisplayConfig.id, worldAnchor);
            else
                StartSearching();
        }
        #endregion

#endif

        // Sets the origin and direction for the holoLens
        private static void SetOrigin(Vector3 position, Quaternion rotation) {
            
            // Disable the origin finder
            if (_originFinder != null) {
                _originFinder.Disable();
                _originFinder = null;
            }

            UpdateOrigin(position, rotation);

            if (_finishAction != null)
            {
                _finishAction.Invoke();
                _finishAction = null;
#if UNITY_WSA
                Configuration.OnPreUpdate += UpdateInverseAnchor;
#endif
            }
        }

        private static void UpdateOrigin(Vector3 position, Quaternion rotation)
        {
            Transform transform = MixedRealityDisplay.currentDisplayConfig.gameObject.transform;

            // Update container transform
            TransformConfig transformOffset = MixedRealityDisplay.currentDisplayConfig.transformOffset;

            transform.localPosition = transformOffset.translate - Vector3.Scale(Quaternion.Inverse(rotation) * position - HEVS.Camera.main.transform.localPosition, transformOffset.scale);
            transform.localRotation = transformOffset.rotate * Quaternion.Inverse(rotation);
            transform.localScale = transformOffset.scale;
        }

#if UNITY_WSA
        // World anchors move them self relative to the camera, this does the opposite
        private static void UpdateInverseAnchor()
        {
            SetOrigin(worldAnchor.transform.position, worldAnchor.transform.rotation);
        }
#endif
    }
}