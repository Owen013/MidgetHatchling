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
        public float _animSpeed;
        public string _colliderMode;
        public bool _useStoryAttributes;

        // Mod vars
        public static SmolHatchlingController Instance;
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

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            _debugLogEnabled = config.GetSettingsValue<bool>("Enable Debug Log");
            _height = config.GetSettingsValue<float>("Height %");
            _radius = config.GetSettingsValue<float>("Radius %");
            _autoRadius = config.GetSettingsValue<bool>("Auto-Radius");
            _colliderMode = config.GetSettingsValue<string>("Resize Collider");
            _pitchChangeEnabled = config.GetSettingsValue<bool>("Change Pitch Depending on Height");

            UpdateTargetScale();
            UpdateStoryAttributes();
        }

        public void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(SmolHatchlingController));
        }

        public void Start()
        {
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

            switch (LoadManager.s_currentScene)
            {
                // If in solar system, set these ship vars
                case OWScene.SolarSystem:
                    _cockpitController = FindObjectOfType<ShipCockpitController>();
                    _logController = FindObjectOfType<ShipLogController>();
                    break;
                // If at eye, set clone and mirror vars
                case OWScene.EyeOfTheUniverse:
                    var cloneControllers = Resources.FindObjectsOfTypeAll<PlayerCloneController>();
                    _cloneController = cloneControllers[0];
                    var mirrorControllers = Resources.FindObjectsOfTypeAll<EyeMirrorController>();
                    _mirrorController = mirrorControllers[0];
                    break;
            }

            _characterLoaded = true;
            UpdateTargetScale();
            SnapSize();
            //UpdateStoryAttributes();
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
            _animSpeed = Mathf.Pow(_targetScale.z, -1);
            _animController._animator.speed = _animSpeed;

            // Move camera and marshmallow stick root down to match new player height
            _cameraController._origLocalPosition = new Vector3(0f, -1 + 1.8496f * _playerScale.y, 0.15f * _playerScale.z);
            _cameraController.transform.localPosition = _cameraController._origLocalPosition;
            _playerMarshmallowStick.transform.localPosition = new Vector3(0.25f, -1.8496f + 1.8496f * _playerScale.y, 0.08f - 0.15f + 0.15f * _playerScale.z);
            
            // Change size of Ash Twin Project player clone if it exists
            if (_npcPlayer != null)
            {
                _npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale = _playerScale / 10;
                _npcPlayer.transform.Find("NPC_Player_FocalPoint").localPosition = new Vector3(-0.093f, 0.991f * _targetScale.y, 0.102f);
            }

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

            switch (LoadManager.s_currentScene)
            {
                // Only do these in Solar System
                case OWScene.SolarSystem:
                    _cockpitController._origAttachPointLocalPos = new Vector3(0, 2.1849f - 1.8496f * _playerScale.y, 4.2307f + 0.15f - 0.15f * _playerScale.z);
                    _logController._attachPoint._attachOffset = new Vector3(0f, 1.8496f - 1.8496f * _playerScale.y, 0.15f - 0.15f * _playerScale.z);
                    break;
                // Only do these at the Eye
                case OWScene.EyeOfTheUniverse:
                    _cloneController._playerVisuals.transform.localScale = _playerScale / 10;
                    _cloneController._signal._owAudioSource.pitch = _playerAudio[0].pitch;
                    _mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = _playerScale / 10;
                    break;
            }
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

        public void UpdateStoryAttributes()
        {
            if (!IsCorrectScene() || !_characterLoaded) return;
            if (_useStoryAttributes)
            {
                _characterController._runSpeed = 4;
                _characterController._strafeSpeed = 4;
                _characterController._walkSpeed = 2;
                _characterController._maxJumpSpeed = 5;

            }
            else
            {
                _characterController._runSpeed = 6;
                _characterController._strafeSpeed = 4;
                _characterController._walkSpeed = 3;
                _characterController._maxJumpSpeed = 7;
            }
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
        [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 targetScale = Instance._targetScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset = new Vector3(0, 1.8496f - 1.8496f * targetScale.y, 0.15f - 0.15f * targetScale.z);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerScreamingController), nameof(PlayerScreamingController.Awake))]
        public static void NPCPlayerAwake(PlayerScreamingController __instance)
        {
            Instance._npcPlayer = __instance.gameObject;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCloneController), nameof(PlayerCloneController.Start))]
        public static void EyeCloneStart(PlayerCloneController __instance)
        {
            Vector3 playerScale = Instance._playerScale;
            float pitch;
            __instance._playerVisuals.transform.localScale = playerScale / 10;
            if (Instance._pitchChangeEnabled) pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
            else pitch = 1;
            __instance._signal._owAudioSource.pitch = pitch;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EyeMirrorController), nameof(EyeMirrorController.Start))]
        public static void EyeMirrorStart(EyeMirrorController __instance)
        {
            Vector3 playerScale = Instance._playerScale;
            __instance._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
        }
    }
}