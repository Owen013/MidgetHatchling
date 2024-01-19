using UnityEngine;

namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        public Vector3 GetCurrentScale()
        {
            return Main.Instance.GetCurrentScale();
        }

        public Vector3 GetTargetScale()
        {
            return Main.Instance.GetTargetScale();
        }

        public float GetAnimSpeed()
        {
            return Main.Instance.GetAnimSpeed();
        }
    }
}