using UnityEngine;
using OSCsharp.Data;
using OSCsharp.Net;
using System.Collections.Generic;

namespace HEVS.UniSA {

    /// <summary>
    /// Sends HoloLens tracker info using OSC.
    /// </summary>
    internal class Transmitter {
        // Store all transmitters
        private static Dictionary<int, Transmitter> _transmitters;

        /// <summary>
        /// Get a transmitter for a port.
        /// </summary>
        public static Transmitter GetTransmitter(int port) {
            if (_transmitters == null) { _transmitters = new Dictionary<int, Transmitter>(); }
            else if (_transmitters.ContainsKey(port)) return _transmitters[port];

            Transmitter transmitter = new Transmitter(port);
            _transmitters[port] = transmitter;
            return transmitter;
        }

        /// <summary>
        /// Close all the transmitters
        /// </summary>
        public static void CloseAll() {
            if (_transmitters == null) return;

            if (_transmitterX != null) _transmitterX.Close();

            foreach (Transmitter transmitter in _transmitters.Values) {
                if (transmitter._transmitter != null) transmitter._transmitter.Close();
            }
        }

        // The transmitter for sending OSC
        private UDPTransmitter _transmitter;
        // The transmitter for sending OSC
        private static UDPTransmitter _transmitterX;

        // Construct a transmitter to send hololens input.
        private Transmitter(int port) {

            // Create and start the transorm
#if UNITY_EDITOR
            _transmitter = new UDPTransmitter("127.0.0.1", port);
#else
            _transmitter = new UDPTransmitter(Cluster.masterNode.address, port);
#endif

            _transmitter.Connect();
            if (_transmitterX == null)
            {
#if UNITY_EDITOR
                _transmitterX = new UDPTransmitter("127.0.0.1", 6667);// TODO: only have one transmitter
#else
                _transmitterX = new UDPTransmitter(Cluster.masterNode.address, 6667);// TODO: only have one transmitter
#endif
                _transmitterX.Connect();
            }
        }

        /// <summary>
        /// Sends the holoLen's transform to the master.
        /// </summary>
        public static void SendTransform(TrackerConfig tracker, TransformConfig transform) {
            Transmitter transmitter = GetTransmitter(((OSCTrackerData)tracker.data).port);

            // Send the transform data
            Vector3 rotate = transform.rotate.eulerAngles;
            transmitter.SendOSC("/" + tracker.id + "/transform",
                transform.translate.x, transform.translate.y, transform.translate.z, rotate.x, rotate.y, rotate.z, 1f, 1f, 1f);
        }

        /// <summary>
        /// Sends the holoLen's input to the master.
        /// </summary>
        public static void SendButton(TrackerConfig tracker, string button, object value) {
            Transmitter transmitter = GetTransmitter(((OSCTrackerData)tracker.data).port);
            
            transmitter.SendOSCX("/" + tracker.id + "/button/" + button, value);
        }
        
        // Send data through OSC
        private void SendOSC(string address, params object[] args) {
            // Message with address "/id/button_gesture"
            OscMessage msg = new OscMessage(address);

            // Set the gesture
            foreach (object arg in args)
                msg.Append(arg);

            // Send the transform data to master
            _transmitter.Send(msg);
        }

        // Send data through OSC
        private void SendOSCX(string address, params object[] args) {
            // Message with address "/id/button_gesture"
            OscMessage msg = new OscMessage(address);

            // Set the gesture
            foreach (object arg in args)
                msg.Append(arg);

            // Send the transform data to master
            _transmitterX.Send(msg);
        }
    }
}