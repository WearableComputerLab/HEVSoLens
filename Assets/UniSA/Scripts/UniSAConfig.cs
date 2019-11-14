using System.Collections.Generic;
using UnityEngine;

namespace HEVS.UniSA {
    /// <summary>
    /// Adds the following functionality:
    ///     - Mixed reality displays
    ///     - Generic tracked displays
    ///     - Fixed origin offaxis displays
    /// </summary>
    public class UniSAConfig : MonoBehaviour {

        public static UniSAConfig current { get => _current; }
        private static UniSAConfig _current;
        
        private void Awake() {
            _current = this;

            // Eventually won't need this
#if UNITY_EDITOR
            HoloLensMaster.current = new HoloLensMaster();
#else
            if (Cluster.isMaster)
                HoloLensMaster.current = new HoloLensMaster();
#endif

            /*DisplayConfig displayOwner = PlatformConfig.current.displays.Find(i => i.id == "disp_touch");
            TransformConfig originalOffset = displayOwner.transformOffset;

            Configuration.OnPreUpdate += () =>
            {
                displayOwner.transformOffset.translate = touch.position - SceneOrigin.position + originalOffset.translate;
                displayOwner.transformOffset.rotate = touch.rotation * originalOffset.rotate;
                displayOwner.transformOffset.scale = Vector3.Scale(touch.lossyScale, originalOffset.scale);
            };*/
        }

        private void OnApplicationQuit() {
            Transmitter.CloseAll();
            HoloLensMaster.current.Disable();
        }


        /*[HEVS.RPC]
        private void ShareOriginData(string clusterID, string data) {
#if UNITY_WSA
            OriginController.ShareOriginData(clusterID, data);
#endif
        }

        [HEVS.RPC]
        private void ShareOriginComplete(string clusterID, bool succeeded) {
#if UNITY_WSA
            OriginController.ShareOriginComplete(clusterID, succeeded);
#endif
        }*/
    }
}