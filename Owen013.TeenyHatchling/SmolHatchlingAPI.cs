using SmolHatchling.Components;
using System;
using UnityEngine;

namespace SmolHatchling;

public class SmolHatchlingAPI
{
    [Obsolete("Deprecated; use GetTargetScale() instead")]
    public Vector3 GetCurrentScale()
    {
        return ScaleController.Instance.TargetScale;
    }

    public Vector3 GetTargetScale()
    {
        return ScaleController.Instance.TargetScale;
    }

    public float GetAnimSpeed()
    {
        return ScaleController.Instance.AnimSpeed;
    }
}