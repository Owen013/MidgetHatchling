using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using SmolHatchling.Interfaces;
using System.Reflection;

namespace SmolHatchling;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }
    public IImmersion ImmersionAPI { get; private set; }
    public bool IsHikersModInstalled { get; private set; }

    public override object GetApi()
    {
        return new SmolHatchlingAPI();
    }

    public override void Configure(IModConfig config)
    {
        Config.UpdateConfig(config);
    }
    public void WriteLine(string text, MessageType type = MessageType.Message)
    {
        Instance.ModHelper.Console.WriteLine(text, type);
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        ImmersionAPI = ModHelper.Interaction.TryGetModApi<IImmersion>("Owen_013.FirstPersonPresence");
        IsHikersModInstalled = ModHelper.Interaction.ModExists("Owen013.MovementMod");

        LoadManager.OnCompleteSceneLoad += StoolManager.OnSceneLoaded;
        ModHelper.Console.WriteLine($"Smol Hatchling is ready to go!", MessageType.Success);
    }
}