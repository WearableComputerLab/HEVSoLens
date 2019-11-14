#if UNITY_WSA
using UnityEngine;
using UnityEngine.XR.WSA.Input;
#endif

namespace HEVS.UniSA
{

    /// <summary>
    /// Used to track the position of a hand for a Mixed Reality headset.
    /// </summary>
    internal class HandLocator
    {

        private TransformConfig _transform;
        /// <summary>
        /// The current transform of the hand.
        /// </summary>
        public TransformConfig transform => _transform;

        /// <summary>
        /// The current transform of the hand.
        /// </summary>
        public TrackerHandedness _handedness { get; }

        /// <summary>
        /// Constructs a hand tracker object.
        /// </summary>
        /// <param name="handedness">Which hand to use.</param>
        public HandLocator(TrackerHandedness handedness)
        {
            _handedness = handedness;
#if UNITY_WSA
            InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
#endif
        }

#if UNITY_WSA

        // Sends the hands transform when it updates.
        private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            if (!CorrectHand(args.state.source)) return;
            
            // Try getting the position and rotation
            args.state.sourcePose.TryGetPosition(out Vector3 position);
            args.state.sourcePose.TryGetRotation(out Quaternion rotation);

            // Send all the transform data
            _transform.translate = position;
            _transform.rotate = rotation;
        }

        // Get the handedness of a source
        private bool CorrectHand(InteractionSource source)
        {
            return (source.handedness == InteractionSourceHandedness.Right)
                == (TrackerHandedness.Right == _handedness);
        }
#endif
    }
}