using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
#if UNITY_WSA
using UnityEngine.XR.WSA.Input;
#endif

namespace HEVS.UniSA {

    /// <summary>
    /// Used to handle input for a MixedRealityDisplay.
    /// </summary>
    internal class InputHandler {

#if UNITY_WSA
        // Recognises gestures performed on the HoloLens.
        private GestureRecognizer _gestureRecognizer;
#endif
        // Recognises speech performed on the HoloLens.
        private DictationRecognizer _speechRecognizer;

        // The Tracker config to handle the input
        private TrackerConfig _tracker;

        // The handedness of the tracker.
        private TrackerHandedness _handedness;

        public InputHandler(TrackerConfig tracker) : this(tracker, TrackerHandedness.Left | TrackerHandedness.Right) { }

        public InputHandler(TrackerConfig tracker, TrackerHandedness handedness) {
            _tracker = tracker;
            _handedness = handedness;

#if UNITY_WSA
            _gestureRecognizer = new GestureRecognizer();
            GestureSettings gestureMask = GetGestureMask(tracker);

            // Using gestures
            if (gestureMask != GestureSettings.None)
            {
                // Add tap to recognisor
                if (gestureMask.HasFlag(GestureSettings.Tap) || gestureMask.HasFlag(GestureSettings.DoubleTap))
                    _gestureRecognizer.Tapped += GestureTapped;

                // Add hold to recognisor
                if (gestureMask.HasFlag(GestureSettings.Hold)) {
                    _gestureRecognizer.HoldStarted += GestureHoldStarted;
                    _gestureRecognizer.HoldCompleted += GestureHoldCompleted;
                    _gestureRecognizer.HoldCanceled += GestureHoldCanceled;
                }

                // Add manipulation to recognisor
                if (gestureMask.HasFlag(GestureSettings.ManipulationTranslate)) {
                    _gestureRecognizer.ManipulationStarted += GestureManipulationStarted;
                    _gestureRecognizer.ManipulationCompleted += GestureManipulationCompleted;
                    _gestureRecognizer.ManipulationCanceled += GestureManipulationCanceled;
                    _gestureRecognizer.ManipulationUpdated += GestureManipulationUpdated; ;
                }

                // Add navigation to recognisor
                if (gestureMask.HasFlag(GestureSettings.NavigationX) || gestureMask.HasFlag(GestureSettings.NavigationY)
                    || gestureMask.HasFlag(GestureSettings.NavigationZ)) {
                    _gestureRecognizer.NavigationStarted += GestureNavigationStarted;
                    _gestureRecognizer.NavigationCompleted += GestureNavigationCompleted;
                    _gestureRecognizer.NavigationCanceled += GestureNavigationCanceled;
                    _gestureRecognizer.NavigationUpdated += GestureNavigationUpdated; ;
                }
                
                _gestureRecognizer.SetRecognizableGestures(gestureMask);
                _gestureRecognizer.StartCapturingGestures();
            }
#endif

            // Using speech recognition
            _speechRecognizer = new DictationRecognizer();
            _speechRecognizer.DictationResult += SpeechRecognition;
        }

#if UNITY_WSA

        #region Taps and Hold

        private void GestureTapped(TappedEventArgs args)
        {
            if (!CorrectHand(args.source)) return;
            if (args.tapCount == 1) { Transmitter.SendButton(_tracker, "tap", null); }
            else Transmitter.SendButton(_tracker, "double_tap", null);
        }

        private void GestureHoldStarted(HoldStartedEventArgs args) {
            if (CorrectHand(args.source)) Transmitter.SendButton(_tracker, "hold", true);
        }

        private void GestureHoldCompleted(HoldCompletedEventArgs args) {
            if (CorrectHand(args.source)) Transmitter.SendButton(_tracker, "hold", false);
        }
        private void GestureHoldCanceled(HoldCanceledEventArgs args) {
            if (CorrectHand(args.source)) Transmitter.SendButton(_tracker, "hold", false);
        }
        #endregion

        #region Manipulation

        // Manipulation Started
        private void GestureManipulationStarted(ManipulationStartedEventArgs args) {
            if (CorrectHand(args.source)) {
                Transmitter.SendButton(_tracker, "manipulation", true);
                SetManipulation(0f, 0f, 0f);
            }
        }

