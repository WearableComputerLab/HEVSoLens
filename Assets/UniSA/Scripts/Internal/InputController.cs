
using UnityEngine;

namespace HEVS.UniSA
{

    /// <summary>
    /// Handles the input for a HoloLens.
    /// </summary>
    internal static class InputController
    {
        // TODO: has to be here or is garbage collected
        private static InputHandler input = null, leftHandInput = null, rightHandInput = null, cursorInput = null;
        private static HandLocator leftHand = null, rightHand = null;
        private static CursorLocator cursor = null;

        public static void Start() {
            MixedRealityDisplay display = MixedRealityDisplay.currentDisplay;
            
            if (display.tracker != null)
                input = new InputHandler(display.tracker);

            if (display.leftHandTracker != null) {
                leftHand = new HandLocator(TrackerHandedness.Left);
                leftHandInput = new InputHandler(display.leftHandTracker, TrackerHandedness.Left); ;
            }

            if (display.rightHandTracker != null) {
                rightHand = new HandLocator(TrackerHandedness.Right);
                rightHandInput = new InputHandler(display.rightHandTracker, TrackerHandedness.Right); ;
            }

            if (display.cursorTracker != null) {
                cursor = new CursorLocator(Camera.main.transform);
                cursorInput = new InputHandler(display.cursorTracker); ;
            }
            
            // Transmit the transform data
            Configuration.OnPreUpdate += () => {
                if (display.tracker != null)
                {
                    Transform mr = MixedRealityDisplay.currentDisplayConfig.gameObject.transform.GetChild(0);

                    TransformConfig transform = new TransformConfig();
                    transform.translate = mr.position;
                    transform.rotate = mr.rotation;

                    Transmitter.SendTransform(display.tracker, transform);
                }
                
                if (display.leftHandTracker != null)
                    Transmitter.SendTransform(display.leftHandTracker, leftHand.transform);

                if (display.rightHandTracker != null)
                    Transmitter.SendTransform(display.rightHandTracker, rightHand.transform);

                if (display.cursorTracker != null)
                    Transmitter.SendTransform(display.cursorTracker, cursor.transform);
            };
        }
    }
}