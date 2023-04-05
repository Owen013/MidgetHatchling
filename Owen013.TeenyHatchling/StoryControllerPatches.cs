using HarmonyLib;
using UnityEngine;

namespace SmolHatchling
{
    public static class StoryControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChertDialogueSwapper), nameof(ChertDialogueSwapper.SelectMood))]
        public static void ChertDialogueSwapped()
        {
            StoryController.Instance.ChangeDialogueTree("Chert");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.EndConversation))]
        public static void EndConversation()
        {
            if (DialogueConditionManager.s_instance.GetConditionState("Busted") && !StoryController.Instance._busted)
            {
                StoryController.Instance._busted = true;
                Locator.GetShipBody().GetComponentInChildren<ShipCockpitController>().LockUpControls(Mathf.Infinity);
                NotificationManager.s_instance.PostNotification(new NotificationData(NotificationTarget.Player, "SHIP HAS BEEN DISABLED BY GROUND CONTROL", 5f), false);
                NotificationManager.s_instance.PostNotification(new NotificationData(NotificationTarget.Ship, "SHIP DISABLED"), true);
            }
        }
    }
}