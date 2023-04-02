using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmolHatchling
{
    public class StoolController : MonoBehaviour
    {
        public static StoolController Instance;
        private AssetBundle models;
        private List<GameObject> stools;
        private Material hearthTexture, nomaiTexture, quantumTexture, strangerTexture, dreamTexture, simTexture;

        public void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(StoolControllerPatches));
        }

        public void Start()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (models == null) models = SmolHatchling.Instance.ModHelper.Assets.LoadBundle("Assets/models");
                stools = new List<GameObject>();
                if (SmolHatchling.Instance.disableStools == true) return;
                if (loadScene == OWScene.SolarSystem) OnSolarSystemLoaded();
            };
        }

        private void OnSolarSystemLoaded()
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

            hearthTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_HEA_VillagePlanks_mat").FirstOrDefault();
            nomaiTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_NOM_HexagonTile_mat").FirstOrDefault();
            quantumTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Terrain_QM_CenterArch_mat").FirstOrDefault();
            strangerTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_IP_Mangrove_Wood_mat").FirstOrDefault();
            dreamTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Structure_DW_Mangrove_Wood_mat").FirstOrDefault();
            simTexture = Resources.FindObjectsOfTypeAll<Material>().Where((x) => x.name == "Terrain_IP_DreamGridVP_mat").FirstOrDefault();

            switch (LoadManager.s_currentScene)
            {
                case OWScene.SolarSystem:

                    // Place stool sockets
                    PlaceObject(NewStoolSocket(), GameObject.Find("ModelRocketStation_AttachPoint"), new Vector3(0.0054f, -1.0032f, -0.0627f), Quaternion.Euler(0, 0, 0)); // Village model rocket
                    PlaceObject(NewStoolSocket(), GameObject.Find("TutorialCamera_Base/InteractZone"), new Vector3(-0.0237f, -0.9278f, -0.5853f), Quaternion.Euler(0, 0, 0)); // Village ghost matter camera
                    PlaceObject(NewStoolSocket(), GameObject.Find("TutorialProbeLauncher_Base/InteractZone"), new Vector3(-0.0496f, -1.0474f, -0.3875f), Quaternion.Euler(0, 0, 0)); // Village scout launcher
                    PlaceObject(NewStoolSocket(), stranger, new Vector3(-68.3667f, 12.9691f, -286.7644f), Quaternion.Euler(55.6455f, 283.0649f, 269.9998f)); // River Lowlands slide player
                    PlaceObject(NewStoolSocket(), stranger, new Vector3(-276.8384f, -48.6267f, 59.6053f), Quaternion.Euler(302.8642f, 12.068f, 270f)); // Cinder Isles slide player
                    PlaceObject(NewStoolSocket(), stranger, new Vector3(124.4978f, -70.4448f, 209.9373f), Quaternion.Euler(11.7714f, 300.5211f, 90f)); // Hidden Gorge slide player
                    PlaceObject(NewStoolSocket(), stranger, new Vector3(230.4972f, 118.5905f, -43.9123f), Quaternion.Euler(353.3662f, 13.5101f, 89.5582f)); // Resevoir slide player
                    PlaceObject(NewStoolSocket(), GameObject.Find("Tunnel/Prefab_IP_DreamObjectProjector (2)"), new Vector3(0.0061f, 0.0937f, -1.4142f), Quaternion.Euler(359.8972f, 357.6237f, 359.9063f)); // Shrouded Woods tunnel door projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("Interactibles_DreamZone_1/RaftDock/Prefab_IP_DreamRaftProjector"), new Vector3(-0.0815f, -0.04f, -1.2619f), Quaternion.Euler(0, 353.6009f, 0)); // Shrouded Woods raft projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("ProjectedBridge/Prefab_IP_DreamObjectProjector (1)"), new Vector3(0.02f, -0.0485f, -1.314f), Quaternion.Euler(358.1544f, 354.8182f, 1.8163f)); // Shrouded Woods bridge projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("LowerLevel/RaftDockProjector/Prefab_IP_DreamObjectProjector"), new Vector3(-0.0109f, 0.0184f, -1.2981f), Quaternion.Euler(357.7859f, 358.1634f, 359.8753f)); // Starlit Cove house projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("Interactibles_DreamZone_2/Prefab_IP_DreamObjectProjector"), new Vector3(-0.1233f, 0.3758f, -1.3279f), Quaternion.Euler(357.8555f, 347.2135f, 0.6181f)); // Starlit Cove lights projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("Prefab_IP_DreamObjectProjector_Bridge"), new Vector3(-0.0057f, 0.189f, -1.3892f), Quaternion.Euler(359.5933f, 355.1652f, 1.915f)); // Endless Canyon starting bridge projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("Interactibles_Hotel/Lobby/Prefab_IP_DreamObjectProjector"), new Vector3(0.0076f, 0, -1.216f), Quaternion.Euler(0, 349.9582f, 0)); // Endless Canyon indoor bridge projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("Prefab_IP_DreamObjectProjector_Hotel"), new Vector3(0.0323f, -0.0514f, -1.3253f), Quaternion.Euler(0, 353.3195f, 0)); // Endless Canyon lights projector
                    PlaceObject(NewStoolSocket(), GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Interactibles_DreamZone_3/RaftDock (1)/Prefab_IP_DreamRaftProjector/"), new Vector3(0.0161f, -0.0204f, -1.2457f), Quaternion.Euler(0, 350, 0)); // Endless Canyon Raft Projector
                    PlaceObject(NewStoolSocket(), dreamWorld, new Vector3(-1.984032f, -290.51f, 685.7135f), Quaternion.Euler(0, 353.3195f, 0)); // Subterr Lake code wheel
                    PlaceObject(NewStoolSocket(), GameObject.Find("Island_A/Interactibles_Island_A/Prefab_IP_DreamObjectProjector (4)"), new Vector3(0.0792f, 0.1458f, -1.483f), Quaternion.Euler(2.219f, 344.8053f, 3.3272f)); // Subterr Lake prison projector 1
                    PlaceObject(NewStoolSocket(), GameObject.Find("Interactibles_Island_B/Prefab_IP_DreamObjectProjector (3)"), new Vector3(0.0884f, -0.1407f, -1.3932f), Quaternion.Euler(8.5598f, 343.9718f, 2.0844f)); // Subterr Lake prison projector 2
                    PlaceObject(NewStoolSocket(), GameObject.Find("Interactibles_Island_C/Prefab_IP_DreamObjectProjector (2)"), new Vector3(0.068f, 0.1968f, -1.3156f), Quaternion.Euler(0.0902f, 348.7343f, 0.0428f)); // Subterr Lake prison projector 3

                    // Place a crap ton of stools
                    PlaceObject(NewStool(), timberHearth, new Vector3(22.62274f, -13.01972f, 199.5054f)); // Village model rocket
                    PlaceObject(NewStool(), timberHearth, new Vector3(35.189f, 49.0149f, 225.9513f), Quaternion.Euler(51.7721f, 82.6791f, 70.8254f)); // Village ghost matter camera
                    PlaceObject(NewStool(), quantumMoon, new Vector3(-2.4616f, -68.6764f, 6.2243f)); // Solanum
                    PlaceObject(NewStool(), stranger, new Vector3(-70.0019f, 13.0961f, -286.3849f)); // River Lowlands slide player
                    PlaceObject(NewStool(), stranger, new Vector3(-273.9175f, -54.951f, 58.9185f)); // Cinder Isles slide player
                    PlaceObject(NewStool(), stranger, new Vector3(120.2041f, -70.7477f, 212.4686f)); // Hidden Gorge Slide player
                    PlaceObject(NewStool(), stranger, new Vector3(180.4737f, 136.3092f, -153.2241f)); // Resevoir code wheel
                    PlaceObject(NewStool(), stranger, new Vector3(231.4058f, 121.4653f, -40.2097f)); // Reservoir slide player
                    PlaceObject(NewStool(), dreamWorld, new Vector3(58.0762f, 1.0248f, -677.8818f)); // Shrouded Woods tunnel door projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(59.3558f, 1.1f, -752.6471f)); // Shrouded Woods raft projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(59.3381f, 8.7338f, -607.8622f)); // Shrouded Woods bridge projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(51.2455f, 13.7052f, -150.3283f)); // Starlit Cove house projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(79.0349f, 25.0568f, -302.9778f)); // Starlit Cove lights projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(14.8133f, 92.0101f, 269.9755f)); // Endless Canyon starting bridge
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-38.8031f, 81.15f, 222.5402f)); // Endless Canyon indoor bridge projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(24.3027f, 93.8709f, 207.3266f)); // Endless Canyon lights projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-22.0802f, 1.16f, 273.6684f)); // Endless Canyon raft projector
                    PlaceObject(NewStool(), dreamWorld, new Vector3(8.796f, -290.8733f, 681.2858f), Quaternion.Euler(358.46f, 180.3598f, 359.1666f)); // Subterr Lake 1
                    PlaceObject(NewStool(), dreamWorld, new Vector3(66.4256f, -290.227f, 632.392f), Quaternion.Euler(355.5746f, 127.8159f, 359.0247f)); // Subterr Lake 2
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-5.7661f, -294.7887f, 603.1592f), Quaternion.Euler(358.9676f, 166.8059f, 357.8715f)); // Subterr Lake 3
                    PlaceObject(NewStool(), dreamWorld, new Vector3(-69.6572f, -290.137f, 630.3542f), Quaternion.Euler(359.5942f, 216.7978f, 1.1301f)); // Subterr Lake 4
                    break;
            }

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



        public void PlaceObject(GameObject gameObject, GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            gameObject.transform.parent = parent.transform;
            gameObject.transform.localPosition = localPos;
            gameObject.transform.localRotation = localRot;

            switch (gameObject.name)
            {
                default:
                    break;

                case "SH_Stool":
                    if (gameObject.GetComponentInParent<AstroObject>())
                    {
                        MeshRenderer stoolRenderer = gameObject.transform.Find("Real").GetComponent<MeshRenderer>();
                        switch (gameObject.GetComponentInParent<AstroObject>().name)
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
                    }
                    break;
            }
        }

        public void PlaceObject(GameObject gameObject, GameObject parentBody, Vector3 localPos)
        {
            Vector3 up;
            gameObject.transform.parent = parentBody.transform;
            gameObject.transform.localPosition = localPos;

            switch (gameObject.GetComponentInParent<AstroObject>().name)
            {
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
                    switch (gameObject.GetComponentInParent<AstroObject>().gameObject.name)
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

        public void UpdateStoolSize()
        {
            foreach (GameObject stool in stools)
            {
                GameObject realObject = stool.transform.Find("Real").gameObject;
                GameObject dreamObject = stool.transform.Find("Simulation").gameObject;
                BoxCollider collider = stool.GetComponent<BoxCollider>();
                realObject.transform.localScale = dreamObject.transform.localScale = new Vector3(0.5f, -SmolHatchling.Instance.targetScale.y + 1, 0.5f);
                collider.size = new Vector3(0.875f, 1.8f * -SmolHatchling.Instance.targetScale.y + 1.8f, 0.875f);
                collider.center = new Vector3(0, 0.5f * collider.size.y, 0);
                stool.SetActive(SmolHatchling.Instance.targetScale.y < 1);
            }
        }
    }
}
