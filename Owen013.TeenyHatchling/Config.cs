using OWML.Common;
namespace SmolHatchling;

public static class Config
{
    public static bool UseCustomPlayerScale { get; private set; }

    public static float PlayerScale { get; private set; }

    public static bool UseScaleHotkeys { get; private set; }

    public static bool UseScaledPlayerSpeed { get; private set; }

    public static bool UseScaledPlayerDamage { get; private set; }

    public delegate void ConfigureEvent();

    public static event ConfigureEvent OnConfigured;

    public static void Configure()
    {
        if (ModMain.Instance == null || ModMain.Instance.ModHelper == null) return;

        IModConfig config = ModMain.Instance.ModHelper.Config;
        UseCustomPlayerScale = config.GetSettingsValue<bool>("UseCustomPlayerScale");
        PlayerScale = config.GetSettingsValue<float>("PlayerScale");
        UseScaleHotkeys = config.GetSettingsValue<bool>("UseScaleHotkeys");
        UseScaledPlayerSpeed = config.GetSettingsValue<bool>("UseScaledPlayerSpeed");
        UseScaledPlayerDamage = config.GetSettingsValue<bool>("UseScaledPlayerDamage");

        OnConfigured?.Invoke();
    }

    public static void SetConfigSetting(string settingName, object value)
    {
        ModMain.Instance.ModHelper.Config.SetSettingsValue(settingName, value);
        Configure();
    }
}
