using SmolHatchling.Components;
using System;
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
        return ScaleController.Instance.AnimSpeed;
    }
}