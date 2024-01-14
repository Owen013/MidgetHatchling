using UnityEngine;

namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        public Vector3 GetCurrentScale()
        {
            return ModController.s_instance.GetCurrentScale();
        }

        public Vector3 GetTargetScale()
        {
            return ModController.s_instance.GetTargetScale();
        }

        public float GetAnimSpeed()
        {
            return ModController.s_instance.GetAnimSpeed();
        }
    }
}