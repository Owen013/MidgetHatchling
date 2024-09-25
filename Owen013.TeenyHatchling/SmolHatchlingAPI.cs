using SmolHatchling.Components;
using System;
using UnityEngine;

namespace SmolHatchling;

public class SmolHatchlingAPI
{
    public float GetPlayerScale()
    {
        return PlayerScaleController.Instance.scale;
    }

    public float GetPlayerAnimSpeed()
    {
        return PlayerScaleController.animSpeed;
    }

    public bool UseScaledPlayerAttributes()
    {
        return ModMain.Instance.GetConfigSetting<bool>("UseScaledPlayerAttributes");
    }

    [Obsolete]
    public Vector3 GetTargetScale()
    {
        ModMain.Instance.Print("GetTargetScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Warning);
        return Vector3.one * PlayerScaleController.Instance.scale;
    }

    public Vector3 GetCurrentScale()
    {
        ModMain.Instance.Print("GetCurrentScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Warning);
        return Vector3.one * PlayerScaleController.Instance.scale;
    }

    [Obsolete]
    public float GetAnimSpeed()
    {
        ModMain.Instance.Print("GetAnimSpeed() is deprecated. Use GetPlayerAnimSpeed() instead.", OWML.Common.MessageType.Warning);
        return PlayerScaleController.animSpeed;
    }
}