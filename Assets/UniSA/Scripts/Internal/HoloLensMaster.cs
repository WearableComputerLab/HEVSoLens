using System.Collections.Generic;
using UnityEngine;
using OSCsharp.Data;
using OSCsharp.Net;
using OSCsharp.Utils;
using System.Linq;

namespace HEVS.UniSA
{

    /// <summary>
    /// Exists only on master node to take holoLens input form HoloLensDevices. Will be removed
    /// </summary>
    internal class HoloLensMaster
    {
        #region Variables

        /// <summary>
        /// The HoloLens' singleton.
        /// </summary>
        public static HoloLensMaster current;

        // The receiver for receiving OSC
        private UDPReceiver _receiver;

        private Dictionary<string, object> _input = new Dictionary<string, object>();
        #endregion

        // Create the receiver object
        public HoloLensMaster()
        {
            // Create the reciever
            _receiver = new UDPReceiver(6667, false);
            _receiver.MessageReceived += MessageReceived;
            _receiver.ErrorOccured += OscErrorOccured;

            // Start the reciever
            _receiver.Start();

            // Adds handler for HoloLens input
            Input.AccumulateCustomInput += HoloLensInput;
        }

        /// <summary>
        /// Turns off the receiver
        /// </summary>
        public void Disable()
        {
            // Stop reciever if it exists
            if (_receiver != null && _receiver.IsRunning)
                _receiver.Stop();
        }

        // Handles OSC messages
        private void MessageReceived(object sender, OscMessageReceivedEventArgs oscMessageReceivedEventArgs)
        {
            OscMessage message = oscMessageReceivedEventArgs.Message;

            // Read the address part by part
            string[] address = message.Address.TrimStart('/').Split('/');

            // Get the display via the address
            TrackerConfig tracker = PlatformConfig.current.trackers.Find(i => i.id == address[0]);

            // For the type of the message
            if (tracker != null)
            {
                foreach (var button in tracker.json["buttons"].Children)
                {
                    if (button["id"].Value != address[2]) continue;

                    if (tracker.json["mapping"].Count == 0)
                        _input[button["mapping"].Value] = message.Data[0];
                    else
                        foreach (var map in button["mapping"].Children)
                            _input[map.Value] = message.Data[0];
                }
            }
        }

        // Handles OSC errors
        private void OscErrorOccured(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            Debug.LogError("HoloLens OSC Error: " + exceptionEventArgs.ToString());
        }

        //
        private void HoloLensInput()
        {
            List<string> stillTrue = new List<string>(_input.Count);

            foreach (var input in _input)
            {
                if (input.Value == null)
                {
                    Input.ForceButtonThisFrame(input.Key);
                }
                else if (input.Value is bool)
                {
                    if ((bool)input.Value)
                    {
                        stillTrue.Add(input.Key);
                        Input.ForceButtonThisFrame(input.Key);
                    }
                }
                else
                {
                    Input.AccumulateAxisThisFrame(input.Key, (float)input.Value);
                }
            }

            _input.Clear();
            foreach (string t in stillTrue) _input.Add(t, true);
        }
    }
}