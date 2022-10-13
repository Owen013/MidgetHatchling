using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;

namespace SmolHatchling
{
    public class SmolHatchling : ModBehaviour
    {
        // Config vars
        public float height, radius, animSpeed;
        public bool autoRadius, pitchChangeEnabled, storyEnabled, stoolsEnabled, hikersModEnabled;
        public string colliderMode;

        // Mod vars
        public static SmolHatchling Instance;
        public Vector3 targetScale, playerScale, colliderScale;
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
        private List<OWAudioSource> playerAudio;

        public override object GetApi()
        {
            return new SmolHatchlingAPI();
        }

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            height = config.GetSettingsValue<float>("Height (Default 1)");
            radius = config.GetSettingsValue<float>("Radius (Default 1)");
            autoRadius = config.GetSettingsValue<bool>("Auto-Radius");
            colliderMode = config.GetSettingsValue<string>("Resize Collider");
            pitchChangeEnabled = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
            //stoolsEnabled = config.GetSettingsValue<bool>("Enable Stools (Requires Reload!)");
            //storyEnabled = config.GetSettingsValue<bool>("Enable Story (Requires Reload!)");

            Setup();
        }

        public void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(SmolHatchlingPatches));
        }

        public void Start()
        {
            //gameObject.AddComponent<StoolController>();
            //gameObject.AddComponent<StoryController>();
            ModHelper.Console.WriteLine($"Smol Hatchling is ready to go!", MessageType.Success);
        }

        public void FixedUpdate()
        {
            if (playerScale != targetScale) LerpSize();
            if (targetScale != null && colliderMode != null && playerCollider != null) UpdateColliderScale();
        }

        public void Setup()
        {
            // Make sure that the scene is the SS or Eye
            OWScene scene = LoadManager.s_currentScene;
            if (scene != OWScene.SolarSystem && scene != OWScene.EyeOfTheUniverse) return;

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
            if (npcPlayer != null)
            {
                npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale = playerScale / 10;
                npcPlayer.transform.Find("NPC_Player_FocalPoint").localPosition = new Vector3(-0.093f, 0.991f * targetScale.y, 0.102f);
            }

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

        public void UpdateColliderScale()
        {
            // Change collider height and radius, and move colliders down so they touch the ground
            if (colliderMode == "Height & Radius") colliderScale = targetScale;
            else if (colliderMode == "Height Only") colliderScale = new Vector3(1, targetScale.y, 1);
            else colliderScale = Vector3.one;
            float height = 2 * colliderScale.y;
            float radius = Mathf.Min(colliderScale.z / 2, height / 2);
            Vector3 center = new Vector3(0, colliderScale.y - 1, 0);
            playerCollider.height = detectorCollider.height = detectorShape.height = height;
            playerCollider.radius = detectorCollider.radius = detectorShape.radius = radius;
            playerCollider.center = detectorCollider.center = detectorShape.center = playerBody._centerOfMass = playerCollider.center = detectorCollider.center = playerBody._activeRigidbody.centerOfMass = center;
        }

        public void PrintLog(string text)
        {
            ModHelper.Console.WriteLine(text);
        }

        public void PrintLog(string text, MessageType messageType)
        {
            ModHelper.Console.WriteLine(text, messageType);
        }
    }
}