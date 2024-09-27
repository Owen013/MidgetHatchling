using SmolHatchling.Components;
using System;
using UnityEngine;

namespace SmolHatchling;

public class SmolHatchlingAPI
{
    /// <summary>
    /// Returns the CURRENT scale of the player.
    /// </summary>
    public float GetPlayerScale()
    {
        if (PlayerScaleController.Instance == null) return 1;
        return PlayerScaleController.Instance.Scale;
    }

    /// <summary>
    /// Returns the FINAL scale that the player is easing towards
    /// </summary>
    public float GetPlayerTargetScale()
    {
        if (PlayerScaleController.Instance == null) return 1;
        return PlayerScaleController.Instance.TargetScale;
    }

    /// <summary>
    /// Returns the animation speed multiplier.
    /// </summary>
    public float GetPlayerAnimSpeed()
    {
        return PlayerScaleController.AnimSpeed;
    }

    public bool UseScaledPlayerAttributes()
    {
        return ModMain.Instance.GetConfigSetting<bool>("ScalePlayerSpeed");
    }

    [Obsolete]
    public Vector3 GetTargetScale()
    {
        ModMain.Instance.Print("GetTargetScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Debug);
        return Vector3.one * PlayerScaleController.Instance.TargetScale;
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