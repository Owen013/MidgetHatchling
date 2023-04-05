using HarmonyLib;
using UnityEngine;

namespace SmolHatchling
{
    public static class SmolHatchlingPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.Start))]
        public static void CharacterStart()
        {
            SmolHatchlingController.Instance.OnCharacterStart();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 targetScale = SmolHatchlingController.Instance._targetScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset = new Vector3(0, 1.8496f - 1.8496f * targetScale.y, 0.15f - 0.15f * targetScale.z);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerScreamingController), nameof(PlayerScreamingController.Awake))]
        public static void NPCPlayerAwake(PlayerScreamingController __instance)
        {
            SmolHatchlingController.Instance._npcPlayer = __instance.gameObject;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCloneController), nameof(PlayerCloneController.Start))]
        public static void EyeCloneStart(PlayerCloneController __instance)
        {
            Vector3 playerScale = SmolHatchlingController.Instance._playerScale;
            float pitch;
            __instance._playerVisuals.transform.localScale = playerScale / 10;
            if (SmolHatchlingController.Instance._pitchChangeEnabled) pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
            else pitch = 1;
            __instance._signal._owAudioSource.pitch = pitch;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EyeMirrorController), nameof(EyeMirrorController.Start))]
        public static void EyeMirrorStart(EyeMirrorController __instance)
        {
            Vector3 playerScale = SmolHatchlingController.Instance._playerScale;
            __instance._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
        }
    }
}
