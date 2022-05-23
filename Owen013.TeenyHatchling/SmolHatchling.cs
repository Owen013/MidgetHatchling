using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using OWML.Common;
using OWML.ModHelper;

namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        public Vector3 GetPlayerScale()
        {
            return SmolHatchling.playerScale;
        }

        public float GetAnimSpeed()
        {
            return SmolHatchling.animSpeed;
        }
    }

    public class SmolHatchling : ModBehaviour
    {
        // Config vars
        float height, radius;
        public static float runSpeed, walkSpeed, acceleration, jumpPower, animSpeed;
        public bool pitchChangeEnabled, storyEnabled, storyEnabledNow;

        // Mod vars
        public static SmolHatchling Instance;
        public static Vector3 playerScale;
        PlayerBody playerBody;
        PlayerCameraController cameraController;
        PlayerAnimController animController;
        CapsuleCollider playerCollider, detectorCollider;
        CapsuleShape detectorShape;
        ShipCockpitController cockpitController;
        ShipLogController logController;
        PlayerCloneController cloneController;
        EyeMirrorController mirrorController;
        public GameObject playerModel, playerThruster, playerMarshmallowStick, npcPlayer;
        AssetBundle textAssets;
        List<OWAudioSource> playerAudio;

        public override object GetApi()
        {
            return new SmolHatchlingAPI();
        }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Load assets
            textAssets = ModHelper.Assets.LoadBundle("Assets/textassets");

            // Add patches
            ModHelper.HarmonyHelper.AddPostfix<PlayerCharacterController>(
                "Start", typeof(Patches), nameof(Patches.CharacterStart));
            ModHelper.HarmonyHelper.AddPostfix<GhostGrabController>(
                "OnStartLiftPlayer", typeof(Patches), nameof(Patches.GhostLiftedPlayer));
            ModHelper.HarmonyHelper.AddPostfix<PlayerScreamingController>(
                "Awake", typeof(Patches), nameof(Patches.NPCPlayerAwake));
            ModHelper.HarmonyHelper.AddPostfix<PlayerCloneController>(
                "Start", typeof(Patches), nameof(Patches.EyeCloneStart));
            ModHelper.HarmonyHelper.AddPostfix<EyeMirrorController>(
                "Start", typeof(Patches), nameof(Patches.EyeMirrorStart));
            ModHelper.HarmonyHelper.AddPostfix<ChertDialogueSwapper>(
                "SelectMood", typeof(Patches), nameof(Patches.ChertDialogueSwapped));

            LoadManager.OnStartSceneLoad += (scene, loadScene) =>
            {
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
                if (storyEnabledNow) ModHelper.Events.Unity.FireInNUpdates(() => ChangeDialogueTree(""), 60);
            };

            ModHelper.Console.WriteLine($"{nameof(SmolHatchling)} is ready to go!", MessageType.Success);
        }

        // Runs whenever the config is changed
        public override void Configure(IModConfig config)
        {
            base.Configure(config);
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

            playerScale = new Vector3(radius, height, radius);
            animSpeed = Mathf.Pow(playerScale.z, -1);

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

            ChangeSize();
        }

        public void ChangeSize()
        {
            // Required temp vars
            float height = 2 * playerScale.y;
            float radius = Mathf.Min(playerCollider.height / 2, (playerScale.x + playerScale.z) / 2 * 0.5f);
            Vector3 center = new Vector3(0, playerScale.y - 1, 0);

            // Change collider height and radius, and move colliders down so they touch the ground
            playerCollider.height = detectorCollider.height = detectorShape.height = height;
            playerCollider.radius = detectorCollider.radius = detectorShape.radius = radius;
            playerCollider.center = detectorCollider.center = detectorShape.center = playerBody._centerOfMass
                = playerCollider.center = detectorCollider.center = playerBody._activeRigidbody.centerOfMass
                = center;

            // Change playermodel size and animation speed
            playerModel.transform.localScale = playerScale / 10;
            playerModel.transform.localPosition = new Vector3(0, -1.03f, -0.2f * playerScale.z);
            playerThruster.transform.localScale = playerScale;
            playerThruster.transform.localPosition = new Vector3(0, -1 + playerScale.y, 0);
            animController._animator.speed = animSpeed;

            // Move camera and marshmallow stick root down to match new player height
            cameraController._origLocalPosition
                = new Vector3(0f, -1 + 1.8496f * playerScale.y, 0.15f * playerScale.z);
            playerMarshmallowStick.transform.localPosition
                = new Vector3(0.25f, -1.8496f + 1.8496f * playerScale.y, 0.08f - 0.15f + 0.15f * playerScale.z);

            // Change size of Ash Twin Project player clone if it exists
            if (npcPlayer != null) npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale
                    = playerScale / 10;

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
                    cockpitController._origAttachPointLocalPos =
                    new Vector3(0, 2.1849f - 1.8496f * playerScale.y, 4.2307f + 0.15f - 0.15f * playerScale.z);
                    logController._attachPoint._attachOffset =
                        new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
                    break;
                // Only do these at the Eye
                case OWScene.EyeOfTheUniverse:
                    cloneController._playerVisuals.transform.localScale = playerScale / 10;
                    cloneController._signal._owAudioSource.pitch = playerAudio[0].pitch;
                    mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
                    break;
            }

            ModHelper.Console.WriteLine($"Player Scale: ({playerModel.transform.localScale * 10}),");
        }

        public void ChangeDialogueTree(string dialogueName)
        {
            var dialogueTrees = FindObjectsOfType<CharacterDialogueTree>();
            CharacterDialogueTree dialogueTree;
            List<string> changedCharacters = new List<string>();
            for (var i = 0; i < dialogueTrees.Length; ++i)
            {
                dialogueTree = dialogueTrees[i];
                string assetName = dialogueTree._xmlCharacterDialogueAsset.name;
                TextAsset textAsset = textAssets.LoadAsset<TextAsset>(assetName);
                if (!textAssets.Contains(assetName) || dialogueName != "" && dialogueName != dialogueTree._characterName) continue;
                dialogueTree.SetTextXml(textAsset);
                AddTranslations(textAsset.ToString());
                dialogueTree.OnDialogueConditionsReset();
                changedCharacters.Add(dialogueTree._characterName);
            }
            string changedList;
            if (changedCharacters.Count == 0)
                ModHelper.Console.WriteLine("No dialogues replaced");
            else
            {
                changedList = string.Join(", ", changedCharacters);
                ModHelper.Console.WriteLine(string.Format("Dialogue replaced for: {0}.", changedList), MessageType.Success);
            }
        }

        private void AddTranslations(string textAsset)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(textAsset);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("DialogueTree");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("DialogueNode");
            string NameField = xmlNode.SelectSingleNode("NameField").InnerText;
            var translationTable = TextTranslation.Get().m_table.theTable;
            translationTable[NameField] = NameField;
            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                var name = xmlNode2.SelectSingleNode("Name").InnerText;

                XmlNodeList xmlText = xmlNode2.SelectNodes("Dialogue/Page");
                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[name + pageData.InnerText] = pageData.InnerText;
                }
                xmlText = xmlNode2.SelectNodes("DialogueOptionsList/DialogueOption/Text");
                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[NameField + name + pageData.InnerText] = pageData.InnerText;

                }
            }
        }

        private bool WrongScene()
        {
            OWScene scene = LoadManager.s_currentScene;
            return !(scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse);
        }
    }

    public static class Patches
    {
        public static void CharacterStart()
        {
            SmolHatchling.Instance.Setup();
        }

        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset =
                new Vector3(0, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
        }

        public static void NPCPlayerAwake(PlayerScreamingController __instance)
        {
            SmolHatchling.Instance.npcPlayer = __instance.gameObject;
        }

        public static void EyeCloneStart(PlayerCloneController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            float pitch;
            __instance._playerVisuals.transform.localScale = playerScale / 10;
            if (SmolHatchling.Instance.pitchChangeEnabled) pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
            else pitch = 1;
            __instance._signal._owAudioSource.pitch = pitch;
        }

        public static void EyeMirrorStart(EyeMirrorController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            __instance._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
        }

        public static void ChertDialogueSwapped()
        {
            if (SmolHatchling.Instance.storyEnabledNow) SmolHatchling.Instance.ChangeDialogueTree("Chert");
        }
    }
}