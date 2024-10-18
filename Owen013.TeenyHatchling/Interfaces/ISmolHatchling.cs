using System;
using UnityEngine;

namespace SmolHatchling.Interfaces;

public interface ISmolHatchling
{
    /// <summary>
    /// Returns the current scale of the player.
    /// </summary>
    public float GetPlayerScale();

    /// <summary>
    /// The default scale of the player.
    /// If set before the first update, the player will be this size when they start.
    /// If set after, the player will slowly ease to this size.
    /// </summary>
    /// <param name="scale">The default scale the player should be.</param>
    public void SetPlayerDefaultScale(float scale);

    /// <summary>
    /// Sets the default scale of the player and then instantly snaps it to a given size.
    /// </summary>
    /// <param name="scale">The scale you want to snap the player to.</param>
    public void SetPlayerScaleInstantly(float scale);

    /// <summary>
    /// Returns the final scale that the player is easing towards.
    /// </summary>
    public float GetPlayerTargetScale();

    /// <summary>
    /// Returns the animation speed multiplier.
    /// </summary>
    public float GetPlayerAnimSpeed();

    /// <summary>
    /// Returns true if Smol Hatchling is scaling the player's speed to match their size.
    /// </summary>
    public bool UseScaledPlayerSpeed();

    /// <summary>
    /// Resizes a GameObject using its ScaleController. If the GameObject does not have a ScaleController, one will be created.
    /// </summary>
    /// <param name="gameObject">The GameObject to resize.</param>
    /// <param name="scale">The size you want the GameObject to be.</param>
    public void SetGameObjectScale(GameObject gameObject, float scale);

    [Obsolete("GetTargetScale() is deprecated. Use GetPlayerScale() instead.")]
    public Vector3 GetTargetScale();

    [Obsolete("GetCurrentScale() is deprecated. Use GetPlayerScale() instead.")]
    public Vector3 GetCurrentScale();

    [Obsolete("GetAnimSpeed() is deprecated. Use GetPlayerAnimSpeed() instead.")]
    public float GetAnimSpeed();

    [Obsolete("UseScaledPlayerAttributes() is deprecated. Use UseScaledPlayerSpeed() instead.")]
    public bool UseScaledPlayerAttributes();
}