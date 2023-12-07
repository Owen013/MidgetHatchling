using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;

namespace SmolHatchling
{
    public class ModController : ModBehaviour
    {
        public static ModController s_instance;
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
        public bool _hikersModEnabled;
        public float _animSpeed;

        // Config
        public bool _debugLogEnabled;
        public bool _autoRadius;
        public bool _pitchChangeEnabled;
        public float _height;
        public float _radius;
        public string _colliderMode;
        public bool _enableStools;
        public bool _autoScaleStools;
        public float _stoolHeight;

        public override object GetApi()
        {
            return new SmolHatchlingAPI();
        }

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            _height = config.GetSettingsValue<float>("Height");
            _radius = config.GetSettingsValue<float>("Radius");
            _colliderMode = config.GetSettingsValue<string>("Resize Collider");
            _pitchChangeEnabled = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
            _enableStools = config.GetSettingsValue<bool>("Enable Stools");
            _autoScaleStools = config.GetSettingsValue<bool>("Auto-Adjust Stool Height");
            _stoolHeight = config.GetSettingsValue<float>("Stool Height");
            _debugLogEnabled = config.GetSettingsValue<bool>("Enable Debug Log");

            UpdateTargetScale();
            if (_stoolController != null) _stoolController.UpdateStoolSize();
        }

        public void Awake()
        {
            s_instance = this;
            _stoolController = gameObject.AddComponent<StoolController>();
            Harmony.CreateAndPatchAll(typeof(ModController));
        }

        public void Start()
        {
            // Ready
            ModHelper.Console.WriteLine($"Smol Hatchling is ready to go!", MessageType.Success);
        }

