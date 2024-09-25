using ScaleManipulator.Components;
using System;
using UnityEngine;

namespace ScaleManipulator;

public class ScaleManipulatorAPI
{
    public float GetPlayerScale()
    {
        return PlayerScaleController.Instance.Scale;
    }

    public float GetPlayerAnimSpeed()
    {
        return PlayerScaleController.AnimSpeed;
    }

    public bool UseScaledPlayerAttributes()
    {
        return ModMain.Instance.GetConfigSetting<bool>("UseScaledPlayerAttributes");
    }

    [Obsolete]
    public Vector3 GetTargetScale()
    {
        ModMain.Instance.Print("GetTargetScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Debug);
        return Vector3.one * PlayerScaleController.Instance.Scale;
    }

    [Obsolete]
    public Vector3 GetCurrentScale()
    {
        ModMain.Instance.Print("GetCurrentScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Debug);
        return Vector3.one * PlayerScaleController.Instance.Scale;
    }

    [Obsolete]
    public float GetAnimSpeed()
    {
        ModMain.Instance.Print("GetAnimSpeed() is deprecated. Use GetPlayerAnimSpeed() instead.", OWML.Common.MessageType.Debug);
        return PlayerScaleController.AnimSpeed;
    }
}