        // Manipulation Updated
        private void GestureManipulationUpdated(ManipulationUpdatedEventArgs args) {
            if (CorrectHand(args.source))
            {
                Transmitter.SendButton(_tracker, "manipulation", true);
                SetManipulation(args.cumulativeDelta.x, args.cumulativeDelta.y, args.cumulativeDelta.z);
            }
        }

        // Manipulation Ended
        private void GestureManipulationCanceled(ManipulationCanceledEventArgs args) {
            if (CorrectHand(args.source)) {
                Transmitter.SendButton(_tracker, "manipulation", false);
                SetManipulation(0f, 0f, 0f);
            }
        }
        private void GestureManipulationCompleted(ManipulationCompletedEventArgs args) {
            if (CorrectHand(args.source)) {
                Transmitter.SendButton(_tracker, "manipulation", false);
                SetManipulation(0f, 0f, 0f);
            }
        }
        #endregion

        #region Navigation

        // Navigation Started
        private void GestureNavigationStarted(NavigationStartedEventArgs args)
        {
            if (CorrectHand(args.source)) {
                Transmitter.SendButton(_tracker, "navigation", true);
                SetNavigation(0f, 0f, 0f);
            }
        }

        // Navigation Updated
        private void GestureNavigationUpdated(NavigationUpdatedEventArgs args) {
            if (CorrectHand(args.source))
            {
                Transmitter.SendButton(_tracker, "navigation", true);
                SetNavigation(args.normalizedOffset.x, args.normalizedOffset.y, args.normalizedOffset.z);
            }
        }

        // Navigation Ended
        private void GestureNavigationCanceled(NavigationCanceledEventArgs args) {
            if (CorrectHand(args.source)) {
                Transmitter.SendButton(_tracker, "navigation", false);
                SetNavigation(0f, 0f, 0f);
            }
        }
        private void GestureNavigationCompleted(NavigationCompletedEventArgs args) {
            if (CorrectHand(args.source)) {
                Transmitter.SendButton(_tracker, "navigation", false);
                SetNavigation(0f, 0f, 0f);
            }
        }
        #endregion

        #region Helpers

        private void SetManipulation(float x, float y, float z) {
            Transmitter.SendButton(_tracker, "manipulation_x", x);
            Transmitter.SendButton(_tracker, "manipulation_y", y);
            Transmitter.SendButton(_tracker, "manipulation_z", z);
        }

        private void SetNavigation(float x, float y, float z) {
            Transmitter.SendButton(_tracker, "navigation_x", x);
            Transmitter.SendButton(_tracker, "navigation_y", y);
            Transmitter.SendButton(_tracker, "navigation_z", z);
        }

        // Get the handedness of a source
        private bool CorrectHand(InteractionSource source) {
            if (_handedness.HasFlag(TrackerHandedness.Left | TrackerHandedness.Right)) return true;

            return (source.handedness == InteractionSourceHandedness.Right)
                == (TrackerHandedness.Right == _handedness);
        }

        // Get the gesture mask for a tracker
        private GestureSettings GetGestureMask(TrackerConfig tracker) {
            GestureSettings gestureMask = GestureSettings.None;

            // Check for gestures
            foreach (var axis in tracker.json["buttons"].Children) {
                switch (axis["id"]) {
                    case "tap":
                    gestureMask |= GestureSettings.Tap;
                    break;

                    case "double_tap":
                    gestureMask |= GestureSettings.DoubleTap;
                    break;

                    case "hold":
                    gestureMask |= GestureSettings.Hold;
                    break;

                    case "manipulation_x":
                    case "manipulation_y":
                    case "manipulation_z":
                    gestureMask |= GestureSettings.ManipulationTranslate;
                    break;

                    case "navigation_x":
                        gestureMask |= GestureSettings.NavigationX;
                        break;

                    case "navigation_y":
                        gestureMask |= GestureSettings.NavigationY;
                        break;

                    case "navigation_z":
                        gestureMask |= GestureSettings.NavigationZ;
                        break;
                }
            }

            return gestureMask;
        }
        #endregion
#endif

        private void SpeechRecognition(string text, ConfidenceLevel confidence) {
            if (confidence != ConfidenceLevel.Rejected) {
                string button = $"speech: { text }";

                // TODO: check that button exists

                Transmitter.SendButton(_tracker, button, true);
            }
        }
    }
}