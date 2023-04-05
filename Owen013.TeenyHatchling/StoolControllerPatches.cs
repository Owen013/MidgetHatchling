using HarmonyLib;
using UnityEngine;

namespace SmolHatchling
{
    public static class StoolControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SmolHatchlingController), nameof(SmolHatchlingController.UpdatePlayerScale))]
        public static void OnUpdatePlayerScale()
        {
            StoolController.Instance.UpdateStoolSize();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWItem), nameof(OWItem.MoveAndChildToTransform))]
        [HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
        public static void ItemTransformed(OWItem __instance)
        {
            if (!__instance.GetComponent<StoolItem>()) return;

            if (__instance.GetComponentInParent<PlayerCameraController>())
            {
                __instance.transform.localPosition = new Vector3(0.2f, -0.5f, 0.3f);
            }
            else if (__instance.GetComponentInParent<StoolSocket>())
            {
                __instance.EnableInteraction(false);
                __instance.SetColliderActivation(true);
            }
            else
            {
                __instance.EnableInteraction(true);
                __instance.SetColliderActivation(true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAttachPoint), nameof(PlayerAttachPoint.AttachPlayer))]
        public static void PlayerToAttachPoint(PlayerAttachPoint __instance)
        {
            if (__instance.gameObject.GetComponentInChildren<StoolSocket>())
            {
                Vector3 playerScale = SmolHatchlingController.Instance._playerScale;
                if (__instance.gameObject.GetComponentInChildren<StoolSocket>()._socketedItem != null) __instance.SetAttachOffset(new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z));
                else __instance.SetAttachOffset(new Vector3(0, 0, 0));

            }
        }
    }
}
