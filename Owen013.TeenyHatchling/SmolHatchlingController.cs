using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;

namespace SmolHatchling
{
    public class SmolHatchlingController : ModBehaviour
    {
        // Config vars
        public bool _debugLogEnabled;
        public bool _autoRadius;
        public bool _pitchChangeEnabled;
        public float _height;
        public float _radius;
        public string _colliderMode;
        public bool _enableStools;
        public bool _autoScaleStools;
        public float _stoolHeight;

        // Mod vars
        public static SmolHatchlingController Instance;
        public StoolController _stoolController;
        public Vector3 _targetScale, _playerScale, _colliderScale;
        public GameObject _playerModel, _playerThruster, _playerMarshmallowStick, _npcPlayer;
        public PlayerBody _playerBody;
        public PlayerCharacterController _characterController;
        public PlayerCameraController _cameraController;
        public PlayerAnimController _animController;
        public CapsuleCollider _playerCollider, _detectorCollider;
        public CapsuleShape _detectorShape;
        public ShipCockpitController _cockpitController;
        public ShipLogController _logController;
        public PlayerCloneController _cloneController;
        public EyeMirrorController _mirrorController;
        public List<OWAudioSource> _playerAudio;
        public bool _characterLoaded;
        public bool _hikersModEnabled;
        public float _animSpeed;
        
        public override object GetApi()
        {
            return new SmolHatchlingAPI();
        }

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            _height = config.GetSettingsValue<float>("Height %") / 100f;
            _radius = config.GetSettingsValue<float>("Radius %") / 100f;
            _colliderMode = config.GetSettingsValue<string>("Resize Collider");
            _pitchChangeEnabled = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
            _enableStools = config.GetSettingsValue<bool>("Enable Stools");
            _autoScaleStools = config.GetSettingsValue<bool>("Auto-Adjust Stool Height");
            _stoolHeight = config.GetSettingsValue<float>("Stool Height %") / 100f;
            _debugLogEnabled = config.GetSettingsValue<bool>("Enable Debug Log");

            UpdateTargetScale();
            if (_stoolController != null) _stoolController.UpdateStoolSize();
        }

