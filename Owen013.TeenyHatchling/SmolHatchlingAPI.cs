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
    /// <param name="scale">The default scale the player should be.</param>
    public void SetPlayerDefaultScale(float scale)
    {
        PlayerScaleController.DefaultScale = scale;
    }

    /// <summary>
    /// Sets the default scale of the player and then instantly snaps it to a given size.
    /// </summary>
    /// <param name="scale">The scale you want to snap the player to.</param>
    public void SetPlayerScaleInstantly(float scale)
    {
        PlayerScaleController.DefaultScale = scale;
        if (PlayerScaleController.Instance != null)
        {
            PlayerScaleController.Instance.Scale = scale;
        }
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
        return Config.UseScaledPlayerSpeed;
    }

    /// <summary>
    /// Resizes a GameObject using its ScaleController. If the GameObject does not have a ScaleController, one will be created.
    /// </summary>
    /// <param name="gameObject">The GameObject to resize.</param>
    /// <param name="scale">The size you want the GameObject to be.</param>
    public void SetGameObjectScale(GameObject gameObject, float scale)
    {
        ScaleController scaleController = gameObject.GetComponent<ScaleController>();
        if (gameObject.GetComponent<ScaleController>() == null)
        {
            if (gameObject.GetComponent<PlayerCharacterController>())
            {
                scaleController = gameObject.AddComponent<PlayerScaleController>();
            }
            else if (gameObject.GetComponent<AnglerfishController>())
            {
                scaleController = gameObject.AddComponent<AnglerfishScaleController>();
            }
            else
            {
                scaleController = gameObject.AddComponent<ScaleController>();
            }
        }

        if (scaleController is PlayerScaleController)
        {
            SetPlayerScaleInstantly(scale);
        }
        else
        {
            scaleController.Scale = scale;
        }
    }

    [Obsolete]
    public Vector3 GetTargetScale()
    {
        ModMain.Print("GetTargetScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Debug);
        return Vector3.one * PlayerScaleController.Instance.TargetScale;
    }

    [Obsolete]
    public Vector3 GetCurrentScale()
    {
        ModMain.Print("GetCurrentScale() is deprecated. Use GetPlayerScale() instead.", OWML.Common.MessageType.Debug);
        return Vector3.one * PlayerScaleController.Instance.Scale;
    }

    [Obsolete]
    public float GetAnimSpeed()
    {
        ModMain.Print("GetAnimSpeed() is deprecated. Use GetPlayerAnimSpeed() instead.", OWML.Common.MessageType.Debug);
        return PlayerScaleController.AnimSpeed;
    }

    [Obsolete]
    public bool UseScaledPlayerAttributes()
    {
        return Config.UseScaledPlayerSpeed;
    }
}