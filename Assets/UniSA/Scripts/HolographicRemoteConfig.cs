using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace HEVS.UniSA
{
    /// <summary>
    /// Used to holographically remote to a HoloLens
    /// </summary>
    public class HolographicRemoteConfig : IJSONObject {

        /// <summary>
        /// The address of the HoloLens.
        /// </summary>
        public string address;

        /// <summary>
        /// The maximum bit rate for sending data.
        /// </summary>
        public int maxBitRate = 99999;

        /// <summary>
        /// Is the holoLens connected?
        /// </summary>
        public bool connected = false;

        /// <summary>
        /// Parse a json.
        /// </summary>
        public bool Parse(JSONNode json) {

            // Set remote variables
            if (json["address"] != null) { address = json["address"].Value; }
            if (json["max_bit_rate"] != null) { maxBitRate = json["max_bit_rate"].AsInt; }

            return true;
        }

        public void OnPlatformActivated()
        {
            throw new System.NotImplementedException();
        }
    }
}
