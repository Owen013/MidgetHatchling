using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using SmolHatchling.Components;
using SmolHatchling.Interfaces;
using System.Reflection;

namespace SmolHatchling;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public static IHikersMod HikersModAPI { get; private set; }

    public static bool IsImmersionInstalled { get; private set; }

    public static bool UseCustomPlayerScale { get; private set; }

    public static float CustomPlayerScale { get; private set; }

    public static bool UseScaleHotkeys { get; private set; }

    public static bool UseOtherCustomScales { get; private set; }

    public static float CustomAnglerfishScale { get; private set; }

    public static float CustomJellyfishScale { get; private set; }

    public static float CustomInhabitantScale { get; private set; }

    public delegate void ConfigureEvent();

    public static event ConfigureEvent OnConfigured;

    public static void Print(string text, MessageType messageType = MessageType.Message)
    {
        if (Instance == null || Instance.ModHelper == null) return;
        Instance.ModHelper.Console.WriteLine(text, messageType);
    }

    public override object GetApi()
    {
        return new SmolHatchlingAPI();
    }

    public override void Configure(IModConfig config)
    {
        base.Configure(config);

        UseCustomPlayerScale = config.GetSettingsValue<bool>("UseCustomPlayerScale");
        CustomPlayerScale = config.GetSettingsValue<float>("CustomPlayerScale");
        UseScaleHotkeys = config.GetSettingsValue<bool>("UseScaleHotkeys");
        UseOtherCustomScales = config.GetSettingsValue<bool>("UseOtherCustomScales");
        CustomAnglerfishScale = config.GetSettingsValue<float>("CustomAnglerfishScale");
        CustomJellyfishScale = config.GetSettingsValue<float>("CustomJellyfishScale");
        CustomInhabitantScale = config.GetSettingsValue<float>("CustomInhabitantScale");

        if (CustomPlayerScale <= 0)
        {
            Print("Player Scale cannot be 0 or less.", MessageType.Error);
            SetConfigSetting("CustomPlayerScale", 1);
        }

        if (CustomAnglerfishScale <= 0)
        {
            Print("Anglerfish Scale cannot be 0 or less.", MessageType.Error);
            SetConfigSetting("CustomAnglerfishScale", 1);
        }

        if (CustomJellyfishScale <= 0)
        {
            Print("Jellyfish Scale cannot be 0 or less.", MessageType.Error);
            SetConfigSetting("CustomJellyfishScale", 1);
        }

        if (CustomInhabitantScale <= 0)
        {
            Print("Inhabitant Scale cannot be 0 or less.", MessageType.Error);
            SetConfigSetting("CustomInhabitantScale", 1);
        }

        OnConfigured?.Invoke();
    }

    public void Configure()
    {
        Configure(ModHelper.Config);
    }

    public void SetConfigSetting(string settingName, object value)
    {
        ModHelper.Config.SetSettingsValue(settingName, value);
        Configure();
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        HikersModAPI = ModHelper.Interaction.TryGetModApi<IHikersMod>("Owen013.MovementMod");
        IsImmersionInstalled = ModHelper.Interaction.ModExists("Owen_013.FirstPersonPresence");

        if (HikersModAPI != null)
        {
            ModHelper.HarmonyHelper.AddPrefix<DreamLanternItem>(nameof(DreamLanternItem.OverrideMaxRunSpeed), typeof(PlayerScaleController), nameof(PlayerScaleController.DreamLanternItem_OverrideMaxRunSpeed));
        }

        Print($"Smol Hatchling is ready to go!", MessageType.Success);
    }
}