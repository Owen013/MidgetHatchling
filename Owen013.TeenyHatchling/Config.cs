using OWML.Common;

namespace SmolHatchling;

public static class Config
{
    public static float PlayerHeight { get; private set; }
    public static float PlayerRadius { get; private set; }
    public static string ColliderMode { get; private set; }
    public static bool IsPitchChangeEnabled { get; private set; }
    public static bool IsStoolsEnabled { get; private set; }
    public static bool AutoScaleStools { get; private set; }
    public static float StoolHeight { get; private set; }

    public delegate void ConfigureEvent();
    public static event ConfigureEvent OnConfigure;

    public static void UpdateConfig(IModConfig config)
    {
        float heightSetting = config.GetSettingsValue<float>("Height");
        if (heightSetting == 0f)
        {
            ModMain.Instance.WriteLine("Player height cannot be 0.", MessageType.Warning);
            config.SetSettingsValue("Height", 1f);
            heightSetting = 1f;
        }
        PlayerHeight = heightSetting;

        float radiusSetting = config.GetSettingsValue<float>("Radius");
        if (heightSetting == 0f)
        {
            ModMain.Instance.WriteLine("Player radius cannot be 0.", MessageType.Warning);
            config.SetSettingsValue("Height", 1f);
            radiusSetting = 1f;
        }
        PlayerRadius = heightSetting;

        ColliderMode = config.GetSettingsValue<string>("Resize Collider");
        IsPitchChangeEnabled = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
        IsStoolsEnabled = config.GetSettingsValue<bool>("Enable Stools");
        AutoScaleStools = config.GetSettingsValue<bool>("Auto-Adjust Stool Height");
        StoolHeight = config.GetSettingsValue<float>("Stool Height");

        OnConfigure?.Invoke();
    }
}
