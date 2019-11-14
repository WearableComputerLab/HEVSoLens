#if UNITY_WSA
using SimpleJSON;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;

namespace HEVS.UniSA {

    internal static class RemoteSetup {
        
        /// <summary>
        /// Starts up holographic remoting.
        /// </summary>
        public static void StartRemoting() {
            var remote = MixedRealityDisplay.currentDisplay.remote;

            if (HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Connected)
                HolographicRemoting.Connect(remote.address, remote.maxBitRate);

            Configuration.OnPreUpdate += CheckHolographicRemoting;
        }

        // Runs every frame until holographic remoting starts
        private static void CheckHolographicRemoting() {
            MixedRealityDisplay display = MixedRealityDisplay.currentDisplay;

            // Get the remote
            var remote = display.remote;

            // If remoting has now connected
            if (HolographicRemoting.ConnectionState == HolographicStreamerConnectionState.Connected) {
                UniSAConfig.current.StartCoroutine(LoadDevice("WindowsMR"));
                Configuration.OnPreUpdate -= CheckHolographicRemoting;
                remote.connected = true;
                MixedRealityDisplay.currentDisplay.Setup(MixedRealityDisplay.currentDisplayConfig, false);
            }
            else if (HolographicRemoting.ConnectionState == HolographicStreamerConnectionState.Disconnected)
                HolographicRemoting.Connect(remote.address, remote.maxBitRate);
        }
        
        // Neccessary for remoting.
        private static IEnumerator LoadDevice(string newDevice) {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = true;
        }
    }
}
#endif