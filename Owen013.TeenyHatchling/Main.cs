using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using SmolHatchling.Components;
using System.Reflection;

namespace SmolHatchling
{
    public class Main : ModBehaviour
    {
        public static Main Instance { get; private set; }

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
            gameObject.AddComponent<StoolManager>();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            // Ready
            ModHelper.Console.WriteLine($"Smol Hatchling is ready to go!", MessageType.Success);
        }
    }
}