using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmolHatchling
{
    public class StoolController : MonoBehaviour
    {
        public static StoolController Instance;
        public AssetBundle _models;
        public List<GameObject> _stools;
        public Material _hearthTexture, _nomaiTexture, _quantumTexture, _strangerTexture, _dreamTexture, _simTexture;
        public string _lastHeldStool;
        public float _stoolHeight;

        public void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(StoolController));
        }

        public void Start()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (_models == null) _models = ModController.s_instance.ModHelper.Assets.LoadBundle("Assets/sh_models");
                _stools = new List<GameObject>();
                if (!ModController.s_instance._enableStools) return;
                if (loadScene == OWScene.SolarSystem) OnSolarSystemLoaded();
            };
        }

        public void OnSolarSystemLoaded()
        {
            GameObject emberTwin = GameObject.Find("CaveTwin_Body");
            GameObject ashTwin = GameObject.Find("TowerTwin_Body");
            GameObject timberHearth = GameObject.Find("TimberHearth_Body");
            GameObject attlerock = GameObject.Find("Moon_Body");
            GameObject brittleHollow = GameObject.Find("BrittleHollow_Body");
            GameObject hollowsLantern = GameObject.Find("VolcanicMoon_Body");
            GameObject giantsDeep = GameObject.Find("GiantsDeep_Body");
            GameObject darkBramble = GameObject.Find("DarkBramble_Body");
            GameObject quantumMoon = GameObject.Find("QuantumMoon_Body");
            GameObject stranger = GameObject.Find("RingWorld_Body");
            GameObject dreamWorld = GameObject.Find("DreamWorld_Body");

            _hearthTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_HEA_VillagePlanks_mat").FirstOrDefault();
            _nomaiTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_NOM_HexagonTile_mat").FirstOrDefault();
            _quantumTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Terrain_QM_CenterArch_mat").FirstOrDefault();
            _strangerTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_IP_Mangrove_Wood_mat").FirstOrDefault();
            _dreamTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_DW_Mangrove_Wood_mat").FirstOrDefault();
            _simTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Terrain_IP_DreamGridVP_mat").FirstOrDefault();

            switch (LoadManager.s_currentScene)
            {
                case OWScene.SolarSystem:

                    // Place stool sockets
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("ModelRocketStation_AttachPoint"), // Village model rocket
                        new Vector3(0.0054f, -1.0032f, -0.0627f),
                        Quaternion.Euler(0, 0, 0));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("TutorialCamera_Base/InteractZone"), // Village ghost matter camera
                        new Vector3(-0.0237f, -0.9278f, -0.5853f),
                        Quaternion.Euler(0, 0, 0));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("TutorialProbeLauncher_Base/InteractZone"), // Village scout launcher
                        new Vector3(-0.0496f, -1.0474f, -0.3875f),
                        Quaternion.Euler(0, 0, 0));
                    PlaceObject(NewStoolSocket(),
                        stranger, // River Lowlands slide player
                        new Vector3(-68.3667f, 12.9691f, -286.7644f),
                        Quaternion.Euler(55.6455f, 283.0649f, 269.9998f));
                    PlaceObject(NewStoolSocket(),
                        stranger, // Cinder Isles slide player
                        new Vector3(-276.8384f, -48.6267f, 59.6053f),
                        Quaternion.Euler(302.8642f, 12.068f, 270f));
                    PlaceObject(NewStoolSocket(),
                        stranger, // Hidden Gorge slide player
                        new Vector3(124.4978f, -70.4448f, 209.9373f),
                        Quaternion.Euler(11.7714f, 300.5211f, 90f));
                    PlaceObject(NewStoolSocket(),
                        stranger, // Resevoir slide player
                        new Vector3(230.4972f, 118.5905f, -43.9123f),
                        Quaternion.Euler(353.3662f, 13.5101f, 89.5582f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Tunnel/Prefab_IP_DreamObjectProjector (2)"), // Shrouded Woods tunnel door projector
                        new Vector3(0.0061f, 0.0937f, -1.4142f),
                        Quaternion.Euler(359.8972f, 357.6237f, 359.9063f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Interactibles_DreamZone_1/RaftDock/Prefab_IP_DreamRaftProjector"), // Shrouded Woods raft projector
                        new Vector3(-0.0815f, -0.04f, -1.2619f),
                        Quaternion.Euler(0, 353.6009f, 0));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("ProjectedBridge/Prefab_IP_DreamObjectProjector (1)"), // Shrouded Woods bridge projector
                        new Vector3(0.02f, -0.0485f, -1.314f),
                        Quaternion.Euler(358.1544f, 354.8182f, 1.8163f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("LowerLevel/RaftDockProjector/Prefab_IP_DreamObjectProjector"), // Starlit Cove house projector
                        new Vector3(-0.0109f, 0.0184f, -1.2981f),
                        Quaternion.Euler(357.7859f, 358.1634f, 359.8753f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Interactibles_DreamZone_2/Prefab_IP_DreamObjectProjector"), // Starlit Cove lights projector
                        new Vector3(-0.1233f, 0.3758f, -1.3279f),
                        Quaternion.Euler(357.8555f, 347.2135f, 0.6181f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Prefab_IP_DreamObjectProjector_Bridge"), // Endless Canyon starting bridge projector
                        new Vector3(-0.0057f, 0.189f, -1.3892f),
                        Quaternion.Euler(359.5933f, 355.1652f, 1.915f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Interactibles_Hotel/Lobby/Prefab_IP_DreamObjectProjector"), // Endless Canyon indoor bridge projector
                        new Vector3(0.0076f, 0, -1.216f),
                        Quaternion.Euler(0, 349.9582f, 0));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Prefab_IP_DreamObjectProjector_Hotel"), // Endless Canyon lights projector
                        new Vector3(0.0323f, -0.0514f, -1.3253f),
                        Quaternion.Euler(0, 353.3195f, 0));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Interactibles_DreamZone_3/RaftDock (1)/Prefab_IP_DreamRaftProjector/"), // Endless Canyon Raft Projector
                        new Vector3(0.0161f, -0.0204f, -1.2457f),
                        Quaternion.Euler(0, 350, 0));
                    PlaceObject(NewStoolSocket(),
                        dreamWorld, // Subterr Lake code wheel
                        new Vector3(-1.984032f, -290.51f, 685.7135f),
                        Quaternion.Euler(0, 353.3195f, 0));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Island_A/Interactibles_Island_A/Prefab_IP_DreamObjectProjector (4)"), // Subterr Lake prison projector 1
                        new Vector3(0.0792f, 0.1458f, -1.483f),
                        Quaternion.Euler(2.219f, 344.8053f, 3.3272f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Interactibles_Island_B/Prefab_IP_DreamObjectProjector (3)"), // Subterr Lake prison projector 2
                        new Vector3(0.0884f, -0.1407f, -1.3932f),
                        Quaternion.Euler(8.5598f, 343.9718f, 2.0844f));
                    PlaceObject(NewStoolSocket(),
                        GameObject.Find("Interactibles_Island_C/Prefab_IP_DreamObjectProjector (2)"), // Subterr Lake prison projector 3
                        new Vector3(0.068f, 0.1968f, -1.3156f),
                        Quaternion.Euler(0.0902f, 348.7343f, 0.0428f));

                    // Place a crap ton of stools
                    PlaceObject(NewStool(_hearthTexture),
                        timberHearth, // Village starting campsite
                        new Vector3(32.1602f, -38.3847f, 184.5434f),
                        Quaternion.Euler(357.5007f, 184.827f, 148.018f));
                    PlaceObject(NewStool(_hearthTexture),
                        timberHearth, // Village ghost matter camera
                        new Vector3(35.189f, 49.0149f, 225.9513f),
                        Quaternion.Euler(51.7721f, 82.6791f, 70.8254f));
                    PlaceObject(NewStool(_quantumTexture),
                        quantumMoon, // Solanum
                        new Vector3(-2.4616f, -68.6764f, 6.2243f));
                    PlaceObject(NewStool(_strangerTexture),
                        stranger, // River Lowlands slide player
                        new Vector3(-70.0019f, 13.0961f, -286.3849f));
                    PlaceObject(NewStool(_strangerTexture),
                        stranger, // Cinder Isles slide player
                        new Vector3(-273.9175f, -54.951f, 58.9185f));
                    PlaceObject(NewStool(_strangerTexture),
                        stranger, // Hidden Gorge Slide player
                        new Vector3(120.2041f, -70.7477f, 212.4686f));
                    PlaceObject(NewStool(_strangerTexture),
                        stranger, // Resevoir code wheel
                        new Vector3(180.4737f, 136.3092f, -153.2241f));
                    PlaceObject(NewStool(_strangerTexture),
                        stranger, // Reservoir slide player
                        new Vector3(231.4058f, 121.4653f, -40.2097f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Shrouded Woods tunnel door projector
                        new Vector3(58.0762f, 1.0248f, -677.8818f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Shrouded Woods raft projector
                        new Vector3(59.3558f, 1.1f, -752.6471f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Shrouded Woods bridge projector
                        new Vector3(59.3381f, 8.7338f, -607.8622f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Starlit Cove house projector
                        new Vector3(51.2455f, 13.7052f, -150.3283f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Starlit Cove lights projector
                        new Vector3(79.0349f, 25.0568f, -302.9778f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Endless Canyon starting bridge
                        new Vector3(14.8133f, 92.0101f, 269.9755f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Endless Canyon indoor bridge projector
                        new Vector3(-38.8031f, 81.15f, 222.5402f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Endless Canyon lights projector
                        new Vector3(24.3027f, 93.8709f, 207.3266f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Endless Canyon raft projector
                        new Vector3(-22.0802f, 1.16f, 273.6684f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Subterr Lake 1
                        new Vector3(8.796f, -290.8733f, 681.2858f),
                        Quaternion.Euler(358.46f, 180.3598f, 359.1666f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Subterr Lake 2
                        new Vector3(66.4256f, -290.227f, 632.392f),
                        Quaternion.Euler(355.5746f, 127.8159f, 359.0247f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Subterr Lake 3
                        new Vector3(-5.7661f, -294.7887f, 603.1592f),
                        Quaternion.Euler(358.9676f, 166.8059f, 357.8715f));
                    PlaceObject(NewStool(_dreamTexture),
                        dreamWorld, // Subterr Lake 4
                        new Vector3(-69.6572f, -290.137f, 630.3542f),
                        Quaternion.Euler(359.5942f, 216.7978f, 1.1301f));
                    break;

                    // There must be a better way to do this...
            }

        }

        public GameObject NewStool(Material texture)
        {
            GameObject stool = Instantiate(_models.LoadAsset<GameObject>("SH_Stool"));
            StoolItem stoolItem = stool.AddComponent<StoolItem>();
            stool.name = $"SH_Stool {_stools.Count}";
            _stools.Add(stool);
            stoolItem._realModel.GetComponent<MeshRenderer>().material = texture;
            stoolItem._dreamModel.GetComponent<MeshRenderer>().material = _simTexture;
            stoolItem._dreamModel.layer = 28;
            UpdateStoolSize();

            return stool;
        }

        public GameObject NewStoolSocket()
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

        public void PlaceObject(GameObject gameObject, GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            if (parent == null)
            {
                ModController.s_instance.PrintLog($"Cannot place {gameObject.name} because parent is null.");
                Destroy(gameObject);
            }

            gameObject.transform.parent = parent.transform;
            gameObject.transform.localPosition = localPos;
            gameObject.transform.localRotation = localRot;
        }

        public void PlaceObject(GameObject gameObject, GameObject parent, Vector3 localPos)
        {
            if (parent == null)
            {
                ModController.s_instance.PrintLog($"Cannot place {gameObject.name} because parent is null.");
                Destroy(gameObject);
            }

            gameObject.transform.parent = parent.transform;
            gameObject.transform.localPosition = localPos;
            AutoAlignObject(gameObject);
        }

        public void AutoAlignObject(GameObject gameObject)
        {
            Vector3 up;
            AstroObject astroBody = gameObject.GetComponentInParent<AstroObject>();
            if (astroBody == null)
            {
                ModController.s_instance.PrintLog($"Cannot auto-align {gameObject.name} because it is not a descendent of an AstroBody");
                return;
            }
            switch (gameObject.GetComponentInParent<AstroObject>().name)
            {
            default:
                    up = astroBody.transform.InverseTransformPoint(gameObject.transform.position).normalized;
                    gameObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, up);
                    break;

                case "RingWorld_Body":
                    up = -FindObjectOfType<RingWorldForceVolume>().GetFieldDirection(gameObject.transform.position);
                    gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                    break;

                case "DreamWorld_Body":
                    gameObject.transform.localRotation = astroBody.transform.localRotation;
                    break;
            }
        }

        public void UpdateStoolSize()
        {
            if (_stools == null) return;
            if (ModController.s_instance._autoScaleStools) _stoolHeight = -ModController.s_instance._targetScale.y + 1;
            else _stoolHeight = ModController.s_instance._stoolHeight;
            foreach (GameObject stool in _stools)
            {
                stool.GetComponent<StoolItem>().SetHeight(_stoolHeight);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ModController), nameof(ModController.UpdatePlayerScale))]
        public static void OnUpdatePlayerScale()
        {
            Instance.UpdateStoolSize();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAttachPoint), nameof(PlayerAttachPoint.AttachPlayer))]
        public static void PlayerToAttachPoint(PlayerAttachPoint __instance)
        {
            StoolSocket socket = __instance.gameObject.GetComponentInChildren<StoolSocket>();
            if (socket != null)
            {
                StoolItem stool = socket.GetSocketedStoolItem();
                float yOffset = 0f;
                float zOffset = 0.15f - 0.15f * ModController.s_instance._targetScale.z;
                if (stool != null) yOffset = 1.8496f * stool.GetHeight();
                __instance.SetAttachOffset(new Vector3(0, yOffset, zOffset));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MemoryUplinkTrigger), nameof(MemoryUplinkTrigger.BeginUplinkSequence))]
        [HarmonyPatch(typeof(VesselWarpController), nameof(VesselWarpController.WarpVessel))]
        public static void SaveHeldStool()
        {
            StoolController stoolController = Instance;
            stoolController._lastHeldStool = null;
            if (!ModController.s_instance._characterLoaded) return;
            foreach (GameObject stool in stoolController._stools)
            {
                if (stool.GetComponentInParent<ItemTool>()) stoolController._lastHeldStool = stool.name;
            }
            ModController.s_instance.PrintLog($"Saved '{stoolController._lastHeldStool}'!");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MemoryUplinkTrigger), nameof(MemoryUplinkTrigger.OnResumeSimulation))]
        public static void LoadHeldStool()
        {
            StoolController stoolController = Instance;
            if (stoolController._lastHeldStool == null) return;
            else FindObjectOfType<ItemTool>().PickUpItemInstantly(GameObject.Find(stoolController._lastHeldStool).GetComponent<OWItem>());
            ModController.s_instance.PrintLog($"Loaded '{stoolController._lastHeldStool}'!");
        }
    }
}