        public void FixedUpdate()
        {
            if (_playerScale != _targetScale) LerpSize();
            UpdateColliderScale();
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
            if (!_characterController) return;

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
            if (!_characterController) return;

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
        public static void CharacterStart(PlayerCharacterController __instance)
        {
            s_instance._characterController = __instance;
            s_instance._playerBody = FindObjectOfType<PlayerBody>();
            s_instance._playerCollider = s_instance._playerBody.GetComponent<CapsuleCollider>();
            s_instance._detectorCollider = s_instance._playerBody.transform.Find("PlayerDetector").GetComponent<CapsuleCollider>();
            s_instance._detectorShape = s_instance._playerBody.GetComponentInChildren<CapsuleShape>();
            s_instance._playerModel = s_instance._playerBody.transform.Find("Traveller_HEA_Player_v2").gameObject;
            s_instance._playerThruster = s_instance._playerBody.transform.Find("PlayerVFX").gameObject;
            s_instance._playerMarshmallowStick = s_instance._playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root").gameObject;
            s_instance._cameraController = Locator.GetPlayerCameraController();
            s_instance._animController = FindObjectOfType<PlayerAnimController>();
            PlayerAudioController audioController = FindObjectOfType<PlayerAudioController>();
            PlayerBreathingAudio breathingAudio = FindObjectOfType<PlayerBreathingAudio>();

            s_instance._playerAudio = new List<OWAudioSource>()
            {
                audioController._oneShotSleepingAtCampfireSource,
                //audioController._oneShotSource,
                breathingAudio._asphyxiationSource,
                breathingAudio._breathingLowOxygenSource,
                breathingAudio._breathingSource,
                breathingAudio._drowningSource
            };

            s_instance.UpdateTargetScale();
            s_instance.SnapSize();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.Start))]
        public static void OnShipCockpitStart(ShipCockpitController __instance) => s_instance._cockpitController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogController), nameof(ShipLogController.Start))]
        public static void OnShipLogStart(ShipLogController __instance) => s_instance._logController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerScreamingController), nameof(PlayerScreamingController.Awake))]
        public static void NPCPlayerAwake(PlayerScreamingController __instance)
        {
            s_instance._npcPlayer = __instance.gameObject;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCloneController), nameof(PlayerCloneController.Start))]
        public static void EyeCloneStart(PlayerCloneController __instance) => s_instance._cloneController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EyeMirrorController), nameof(EyeMirrorController.Start))]
        public static void EyeMirrorStart(EyeMirrorController __instance) => s_instance._mirrorController = __instance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 targetScale = s_instance._targetScale;
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
                vector = s_instance._characterController.GetRelativeGroundVelocity();
            }
            if (Mathf.Abs(vector.x) < 0.05f)
            {
                vector.x = 0f;
            }
            if (Mathf.Abs(vector.z) < 0.05f)
            {
                vector.z = 0f;
            }
            __instance._animator.SetFloat("RunSpeedX", vector.x / (3f * s_instance._targetScale.z));
            __instance._animator.SetFloat("RunSpeedY", vector.z / (3f * s_instance._targetScale.z));
        }

        // replacing the entire method is far from ideal, but i don't know how to transpile yet
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.CastForGrounded))]
        public static bool CastForGrounded(PlayerCharacterController __instance)
        {
            float num = Time.fixedDeltaTime * 60f;
            bool flag = __instance._fluidDetector.InFluidType(FluidVolume.Type.TRACTOR_BEAM) || __instance._fluidDetector.InFluidType(FluidVolume.Type.SAND) || __instance._fluidDetector.InFluidType(FluidVolume.Type.WATER);
            bool flag2 = __instance._groundSnappingEnabled && __instance._wasGrounded && !flag && __instance._jetpackModel.GetLocalAcceleration().y <= 0f && Time.time > __instance._lastJumpTime + 0.5f;
            float num2 = (flag2 ? 0.1f : 0.06f) * num;
            Vector3 localUpDirection = __instance._owRigidbody.GetLocalUpDirection();
            float num3 = 0.46f;
            float maxDistance = num2 + (1f - num3);
            int num4 = Physics.SphereCastNonAlloc(__instance._owRigidbody.GetPosition(), num3, -localUpDirection, __instance._raycastHits, maxDistance, OWLayerMask.groundMask, QueryTriggerInteraction.Ignore);
            RaycastHit raycastHit = default(RaycastHit);
            bool flag3 = false;
            for (int i = 0; i < num4; i++)
            {
                if (__instance.IsValidGroundedHit(__instance._raycastHits[i]))
                {
                    if (!flag3)
                    {
                        raycastHit = __instance._raycastHits[i];
                        flag3 = true;
                    }
                    else if (__instance._raycastHits[i].distance < raycastHit.distance)
                    {
                        raycastHit = __instance._raycastHits[i];
                    }
                }
            }
            if (flag3)
            {
                float num5 = float.PositiveInfinity;
                bool flag4 = false;
                if (__instance.AllowGroundedOnRigidbody(raycastHit.rigidbody) && Vector3.Angle(localUpDirection, raycastHit.normal) <= (float)__instance._maxAngleToBeGrounded)
                {
                    num5 = __instance.GetGroundHitDistance(raycastHit);
                    flag4 = true;
                }
                else
                {
                    for (int j = 0; j < num4; j++)
                    {
                        if (__instance.IsValidGroundedHit(__instance._raycastHits[j]) && __instance.AllowGroundedOnRigidbody(__instance._raycastHits[j].rigidbody))
                        {
                            float groundHitDistance = __instance.GetGroundHitDistance(__instance._raycastHits[j]);
                            num5 = Mathf.Min(num5, groundHitDistance);
                            if (Vector3.Angle(localUpDirection, __instance._raycastHits[j].normal) <= (float)__instance._maxAngleToBeGrounded)
                            {
                                raycastHit = __instance._raycastHits[j];
                                flag4 = true;
                            }
                            else
                            {
                                __instance._raycastHitNormals[j] = Vector3.ProjectOnPlane(__instance._raycastHits[j].normal, localUpDirection);
                            }
                        }
                    }
                    if (!flag4)
                    {
                        int num6 = 0;
                        while (num6 < num4 && !__instance._isGrounded)
                        {
                            if (__instance.IsValidGroundedHit(__instance._raycastHits[num6]))
                            {
                                int num7 = num6 + 1;
                                while (num7 < num4 && !__instance._isGrounded)
                                {
                                    if (__instance.IsValidGroundedHit(__instance._raycastHits[num6]) && Vector3.Angle(__instance._raycastHitNormals[num6], __instance._raycastHitNormals[num7]) > (float)__instance._maxAngleBetweenSlopes)
                                    {
                                        flag4 = true;
                                        raycastHit = __instance._raycastHits[num6];
                                        num5 = __instance.GetGroundHitDistance(__instance._raycastHits[num6]);
                                        break;
                                    }
                                    num7++;
                                }
                            }
                            num6++;
                        }
                    }
                    if (!flag4 && __instance._wasGrounded && raycastHit.collider.material.dynamicFriction > 0f)
                    {
                        Vector3 onNormal = __instance._transform.InverseTransformPoint(raycastHit.point);
                        onNormal.y = 0f;
                        Vector3 vector = __instance._transform.InverseTransformDirection(__instance._owRigidbody.GetVelocity() - __instance._groundBody.GetPointVelocity(__instance._transform.position));
                        if (vector.y > 0f)
                        {
                            Vector3 a = -Vector3.Project(vector, onNormal);
                            a.y = -vector.y;
                            __instance._owRigidbody.AddLocalVelocityChange(a * 0.7f * num);
                        }
                    }
                }
                IgnoreCollision ignoreCollision = flag4 ? raycastHit.collider.GetComponent<IgnoreCollision>() : null;
                if (flag4 && (ignoreCollision == null || !ignoreCollision.IgnoresPlayer()))
                {
                    if (flag2)
                    {
                        Vector3 vector2 = -localUpDirection * Mathf.Max(0f, num5);
                        vector2 += localUpDirection * 0.001f;
                        __instance.transform.position = __instance.transform.position + vector2;
                    }
                    if (raycastHit.rigidbody != null)
                    {
                        __instance._groundBody = raycastHit.rigidbody.GetRequiredComponent<OWRigidbody>();
                    }
                    else
                    {
                        Debug.LogError("The collider we're trying to stand on is not attached to a Rigidbody!");
                        Debug.Break();
                    }
                    __instance._groundCollider = raycastHit.collider;
                    __instance._groundContactPt = raycastHit.point;
                    __instance._groundNormal = raycastHit.normal;
                    __instance._groundSurface = Locator.GetSurfaceManager().GetHitSurfaceType(raycastHit);
                    __instance._collidingQuantumObject = __instance._groundCollider.GetComponentInParent<QuantumObject>();
                    if (__instance._collidingQuantumObject != null)
                    {
                        __instance._collidingQuantumObject.SetPlayerStandingOnObject(true);
                    }
                    __instance._movingPlatform = __instance._groundCollider.GetComponentInParent<MovingPlatform>();
                    __instance._groundedOnRisingSand = __instance._groundCollider.CompareTag("RisingSand");
                    __instance._antiSinkingCollider.enabled = (__instance._wasGrounded && num5 > -0.1f);
                    __instance._isGrounded = true;
                    if (!__instance._wasGrounded && __instance.OnBecomeGrounded != null)
                    {
                        __instance.OnBecomeGrounded();
                    }
                }
            }
            return false;
        }
    }
}