using HarmonyLib;
using SmolHatchling.Components;
using UnityEngine;

namespace SmolHatchling;

[HarmonyPatch]
public static class Patches
{
    // this one is manually added if Hiker's Mod is not installed
    public static bool OverrideMaxRunSpeed(ref float maxSpeedX, ref float maxSpeedZ, DreamLanternItem __instance)
    {
        if (!Config.UseScaledPlayerAttributes) return true;

        float lerpPosition = 1f - __instance._lanternController.GetFocus();
        lerpPosition *= lerpPosition;
        maxSpeedX = Mathf.Lerp(2f * ScaleController.Instance.TargetScale.x, maxSpeedX, lerpPosition);
        maxSpeedZ = Mathf.Lerp(2f * ScaleController.Instance.TargetScale.x, maxSpeedZ, lerpPosition);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.Awake))]
    public static void CharacterControllerAwake(PlayerCharacterController __instance)
    {
        __instance.gameObject.AddComponent<ScaleController>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.Start))]
    public static void AnimControllerStart(PlayerCharacterController __instance)
    {
        __instance.gameObject.AddComponent<PlayerModelController>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
    public static void GhostLiftedPlayer(GhostGrabController __instance)
    {
        Vector3 targetScale = ScaleController.Instance.TargetScale;
        // Offset attachment so that camera is where it normally is
        __instance._attachPoint._attachOffset = new Vector3(0, 1.8496f - 1.8496f * targetScale.y, 0.15f - 0.15f * targetScale.z);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    public static void SetRunAnimFloats(PlayerAnimController __instance)
    {
        Vector3 vector = Vector3.zero;
        if (!PlayerState.IsAttached())
        {
            vector = Locator.GetPlayerController().GetRelativeGroundVelocity();
        }

        if (Mathf.Abs(vector.x) < 0.05f)
        {
            vector.x = 0f;
        }

        if (Mathf.Abs(vector.z) < 0.05f)
        {
            vector.z = 0f;
        }
        __instance._animator.SetFloat("RunSpeedX", vector.x / (3f * ScaleController.Instance.TargetScale.x));
        __instance._animator.SetFloat("RunSpeedY", vector.z / (3f * ScaleController.Instance.TargetScale.x));
    }
}