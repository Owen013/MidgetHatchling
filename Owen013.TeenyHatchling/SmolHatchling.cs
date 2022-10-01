using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace SmolHatchling
{
    public class SmolHatchling : ModBehaviour
    {
        // Config vars
        public float height, radius, animSpeed;
        public bool autoRadius, pitchChangeEnabled, storyEnabled, storyEnabledNow, hikersModEnabled;

        // Mod vars
        public static SmolHatchling Instance;
        public Vector3 targetScale, playerScale;
        public GameObject playerModel, playerThruster, playerMarshmallowStick, npcPlayer;
        private PlayerBody playerBody;
        private PlayerCameraController cameraController;
        private PlayerAnimController animController;
        private CapsuleCollider playerCollider, detectorCollider;
        private CapsuleShape detectorShape;
        private ShipCockpitController cockpitController;
        private ShipLogController logController;
        private PlayerCloneController cloneController;
        private EyeMirrorController mirrorController;
        private AssetBundle textAssets, models;
        private List<OWAudioSource> playerAudio;
        private List<GameObject> stools;
        private GameObject emberTwin, ashTwin, timberHearth, attlerock, brittleHollow, hollowsLantern, giantsDeep, darkBramble,
                   quantumMoon, stranger, dreamWorld;
        private Material hearthTexture, nomaiTexture, quantumTexture, strangerTexture, dreamTexture, simTexture;

        private bool WrongScene()
        {
            OWScene scene = LoadManager.s_currentScene;
            return !(scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse);
        }

        // NewStool()

        // NewStoolSocket()

        public override object GetApi()
        {
            return new SmolHatchlingAPI();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Load assets
            textAssets = ModHelper.Assets.LoadBundle("Assets/textassets");
            models = ModHelper.Assets.LoadBundle("Assets/models");

            // Add patches
            ModHelper.HarmonyHelper.AddPostfix<PlayerCharacterController>("Start", typeof(Patches), nameof(Patches.CharacterStart));
            ModHelper.HarmonyHelper.AddPostfix<GhostGrabController>("OnStartLiftPlayer", typeof(Patches), nameof(Patches.GhostLiftedPlayer));
            ModHelper.HarmonyHelper.AddPostfix<PlayerScreamingController>("Awake", typeof(Patches), nameof(Patches.NPCPlayerAwake));
            ModHelper.HarmonyHelper.AddPostfix<PlayerCloneController>("Start", typeof(Patches), nameof(Patches.EyeCloneStart));
            ModHelper.HarmonyHelper.AddPostfix<EyeMirrorController>("Start", typeof(Patches), nameof(Patches.EyeMirrorStart));
            ModHelper.HarmonyHelper.AddPostfix<ChertDialogueSwapper>("SelectMood", typeof(Patches), nameof(Patches.ChertDialogueSwapped));
            /*
            ModHelper.HarmonyHelper.AddPrefix<PlayerCharacterController>(
                "IsValidGroundedHit", typeof(Patches), nameof(Patches.IsValidGroundedHit));
             */

            LoadManager.OnStartSceneLoad += (scene, loadScene) =>
            {
                stools = new List<GameObject>();

                // storyEnabled is set at the beginning of the loop and doesn't change, because DialogueTrees
                // should only be changed right at the beginning.
                switch (storyEnabled)
                {
                    case true:
                        storyEnabledNow = true;
                        break;
                    case false:
                        storyEnabledNow = false;
                        break;
                }
            };

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                //if (storyEnabledNow) ModHelper.Events.Unity.FireInNUpdates(() => SetupStory(), 60);
            };

            ModHelper.Console.WriteLine($"{nameof(SmolHatchling)} is ready to go!", MessageType.Success);
        }

        private void FixedUpdate()
        {
            if (playerScale != targetScale) LerpSize();
        }

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            autoRadius = config.GetSettingsValue<bool>("Auto Radius");
            height = config.GetSettingsValue<float>("Height (Default 1)");
            radius = config.GetSettingsValue<float>("Radius (Default 1)");
            pitchChangeEnabled = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
            storyEnabled = config.GetSettingsValue<bool>("Enable Story (Requires Reload!)");
            Setup();
        }

        public void Setup()
        {
            // Make sure that the scene is the SS or Eye
            if (WrongScene()) return;
            // Cancel otherwise

            if (autoRadius) targetScale = new Vector3(Mathf.Sqrt(height), height, Mathf.Sqrt(height));
            else targetScale = new Vector3(radius, height, radius);

            playerBody = FindObjectOfType<PlayerBody>();
            playerCollider = playerBody.GetComponent<CapsuleCollider>();
            detectorCollider = playerBody.transform.Find("PlayerDetector").GetComponent<CapsuleCollider>();
            detectorShape = playerBody.GetComponentInChildren<CapsuleShape>();
            playerModel = playerBody.transform.Find("Traveller_HEA_Player_v2").gameObject;
            playerThruster = playerBody.transform.Find("PlayerVFX").gameObject;
            playerMarshmallowStick = playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root").gameObject;
            cameraController = FindObjectOfType<PlayerCameraController>();
            animController = FindObjectOfType<PlayerAnimController>();
            PlayerAudioController audioController = FindObjectOfType<PlayerAudioController>();
            PlayerBreathingAudio breathingAudio = FindObjectOfType<PlayerBreathingAudio>();

            playerAudio = new List<OWAudioSource>()
            {
                audioController._oneShotSleepingAtCampfireSource,
                audioController._oneShotSource,
                breathingAudio._asphyxiationSource,
                breathingAudio._breathingLowOxygenSource,
                breathingAudio._breathingSource,
                breathingAudio._drowningSource
            };

            switch (LoadManager.s_currentScene)
            {
                // If in solar system, set these ship vars
                case OWScene.SolarSystem:
                    cockpitController = FindObjectOfType<ShipCockpitController>();
                    logController = FindObjectOfType<ShipLogController>();
                    break;
                // If at eye, set clone and mirror vars
                case OWScene.EyeOfTheUniverse:
                    var cloneControllers = Resources.FindObjectsOfTypeAll<PlayerCloneController>();
                    cloneController = cloneControllers[0];
                    var mirrorControllers = Resources.FindObjectsOfTypeAll<EyeMirrorController>();
                    mirrorController = mirrorControllers[0];
                    break;
            }

            emberTwin = GameObject.Find("CaveTwin_Body");
            ashTwin = GameObject.Find("TowerTwin_Body");
            timberHearth = GameObject.Find("TimberHearth_Body");
            attlerock = GameObject.Find("Moon_Body");
            brittleHollow = GameObject.Find("BrittleHollow_Body");
            hollowsLantern = GameObject.Find("VolcanicMoon_Body");
            giantsDeep = GameObject.Find("GiantsDeep_Body");
            darkBramble = GameObject.Find("DarkBramble_Body");
            quantumMoon = GameObject.Find("QuantumMoon_Body");
            stranger = GameObject.Find("RingWorld_Body");
            dreamWorld = GameObject.Find("DreamWorld_Body");

            hearthTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_HEA_VillagePlanks_mat").FirstOrDefault();
            nomaiTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_NOM_HexagonTile_mat").FirstOrDefault();
            quantumTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Terrain_QM_CenterArch_mat").FirstOrDefault();
            strangerTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_IP_Mangrove_Wood_mat").FirstOrDefault();
            dreamTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_DW_Mangrove_Wood_mat").FirstOrDefault();
            simTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Terrain_IP_DreamGridVP_mat").FirstOrDefault();
        }

        public void SnapSize()
        {
            playerScale = targetScale;
            UpdatePlayerScale();
        }

        public void LerpSize()
        {
            playerScale = Vector3.Lerp(playerScale, targetScale, 0.125f);
            UpdatePlayerScale();
        }

        public void UpdatePlayerScale()
        {
            // Required temp vars
            float height = 2 * playerScale.y;
            float radius = Mathf.Min(playerScale.z / 2, height / 2);
            Vector3 center = new Vector3(0, playerScale.y - 1, 0);

            // Change collider height and radius, and move colliders down so they touch the ground
            playerCollider.height = detectorCollider.height = detectorShape.height = height;
            playerCollider.radius = detectorCollider.radius = detectorShape.radius = radius;
            playerCollider.center = detectorCollider.center = detectorShape.center = playerBody._centerOfMass = playerCollider.center = detectorCollider.center = playerBody._activeRigidbody.centerOfMass = center;

            // Change playermodel size and animation speed
            playerModel.transform.localScale = playerScale / 10;
            playerModel.transform.localPosition = new Vector3(0, -1.03f, -0.2f * playerScale.z);
            playerThruster.transform.localScale = playerScale;
            playerThruster.transform.localPosition = new Vector3(0, -1 + playerScale.y, 0);
            animSpeed = Mathf.Pow(targetScale.z, -1);
            if (!hikersModEnabled) animController._animator.speed = animSpeed;

            // Move camera and marshmallow stick root down to match new player height
            cameraController._origLocalPosition = new Vector3(0f, -1 + 1.8496f * playerScale.y, 0.15f * playerScale.z);
            cameraController.transform.localPosition = cameraController._origLocalPosition;
            playerMarshmallowStick.transform.localPosition = new Vector3(0.25f, -1.8496f + 1.8496f * playerScale.y, 0.08f - 0.15f + 0.15f * playerScale.z);

            // Change size of Ash Twin Project player clone if it exists
            if (npcPlayer != null) npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale = playerScale / 10;

            //UpdateStoolSize();

            // Change pitch if enabled
            switch (pitchChangeEnabled)
            {
                case true:
                    float pitch = 0.5f * Mathf.Pow(playerScale.y, -1) + 0.5f;
                    foreach (OWAudioSource audio in playerAudio) audio.pitch = pitch;
                    break;

                case false:
                    foreach (OWAudioSource audio in playerAudio) audio.pitch = 1;
                    break;
            }

            switch (LoadManager.s_currentScene)
            {
                // Only do these in Solar System
                case OWScene.SolarSystem:
                    cockpitController._origAttachPointLocalPos = new Vector3(0, 2.1849f - 1.8496f * playerScale.y, 4.2307f + 0.15f - 0.15f * playerScale.z);
                    logController._attachPoint._attachOffset = new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
                    break;
                // Only do these at the Eye
                case OWScene.EyeOfTheUniverse:
                    cloneController._playerVisuals.transform.localScale = playerScale / 10;
                    cloneController._signal._owAudioSource.pitch = playerAudio[0].pitch;
                    mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
                    break;
            }
        }

        // SetupStory()

        // PlaceObject()

        // PlaceObject()

        // UpdateStoolSize()

        // ChangeDialogueTree()

        // AddTranslations()
    }

    public static class Patches
    {
        public static void CharacterStart()
        {
            SmolHatchling.Instance.Setup();
            SmolHatchling.Instance.SnapSize();
        }

        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 playerScale = SmolHatchling.Instance.playerScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset = new Vector3(0, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
        }

        public static void NPCPlayerAwake(PlayerScreamingController __instance)
        {
            SmolHatchling.Instance.npcPlayer = __instance.gameObject;
        }

        public static void EyeCloneStart(PlayerCloneController __instance)
        {
            Vector3 playerScale = SmolHatchling.Instance.playerScale;
            float pitch;
            __instance._playerVisuals.transform.localScale = playerScale / 10;
            if (SmolHatchling.Instance.pitchChangeEnabled) pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
            else pitch = 1;
            __instance._signal._owAudioSource.pitch = pitch;
        }

        public static void EyeMirrorStart(EyeMirrorController __instance)
        {
            Vector3 playerScale = SmolHatchling.Instance.playerScale;
            __instance._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
        }

        public static void ChertDialogueSwapped()
        {
            //if (SmolHatchling.Instance.storyEnabledNow) SmolHatchling.Instance.ChangeDialogueTree("Chert");
        }

        public static void StoolTransformed(StoolItem __instance)
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

        public static void PlayerToAttachPoint(PlayerAttachPoint __instance)
        {
            if (__instance.gameObject.GetComponentInChildren<StoolSocket>())
            {
                Vector3 playerScale = SmolHatchling.Instance.playerScale;
                if (__instance.gameObject.GetComponentInChildren<StoolSocket>()._socketedItem != null) __instance.SetAttachOffset(new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z));
                else __instance.SetAttachOffset(new Vector3(0, 0, 0));

            }
        }

        public static bool IsValidGroundedHit(ref bool __result, RaycastHit hit)
        {
            // This patch is disabled because while it fixes the problem of the player becoming ungrounded
            // when next to a wall, it causes a bigger problem of allowing the player to be grounded ON a wall.
            __result = hit.distance > -(0.5f - SmolHatchling.Instance.playerScale.z * 0.5f) && hit.rigidbody != Locator.GetPlayerController()._owRigidbody.GetRigidbody();
            return false;
        }
    }

    public class StoolItem : OWItem
    {
        public override string GetDisplayName()
        {
            return "Stool";
        }

        public override void Awake()
        {
            _type = (ItemType)256;
            base.Awake();
        }
    }

    public class StoolSocket : OWItemSocket
    {
        public override bool AcceptsItem(OWItem item)
        {
            return item.GetComponent<StoolItem>() != null;
        }

        public override void Awake()
        {
            _acceptableType = (ItemType)256;
            _socketTransform = transform;
            gameObject.layer = 21;

            if (_sector == null)
            {
                _sector = GetComponentInParent<Sector>();
            }
            if (_sector == null)
            {
                //Debug.LogError("Could not find Sector in OWItemSocket parents", this);
                Debug.Break();
            }
            if (_socketTransform.childCount > 0)
            {
                _socketedItem = _socketTransform.GetComponentInChildren<OWItem>();
            }
        }
    }

    public class SmolHatchlingAPI
    {
        // This API was added for support with HikersMod, since HikersMod needs to know the scale and animspeed of the
        // hatchling.
        public Vector3 GetPlayerScale()
        {
            return SmolHatchling.Instance.playerScale;
        }

        public float GetAnimSpeed()
        {
            return SmolHatchling.Instance.animSpeed;
        }

        public void SetHikersModEnabled()
        {
            SmolHatchling.Instance.hikersModEnabled = true;
        }
    }
}