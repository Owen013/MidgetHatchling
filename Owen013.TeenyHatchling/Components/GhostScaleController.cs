using HarmonyLib;
using UnityEngine;

namespace SmolHatchling.Components;

[HarmonyPatch]
public class GhostScaleController : ScaleController
{
    public static float DefaultScale = 1;

    protected override void FixedUpdate()
    {
        if (ModMain.UseOtherCustomScales && TargetScale != ModMain.CustomInhabitantScale)
        {
            SetTargetScale(ModMain.CustomInhabitantScale);
        }

        base.FixedUpdate();
    }

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostBrain), nameof(GhostBrain.Start))]
    private static void AddScaleControllerToGhost(GhostBrain __instance)
    {
        ScaleController scaleController = __instance.gameObject.AddComponent<GhostScaleController>();
        // fire on the next update to avoid breaking things
        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            if (ModMain.UseOtherCustomScales)
            {
                scaleController.Scale = ModMain.CustomInhabitantScale;
            }
            else
            {
                scaleController.Scale = DefaultScale;
            }

            __instance.transform.position += __instance.transform.up * (-1 + scaleController.Scale);
        });
    }
}