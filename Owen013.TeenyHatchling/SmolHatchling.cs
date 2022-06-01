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
        public bool autoRadius, pitchChangeEnabled, storyEnabled, storyEnabledNow;

        // Mod vars
        public static SmolHatchling Instance;
        public Vector3 playerScale;
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

        private GameObject NewStool()
        {
            GameObject stool = Instantiate(models.LoadAsset<GameObject>("SH_Stool"));
            GameObject real = stool.transform.Find("Real").gameObject;
            GameObject sim = stool.transform.Find("Simulation").gameObject;
            stool.name = "SH_Stool";
            stools.Add(stool);
            stool.AddComponent<StoolItem>();
            real.GetComponent<MeshRenderer>().material = hearthTexture;
            sim.GetComponent<MeshRenderer>().material = simTexture;
            sim.layer = 28;
            UpdateStoolSize();

            return stool;
        }

        private GameObject NewStoolSocket()
        {
            // Add model rocket stool socket
            GameObject socketObject = new GameObject();
            StoolSocket socketComponent = socketObject.AddComponent<StoolSocket>();
            SphereCollider sphereCollider = socketObject.AddComponent<SphereCollider>();
            OWCollider oWCollider = socketObject.AddComponent<OWCollider>();
            socketObject.name = "SH_StoolSocket";
            socketComponent._socketTransform = socketObject.transform;
            sphereCollider.center = new Vector3(0, 0.5f, 0);
            sphereCollider.radius = 0.75f;
            oWCollider.enabled = false;

            return socketObject;
        }

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
                if (storyEnabledNow) ModHelper.Events.Unity.FireInNUpdates(() => SetupStory(), 60);
            };

            ModHelper.Console.WriteLine($"{nameof(SmolHatchling)} is ready to go!", MessageType.Success);
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

            if (autoRadius) playerScale = new Vector3(Mathf.Sqrt(height), height, Mathf.Sqrt(height));
            else playerScale = new Vector3(radius, height, radius);
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

            ChangeSize();
        }

        public void ChangeSize()
        {
            // Required temp vars
            float height = 2 * playerScale.y;
            float radius = Mathf.Min(playerScale.z / 2, height / 2);
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
            cameraController.transform.localPosition = cameraController._origLocalPosition;
            playerMarshmallowStick.transform.localPosition
                = new Vector3(0.25f, -1.8496f + 1.8496f * playerScale.y, 0.08f - 0.15f + 0.15f * playerScale.z);

            // Change size of Ash Twin Project player clone if it exists
            if (npcPlayer != null) npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale
                = playerScale / 10;

            UpdateStoolSize();

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

        private void SetupStory()
        {
            // Add stool patches
            ModHelper.HarmonyHelper.AddPostfix<StoolItem>(
                "MoveAndChildToTransform", typeof(Patches), nameof(Patches.StoolTransformed));
            ModHelper.HarmonyHelper.AddPostfix<StoolItem>(
                "DropItem", typeof(Patches), nameof(Patches.StoolTransformed));
            ModHelper.HarmonyHelper.AddPostfix<PlayerAttachPoint>(
                "AttachPlayer", typeof(Patches), nameof(Patches.PlayerToAttachPoint));

            // Change all dialogue trees
            ChangeDialogueTree("");

            switch(LoadManager.s_currentScene)
            {
                case OWScene.SolarSystem:

                    // Place page with launch codes
                    GameObject gameObject = Instantiate(GameObject.Find("DeepFieldNotes_2"));
                    CharacterDialogueTree dialogueTree = gameObject.GetComponentInChildren<CharacterDialogueTree>();
                    InteractVolume interactVolume = gameObject.GetComponentInChildren<InteractVolume>();
                    GameObject pageModel = gameObject.transform.Find("plaque_paper_1 (1)").gameObject;
                    gameObject.name = "SH_LaunchCodesNote";
                    gameObject.transform.parent = GameObject.Find("TimberHearth_Body").transform;
                    gameObject.transform.localPosition = new Vector3(-54.6006f, 5.6734f, 218.6826f);
                    gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    interactVolume.transform.localPosition = new Vector3(0.2f, 0.9f, 0.2f);
                    interactVolume.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                    pageModel.transform.localPosition = new Vector3(0, 0, 0);
                    pageModel.transform.localRotation = Quaternion.Euler(60.3462f, 346.2182f, 0);
                    Destroy(gameObject.transform.Find("plaque_paper_1 (2)").gameObject);
                    Destroy(gameObject.transform.Find("plaque_paper_1 (3)").gameObject);
                    foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
                        renderer.enabled = true;
                    dialogueTree._attentionPoint = pageModel.transform;
                    dialogueTree._characterName = "SH_LaunchCodesNote";
                    dialogueTree._xmlCharacterDialogueAsset = textAssets.LoadAsset<TextAsset>("SH_LaunchCodesNote");
                    ChangeDialogueTree("SH_LaunchCodesNote");
                    interactVolume.enabled = true;
                    interactVolume.EnableInteraction();

                    // Add model rocket stool socket
                    GameObject stoolSocket = NewStoolSocket();
                    stoolSocket.transform.parent = GameObject.Find("ModelRocketStation_AttachPoint").transform;
                    stoolSocket.transform.localPosition = new Vector3(0.0054f, -1.0032f, -0.0627f);
                    stoolSocket.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    // Place a crap ton of stools
                    PlaceObject(NewStool(), timberHearth, new Vector3(22.62274f, -13.01972f, 199.5054f)); // Model rocket
                    PlaceObject(NewStool(), quantumMoon, new Vector3(-2.4616f, -68.6764f, 6.2243f)); // Solanum
                    PlaceObject(NewStool(), stranger, new Vector3(-70.0019f, 13.0961f, -286.3849f)); // River Lowlands slide player
                    PlaceObject(NewStool(), stranger, new Vector3(-273.9175f, -54.951f, 58.9185f)); // Cinder Isles slide player
                    PlaceObject(NewStool(), stranger, new Vector3(120.2041f, -70.7477f, 212.4686f)); // Hidden Gorge Slide player
                    PlaceObject(NewStool(), stranger, new Vector3(180.4737f, 136.3092f, -153.2241f)); // Resevoir code wheel
                    PlaceObject(NewStool(), stranger, new Vector3(231.4058f, 121.4653f, -40.2097f)); // Reservoir slide player
                    PlaceObject(NewStool(), dreamWorld, new Vector3(58.0762f, 1.0248f, -677.8818f)); // Shrouded Woods tunnel door projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(58.4984f, 1.1f, -746.1944f)); // Shrouded Woods raft projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(59.3381f, 8.7338f, -607.8622f)); // Shrouded Woods bridge projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(51.2455f, 13.7052f, -150.3283f)); // Starlit Cove house projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(79.0349f, 25.0568f, -302.9778f)); // Starlit Cove lights projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(14.8133f, 92.0101f, 269.9755f)); // Endless Canyon starting bridge
                    PlaceObject(NewStool(), dreamWorld, new Vector3(24.3027f, 93.8709f, 207.3266f)); // Endless Canyon lights projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(66.2891f, -290.2409f, 632.512f)); // Subterr Lake 1
                    PlaceObject(NewStool(), dreamWorld, new Vector3(15.81989f, -290.9268f, 681.0381f)); // Subterr Lake 2
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-2.1641f - 294.6368f, 602.5558f)); // Subterr Lake 3
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-56.9233f, -290.9005f, 634.5746f)); // Subterr Lake 4
                    // Old stools (need changing)
                    PlaceObject(NewStool(), dreamWorld, new Vector3(59.68134f, 1.12686f, 694.9167f));
                    PlaceObject(NewStool(), dreamWorld, new Vector3(48.45544f, 13.68467f, -153.7672f));
                    PlaceObject(NewStool(), dreamWorld, new Vector3(77.29767f, 25.0746f, -300.6379f));
                    PlaceObject(NewStool(), dreamWorld, new Vector3(62.20693f, -290.4292f, 631.58f));
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-2.184837f, -294.6989f, 608.2343f));
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-66.19071f, -290.1708f, 630.8723f));
                    break;
            }
            
        }

        private void PlaceObject(GameObject gameObject, GameObject parentBody, Vector3 position)
        {
            Vector3 up;
            gameObject.transform.parent = parentBody.transform;
            gameObject.transform.localPosition = position;

            switch (parentBody.name)
            {
            // If this is a regular planet, then set 
            default:
                    up = parentBody.transform.InverseTransformPoint(gameObject.transform.position).normalized;
                    gameObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, up);
                    break;

                case "RingWorld_Body":
                    up = -FindObjectOfType<RingWorldForceVolume>().GetFieldDirection(gameObject.transform.position);
                    gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                    break;

                case "DreamWorld_Body":
                    gameObject.transform.localRotation = parentBody.transform.localRotation;
                    break;
            }

            switch (gameObject.name)
            {
                default:
                    break;

                case "SH_Stool":
                    MeshRenderer stoolRenderer = gameObject.transform.Find("Real").GetComponent<MeshRenderer>();
                    switch (parentBody.name)
                    {
                        default:
                            stoolRenderer.material = nomaiTexture;
                            break;
                        
                        case "TimberHearth_Body":
                            stoolRenderer.material = hearthTexture;
                            break;

                        case "QuantumMoon_Body":
                            stoolRenderer.material = quantumTexture;
                            break;

                        case "RingWorld_Body":
                            stoolRenderer.material = strangerTexture;
                            break;

                        case "DreamWorld_Body":
                            stoolRenderer.material = dreamTexture;
                            break;
                    }
                    break;
            }
        }

        private void UpdateStoolSize()
        {
            foreach (GameObject stool in stools)
            {
                GameObject realObject = stool.transform.Find("Real").gameObject;
                GameObject dreamObject = stool.transform.Find("Simulation").gameObject;
                BoxCollider collider = stool.GetComponent<BoxCollider>();
                realObject.transform.localScale = dreamObject.transform.localScale = new Vector3(0.5f, -playerScale.y + 1, 0.5f);
                collider.size = new Vector3(0.875f, 1.8f * -playerScale.y + 1.8f, 0.875f);
                collider.center = new Vector3(0, 0.5f * collider.size.y, 0);
                stool.SetActive(playerScale.y < 1);
            }
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
    }

    public static class Patches
    {
        public static void CharacterStart()
        {
            SmolHatchling.Instance.Setup();
        }

        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 playerScale = SmolHatchling.Instance.playerScale;
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
            if (SmolHatchling.Instance.storyEnabledNow) SmolHatchling.Instance.ChangeDialogueTree("Chert");
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
                if (__instance.gameObject.GetComponentInChildren<StoolSocket>()._socketedItem != null)
                    __instance.SetAttachOffset(new Vector3(0, -SmolHatchling.Instance.playerScale.y * 1.5f + 1.5f, 0));
                else __instance.SetAttachOffset(new Vector3(0, 0, 0));

            }
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
                Debug.LogError("Could not find Sector in OWItemSocket parents", this);
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
    }
}

/*
 *      Oh, hi there.
 *      
 */