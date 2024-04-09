using SmolHatchling.Components;
using UnityEngine;

namespace SmolHatchling;

public class SmolHatchlingAPI
{
    public Vector3 GetTargetScale()
    {
        if (ScaleController.Instance == null) return Vector3.one;
        return ScaleController.Instance.TargetScale;
    }

    public Vector3 GetCurrentScale()
    {
        if (ScaleController.Instance == null) return Vector3.one;
        return ScaleController.Instance.CurrentScale;
    }

    public float GetAnimSpeed()
    {
        return PlayerModelController.AnimSpeed;
    }

    public bool UseScaledPlayerAttributes()
    {
        return Config.UseScaledPlayerAttributes;
    }
}