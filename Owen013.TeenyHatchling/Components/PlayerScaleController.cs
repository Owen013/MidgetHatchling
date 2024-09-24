using HarmonyLib;
using UnityEngine;

namespace SmolHatchling.Components;

[HarmonyPatch]
public class PlayerScaleController : ScaleController
{

    public override Vector3 scale
    {
        get
        {
            return gameObject.transform.localScale;
        }

        set
        {
            gameObject.transform.localScale = Vector3.one * value.x;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerBody), nameof(PlayerBody.Awake))]
    private static void AddToPlayerBody(PlayerBody __instance)
    {
        __instance.gameObject.AddComponent<PlayerScaleController>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.CastForGrounded))]
    private static bool PlayerCharacterController_CastForGrounded(PlayerCharacterController __instance)
    {
        if (!ModMain.Instance.GetConfigSetting<bool>("UseCustomPlayerScale") || ModMain.Instance.GetConfigSetting<float>("PlayerScale") == 1) return true;



        return false;
    }

    private void Update()
    {
        if (ModMain.Instance.GetConfigSetting<bool>("UseCustomPlayerScale"))
        {
            scale = Vector3.one * ModMain.Instance.GetConfigSetting<float>("PlayerScale");
        }
    }
}