        public void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(SmolHatchlingController));
        }

        public void Start()
        {
            // Add StoolController
            _stoolController = gameObject.AddComponent<StoolController>();

            // Set characterLoaded to false at the beginning of each scene load
            LoadManager.OnStartSceneLoad += (scene, loadScene) => _characterLoaded = false;

            // Ready
            ModHelper.Console.WriteLine($"Smol Hatchling is ready to go!", MessageType.Success);
        }

        public void FixedUpdate()
        {
            if (_playerScale != _targetScale) LerpSize();
            UpdateColliderScale();
        }

        public void OnCharacterStart()
        {
            _playerBody = FindObjectOfType<PlayerBody>();
            _playerCollider = _playerBody.GetComponent<CapsuleCollider>();
            _detectorCollider = _playerBody.transform.Find("PlayerDetector").GetComponent<CapsuleCollider>();
            _detectorShape = _playerBody.GetComponentInChildren<CapsuleShape>();
            _playerModel = _playerBody.transform.Find("Traveller_HEA_Player_v2").gameObject;
            _playerThruster = _playerBody.transform.Find("PlayerVFX").gameObject;
            _playerMarshmallowStick = _playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root").gameObject;
            _characterController = Locator.GetPlayerController();
            _cameraController = Locator.GetPlayerCameraController();
            _animController = FindObjectOfType<PlayerAnimController>();
            PlayerAudioController audioController = FindObjectOfType<PlayerAudioController>();
            PlayerBreathingAudio breathingAudio = FindObjectOfType<PlayerBreathingAudio>();

            _playerAudio = new List<OWAudioSource>()
            {
                audioController._oneShotSleepingAtCampfireSource,
                audioController._oneShotSource,
                breathingAudio._asphyxiationSource,
                breathingAudio._breathingLowOxygenSource,
                breathingAudio._breathingSource,
                breathingAudio._drowningSource
            };

            _characterLoaded = true;
            UpdateTargetScale();
            SnapSize();
        }

        public void UpdateTargetScale()
        {
            if (_autoRadius) _targetScale = new Vector3(Mathf.Sqrt(_height), _height, Mathf.Sqrt(_height));
            else _targetScale = new Vector3(_radius, _height, _radius);
        }

        public void SnapSize()
        {
            _playerScale = _targetScale;
            UpdatePlayerScale();
        }

        public void LerpSize()
        {
            _playerScale = Vector3.Lerp(_playerScale, _targetScale, 0.125f);
            UpdatePlayerScale();
        }

        public void UpdatePlayerScale()
        {
            if (!IsCorrectScene() || !_characterLoaded) return;

            // Change playermodel size and animation speed
            _playerModel.transform.localScale = _playerScale / 10;
            _playerModel.transform.localPosition = new Vector3(0, -1.03f, -0.2f * _playerScale.z);
            _playerThruster.transform.localScale = _playerScale;
            _playerThruster.transform.localPosition = new Vector3(0, -1 + _playerScale.y, 0);

            // Move camera and marshmallow stick root down to match new player height
            _cameraController._origLocalPosition = new Vector3(0f, -1 + 1.8496f * _playerScale.y, 0.15f * _playerScale.z);
            _cameraController.transform.localPosition = _cameraController._origLocalPosition;
            _playerMarshmallowStick.transform.localPosition = new Vector3(0.25f, -1.8496f + 1.8496f * _playerScale.y, 0.08f - 0.15f + 0.15f * _playerScale.z);

            // Change pitch if enabled
            switch (_pitchChangeEnabled)
            {
                case true:
                    float pitch = 0.5f * Mathf.Pow(_playerScale.y, -1) + 0.5f;
                    foreach (OWAudioSource audio in _playerAudio) audio.pitch = pitch;
                    break;

                case false:
                    foreach (OWAudioSource audio in _playerAudio) audio.pitch = 1;
                    break;
            }

            // Change size of Ash Twin Project player clone if it exists
            if (_npcPlayer != null)
            {
                _npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale = _playerScale / 10;
                _npcPlayer.transform.Find("NPC_Player_FocalPoint").localPosition = new Vector3(-0.093f, 0.991f * _targetScale.y, 0.102f);
            }

            UpdateCockpitAttachPoint();
            UpdateLogAttachPoint();
            UpdateEyeCloneScale();
            UpdateMirrorCloneScale();

            _animSpeed = Mathf.Pow(_targetScale.z, -1);
            if (!_hikersModEnabled)
            {
                _animController._animator.speed = _animSpeed;
                if (_cloneController != null) _cloneController._playerVisuals.GetComponent<PlayerAnimController>()._animator.speed = _animSpeed;
                if (_mirrorController != null) _mirrorController._mirrorPlayer.GetComponentInChildren<PlayerAnimController>()._animator.speed = _animSpeed;
            }
        }

        public void UpdateCockpitAttachPoint()
        {
            if (_cockpitController == null) return;
            _cockpitController._origAttachPointLocalPos = new Vector3(0, 2.1849f - 1.8496f * _playerScale.y, 4.2307f + 0.15f - 0.15f * _playerScale.z);
        }

        public void UpdateLogAttachPoint()
        {
            if (_logController == null) return;
            _logController._attachPoint._attachOffset = new Vector3(0f, 1.8496f - 1.8496f * _playerScale.y, 0.15f - 0.15f * _playerScale.z);
        }

        public void UpdateEyeCloneScale()
        {
            if (_cloneController == null) return;
            _cloneController._playerVisuals.transform.localScale = _playerScale / 10;
            _cloneController._signal._owAudioSource.pitch = _playerAudio[0].pitch;
        }

        public void UpdateMirrorCloneScale()
        {
            // Update mirror player scale if it exists
            if (_mirrorController == null) return;
            _mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = _playerScale / 10;
        }

        public void UpdateColliderScale()
        {
            if (!IsCorrectScene() || !_characterLoaded) return;

            // Change collider height and radius, and move colliders down so they touch the ground
            if (_colliderMode == "Height & Radius") _colliderScale = _targetScale;
            else if (_colliderMode == "Height Only") _colliderScale = new Vector3(1, _targetScale.y, 1);
            else _colliderScale = Vector3.one;
            float height = 2 * _colliderScale.y;
            float radius = Mathf.Min(_colliderScale.z / 2, height / 2);
            Vector3 center = new Vector3(0, _colliderScale.y - 1, 0);
            _playerCollider.height = _detectorCollider.height = _detectorShape.height = height;
            _playerCollider.radius = _detectorCollider.radius = _detectorShape.radius = radius;
            _playerCollider.center = _detectorCollider.center = _detectorShape.center = _playerBody._centerOfMass = _playerCollider.center = _detectorCollider.center = _playerBody._activeRigidbody.centerOfMass = center;
        }

        public bool IsCorrectScene()
        {
            return LoadManager.s_currentScene == OWScene.SolarSystem || LoadManager.s_currentScene == OWScene.EyeOfTheUniverse;
        }

        public void PrintLog(string text)
        {
            if (!_debugLogEnabled) return;
            ModHelper.Console.WriteLine(text);
        }

        public void PrintLog(string text, MessageType messageType)
        {
            if (!_debugLogEnabled) return;
            ModHelper.Console.WriteLine(text, messageType);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.Start))]
        public static void CharacterStart()
        {
            Instance.OnCharacterStart();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.Start))]
        public static void OnShipCockpitStart(ShipCockpitController __instance) => Instance._cockpitController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogController), nameof(ShipLogController.Start))]
        public static void OnShipLogStart(ShipLogController __instance) => Instance._logController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerScreamingController), nameof(PlayerScreamingController.Awake))]
        public static void NPCPlayerAwake(PlayerScreamingController __instance)
        {
            Instance._npcPlayer = __instance.gameObject;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCloneController), nameof(PlayerCloneController.Start))]
        public static void EyeCloneStart(PlayerCloneController __instance) => Instance._cloneController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EyeMirrorController), nameof(EyeMirrorController.Start))]
        public static void EyeMirrorStart(EyeMirrorController __instance) => Instance._mirrorController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 targetScale = Instance._targetScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset = new Vector3(0, 1.8496f - 1.8496f * targetScale.y, 0.15f - 0.15f * targetScale.z);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
        public static void SetRunAnimFloats(PlayerAnimController __instance)
        {
            Vector3 vector = Vector3.zero;
            if (!PlayerState.IsAttached())
            {
                vector = Instance._characterController.GetRelativeGroundVelocity();
            }
            if (Mathf.Abs(vector.x) < 0.05f)
            {
                vector.x = 0f;
            }
            if (Mathf.Abs(vector.z) < 0.05f)
            {
                vector.z = 0f;
            }
            __instance._animator.SetFloat("RunSpeedX", vector.x / (3f * Instance._targetScale.z));
            __instance._animator.SetFloat("RunSpeedY", vector.z / (3f * Instance._targetScale.z));
        }
    }
}