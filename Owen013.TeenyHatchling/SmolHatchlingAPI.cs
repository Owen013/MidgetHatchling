using UnityEngine;

namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        // This API was added for support with HikersMod, since HikersMod needs to know the scale and animspeed of the
        // hatchling.
        public Vector3 GetPlayerScale()
        {
            return SmolHatchling.Instance.playerScale;
        }

        public float GetAnimSpeed()
        {
            return SmolHatchling.Instance.animSpeed;
        }

        public void SetHikersModEnabled()
        {
            SmolHatchling.Instance.hikersModEnabled = true;
        }
    }
}
