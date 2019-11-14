using UnityEngine;

namespace HEVS.UniSA
{
    internal interface OriginFinder
    {
        bool TryGetOrigin(out Vector3 position, out Quaternion rotation);

        void Disable();
    }
}