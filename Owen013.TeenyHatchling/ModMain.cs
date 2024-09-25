using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace SmolHatchling;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public override object GetApi()
    {
        return new SmolHatchlingAPI();
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
        
    }
}