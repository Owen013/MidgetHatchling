using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using SmolHatchling.Components;
using SmolHatchling.Interfaces;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmolHatchling;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public static IHikersMod HikersModAPI { get; private set; }

    private float _resetButtonHoldTime;

    public override object GetApi()
    {
        return new SmolHatchlingAPI();
    }

    public override void Configure(IModConfig config)
    {
        base.Configure(config);
        if (PlayerScaleController.Instance != null)
        {
            if (GetConfigSetting<bool>("UseCustomPlayerScale"))
            {
                PlayerScaleController.Instance.SetTargetScale(GetConfigSetting<float>("PlayerScale"));
            }
            else
            {
                PlayerScaleController.Instance.SetTargetScale(PlayerScaleController.s_defaultScale);
            }
        }
    }

    public T GetConfigSetting<T>(string settingName)
    {
        return ModHelper.Config.GetSettingsValue<T>(settingName);
    }

    public void SetConfigSetting(string settingName, object value)
    {
        ModHelper.Config.SetSettingsValue(settingName, value);
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
            ModHelper.HarmonyHelper.AddPrefix<DreamLanternItem>(nameof(DreamLanternItem.OverrideMaxRunSpeed), typeof(PlayerScaleController), nameof(PlayerScaleController.DreamLanternItem_OverrideMaxRunSpeed));
        }

        Print($"Smol Hatchling is ready to go!", MessageType.Success);
    }

    private void Update()
    {
        if (Keyboard.current[Key.Slash].isPressed)
        {
            if (_resetButtonHoldTime >= 5f)
            {
                SetConfigSetting("UseCustomPlayerScale", false);
                if (PlayerScaleController.Instance != null)
                {
                    PlayerScaleController.Instance.SetTargetScale(PlayerScaleController.s_defaultScale);
                }
                _resetButtonHoldTime = 0;
                Print("'Use Custom Player Scale' disabled");
            }
            else
            {
                _resetButtonHoldTime += Time.unscaledDeltaTime;
            }
        }
        else
        {
            _resetButtonHoldTime = 0;
        }
    }
}