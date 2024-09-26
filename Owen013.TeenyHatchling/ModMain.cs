using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using ScaleManipulator.Components;
using ScaleManipulator.Interfaces;
using System.Reflection;

namespace ScaleManipulator;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public static IHikersMod HikersModAPI { get; private set; }

    public override object GetApi()
    {
        return new ScaleManipulatorAPI();
    }

    public override void Configure(IModConfig config)
    {
        base.Configure(config);
        if (GetConfigSetting<bool>("UseCustomPlayerScale") && PlayerScaleController.Instance != null)
        {
            PlayerScaleController.Instance.targetScale = GetConfigSetting<float>("PlayerScale");
        }
    }

    public T GetConfigSetting<T>(string settingName)
    {
        return ModHelper.Config.GetSettingsValue<T>(settingName);
    }

    public void Print(string text, MessageType messageType = MessageType.Message)
    {
        ModHelper.Console.WriteLine(text, messageType);
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        HikersModAPI = ModHelper.Interaction.TryGetModApi<IHikersMod>("Owen013.MovementMod");

        if (ModHelper.Interaction.ModExists("Owen013.MovementMod"))
        {
            ModHelper.HarmonyHelper.AddPrefix<DreamLanternItem>(nameof(DreamLanternItem.OverrideMaxRunSpeed), typeof(PlayerScaleController), nameof(PlayerScaleController.OverrideMaxRunSpeed));
        }

        Print($"Smol Hatchling is ready to go!", MessageType.Success);
    }
}