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
    /// <param name="defaultScale">The default scale the player should be.</param>
    public void SetPlayerDefaultScale(float defaultScale);

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
}
