using SmolHatchling.Components;
using UnityEngine;

namespace SmolHatchling;

public class SmolHatchlingAPI
{
    public Vector3 GetTargetScale()
    {
        return ScaleController.Instance.TargetScale;
    }

    public Vector3 GetCurrentScale()
    {
        return ScaleController.Instance.CurrentScale;
    }

    public float GetAnimSpeed()
    {
        return PlayerModelController.AnimSpeed;
    }

    //public bool ScalePlayerAttributes()
    //{
    //    return Config.UseScaledPlayerAttributes;
    //}
}