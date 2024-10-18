using HarmonyLib;
using SmolHatchling.Components;
using UnityEngine;

namespace SmolHatchling;

[HarmonyPatch]
public static class GhostSpeedHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GhostController), nameof(GhostController.UpdatePositionFromVelocity))]
    private static bool GhostController_MoveToLocalPosition(GhostController __instance)
    {
        ScaleController scaleController = __instance.GetComponent<ScaleController>();
        if (scaleController == null || scaleController.Scale == 1) return true;

        Vector3 vector = __instance.transform.localPosition + __instance._velocity * scaleController.Scale * Time.fixedDeltaTime;
        if (__instance._ghostCollider != null && __instance._ghostCollider.enabled && __instance._playerCollider != null && !__instance._grabController.enabled)
        {
            Vector3 positionB = __instance.WorldToLocalPosition(__instance._playerCollider.transform.position);
            Quaternion rotationB = __instance._nodeRoot.InverseTransformRotation(__instance._playerCollider.transform.rotation);
            Vector3 a;
            float d;
            if (Physics.ComputePenetration(__instance._ghostCollider, vector, __instance.transform.localRotation, __instance._playerCollider, positionB, rotationB, out a, out d))
            {
                vector += a * d;
            }
        }

        __instance.transform.localPosition = vector;

        return false;
    }
}