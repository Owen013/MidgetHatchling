using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using SmolHatchling.Components;
using System.Reflection;

namespace SmolHatchling;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public override object GetApi()
    {
        return new SmolHatchlingAPI();
    }

    public override void Configure(IModConfig config)
    {
        base.Configure(config);
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
        if (ModHelper.Interaction.ModExists("Owen013.MovementMod"))
        {
            ModHelper.HarmonyHelper.AddPrefix<DreamLanternItem>(nameof(DreamLanternItem.OverrideMaxRunSpeed), typeof(PlayerScaleController), nameof(PlayerScaleController.OverrideMaxRunSpeed));
        }
    }

    private void Start()
    {
        Print($"Smol Hatchling is ready to go!", MessageType.Success);
    }
}