using SmolHatchling.Components;
using System;
using UnityEngine;

namespace SmolHatchling;

public class SmolHatchlingAPI
{
    /// <summary>
    /// Returns the current scale of the player.
    /// </summary>
    public float GetPlayerScale()
    {
        if (PlayerScaleController.Instance == null) return 1;
        return PlayerScaleController.Instance.Scale;
    }

    /// <summary>
    /// The default scale of the player.
    /// If set before the first update, the player will be this size when they start.
    /// If set after, the player will slowly ease to this size.
    /// </summary>
    /// <param name="defaultScale">The default scale the player should be.</param>
    public void SetPlayerDefaultScale(float defaultScale)
    {
        PlayerScaleController.s_defaultScale = defaultScale;
    }

    /// <summary>
    /// Returns the final scale that the player is easing towards.
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

    /// <summary>
    /// Returns true if Smol Hatchling is scaling the player's speed to match their size.
    /// </summary>
    public bool UseScaledPlayerSpeed()
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

    [Obsolete]
    public bool UseScaledPlayerAttributes()
    {
        return ModMain.Instance.GetConfigSetting<bool>("ScalePlayerSpeed");
    }
}