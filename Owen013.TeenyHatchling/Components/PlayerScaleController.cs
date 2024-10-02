using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmolHatchling.Components;

[HarmonyPatch]
public class PlayerScaleController : ScaleController
{
    public static PlayerScaleController Instance { get; private set; }

    public static float AnimSpeed { get; private set; }

    public static float DefaultScale = 1;

    public override float Scale
    {
        set
        {
            transform.localScale = Vector3.one * value;
            EaseToScale(value);
            Locator.GetPlayerCamera().nearClipPlane = Mathf.Min(0.1f, 0.1f * Scale);
        }
    }

    private float _resetButtonHeldTime;

    public override void EaseToScale(float scale)
    {
        base.EaseToScale(scale);
        ModMain.HikersModAPI?.UpdateConfig();
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        Config.OnConfigured += () =>
        {
            if (Config.UseCustomPlayerScale)
            {
                EaseToScale(Config.PlayerScale);
            }
            else
            {
                EaseToScale(DefaultScale);
            }
        };
    }

    private void Update()
    {
        if (Keyboard.current[Key.Slash].isPressed)
        {
            if (_resetButtonHeldTime >= 5f)
            {
                Config.SetConfigSetting("UseCustomPlayerScale", false);
                EaseToScale(DefaultScale);
                _resetButtonHeldTime = 0;
                ModMain.Print("'Use Custom Player Scale' disabled");
            }
            else
            {
                _resetButtonHeldTime += Time.unscaledDeltaTime;
            }
        }
        else
        {
            _resetButtonHeldTime = 0;
        }

        if (OWInput.IsInputMode(InputMode.Character) && Config.UseCustomPlayerScale && Config.UseScaleHotkeys)
        {
            if (Keyboard.current[Key.Comma].wasPressedThisFrame)
            {
                float currentScale = Config.PlayerScale;
                float newScale = currentScale / 2;
                Config.SetConfigSetting("PlayerScale", newScale);
                EaseToScale(newScale);
            }

            if (Keyboard.current[Key.Period].wasPressedThisFrame)
            {
                float currentScale = Config.PlayerScale;
                float newScale = currentScale * 2;
                Config.SetConfigSetting("PlayerScale", newScale);
                EaseToScale(newScale);
            }

            if (Keyboard.current[Key.Slash].wasPressedThisFrame)
            {
                float newScale = 1;
                Config.SetConfigSetting("PlayerScale", newScale);
                EaseToScale(newScale);
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (!Config.UseCustomPlayerScale && TargetScale != DefaultScale)
        {
            EaseToScale(DefaultScale);
        }

        base.FixedUpdate();

        if (Scale != TargetScale)
        {
            Locator.GetPlayerCamera().nearClipPlane = Mathf.Min(0.1f, 0.1f * Scale);
        }

        if (ModMain.HikersModAPI == null)
        {
            PlayerCharacterController player = GetComponent<PlayerCharacterController>();
            if (Config.UseScaledPlayerSpeed)
            {
                player._runSpeed = 6 * Scale;
                player._strafeSpeed = 4 * Scale;
                player._walkSpeed = 3 * Scale;
                player._airSpeed = 3 * Scale;
                player._acceleration = 0.5f * Scale;
                player._airAcceleration = 0.09f * Scale;
                player._minJumpSpeed = 3 * Scale;
                player._maxJumpSpeed = 7 * Scale;
            }
            else
            {
                player._runSpeed = 6;
                player._strafeSpeed = 4;
                player._walkSpeed = 3;
                player._airSpeed = 3;
                player._acceleration = 0.5f;
                player._airAcceleration = 0.09f;
                player._minJumpSpeed = 3;
                player._maxJumpSpeed = 7;
            }
        }
    }

    private void LateUpdate()
    {
        AnimSpeed = 1f / Instance.Scale;
        if (ModMain.HikersModAPI == null)
        {
            AnimSpeed = Mathf.Max(Mathf.Sqrt(Locator.GetPlayerController().GetRelativeGroundVelocity().magnitude * AnimSpeed / 6f), 1f);
            if (!ModMain.IsImmersionInstalled)
            {
                Locator.GetPlayerController().GetComponentInChildren<Animator>().speed = AnimSpeed;
            }
        }
    }

    // PATCHES

    // transpiling this would probably be better but i no know how
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.CastForGrounded))]
    private static bool PlayerCharacterController_CastForGrounded(PlayerCharacterController __instance)
    {
        if (Instance.Scale == 1) return true;

        float time = Time.fixedDeltaTime * 60f;
        bool isInFluid = __instance._fluidDetector.InFluidType(FluidVolume.Type.TRACTOR_BEAM) || __instance._fluidDetector.InFluidType(FluidVolume.Type.SAND) || __instance._fluidDetector.InFluidType(FluidVolume.Type.WATER);
        bool wasGrounded = __instance._groundSnappingEnabled && __instance._wasGrounded && !isInFluid && __instance._jetpackModel.GetLocalAcceleration().y <= 0f && Time.time > __instance._lastJumpTime + 0.5f;
        Vector3 localUpDirection = __instance._owRigidbody.GetLocalUpDirection();
        float radius = 0.46f;
        float maxDistance = (wasGrounded ? 0.1f : 0.06f) * time + (1f - radius);
        float scale = Instance.Scale;
        int numSphereCastHits = Physics.SphereCastNonAlloc(__instance._owRigidbody.GetPosition(), radius * scale, -localUpDirection, __instance._raycastHits, maxDistance * scale, OWLayerMask.groundMask, QueryTriggerInteraction.Ignore);
        RaycastHit raycastHit = default(RaycastHit);
        bool hasValidGroundedHit = false;
        for (int i = 0; i < numSphereCastHits; i++)
        {
            if (__instance.IsValidGroundedHit(__instance._raycastHits[i]))
            {
                if (!hasValidGroundedHit)
                {
                    raycastHit = __instance._raycastHits[i];
                    hasValidGroundedHit = true;
                }
                else if (__instance._raycastHits[i].distance < raycastHit.distance)
                {
                    raycastHit = __instance._raycastHits[i];
                }
            }
        }

        if (hasValidGroundedHit)
        {
            float groundDistance = float.PositiveInfinity;
            bool canBecomeGrounded = false;
            if (__instance.AllowGroundedOnRigidbody(raycastHit.rigidbody) && Vector3.Angle(localUpDirection, raycastHit.normal) <= (float)__instance._maxAngleToBeGrounded)
            {
                groundDistance = __instance.GetGroundHitDistance(raycastHit);
                canBecomeGrounded = true;
            }
            else
            {
                for (int i = 0; i < numSphereCastHits; i++)
                {
                    if (__instance.IsValidGroundedHit(__instance._raycastHits[i]) && __instance.AllowGroundedOnRigidbody(__instance._raycastHits[i].rigidbody))
                    {
                        float groundHitDistance = __instance.GetGroundHitDistance(__instance._raycastHits[i]);
                        groundDistance = Mathf.Min(groundDistance, groundHitDistance);
                        if (Vector3.Angle(localUpDirection, __instance._raycastHits[i].normal) <= (float)__instance._maxAngleToBeGrounded)
                        {
                            raycastHit = __instance._raycastHits[i];
                            canBecomeGrounded = true;
                        }
                        else
                        {
                            __instance._raycastHitNormals[i] = Vector3.ProjectOnPlane(__instance._raycastHits[i].normal, localUpDirection);
                        }
                    }
                }

                if (!canBecomeGrounded)
                {
                    for (int i = 0; i < numSphereCastHits && !__instance._isGrounded; i++)
                    {
                        if (__instance.IsValidGroundedHit(__instance._raycastHits[i]))
                        {
                            for (int j = i + 1; j < numSphereCastHits && !__instance._isGrounded; j++)
                            {
                                if (__instance.IsValidGroundedHit(__instance._raycastHits[i]) && Vector3.Angle(__instance._raycastHitNormals[i], __instance._raycastHitNormals[j]) > (float)__instance._maxAngleBetweenSlopes)
                                {
                                    canBecomeGrounded = true;
                                    raycastHit = __instance._raycastHits[i];
                                    groundDistance = __instance.GetGroundHitDistance(__instance._raycastHits[i]);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!canBecomeGrounded && __instance._wasGrounded && raycastHit.collider.material.dynamicFriction > 0f)
                {
                    Vector3 onNormal = __instance._transform.InverseTransformPoint(raycastHit.point);
                    onNormal.y = 0f;
                    Vector3 vector = __instance._transform.InverseTransformDirection(__instance._owRigidbody.GetVelocity() - __instance._groundBody.GetPointVelocity(__instance._transform.position));
                    if (vector.y > 0f)
                    {
                        Vector3 projectedNormal = -Vector3.Project(vector, onNormal);
                        projectedNormal.y = -vector.y;
                        __instance._owRigidbody.AddLocalVelocityChange(projectedNormal * 0.7f * time);
                    }
                }
            }

            IgnoreCollision ignoreCollision = canBecomeGrounded ? raycastHit.collider.GetComponent<IgnoreCollision>() : null;
            if (canBecomeGrounded && (ignoreCollision == null || !ignoreCollision.IgnoresPlayer()))
            {
                if (wasGrounded)
                {
                    Vector3 vector2 = -localUpDirection * Mathf.Max(0f, groundDistance);
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
                __instance._antiSinkingCollider.enabled = (__instance._wasGrounded && groundDistance > -0.1f);
                __instance._isGrounded = true;
                if (!__instance._wasGrounded)
                {
                    Extensions.RaiseEvent(__instance, "OnBecomeGrounded");
                }
            }
        }

        return false;
    }

    // again i hate replacing entire methods but i don't know how else to do it
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.UpdateMovement))]
    private static bool PlayerCharacterController_UpdateMovement(PlayerCharacterController __instance)
    {
        if (Instance.Scale == 1) return true;

        Vector2 vector = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character | InputMode.NomaiRemoteCam);
        float magnitude = vector.magnitude;
        if (__instance._groundBody != null)
        {
            if (magnitude == 0f)
            {
                __instance.PreventSliding();
            }
            Vector3 pointAcceleration = __instance._groundBody.GetPointAcceleration(__instance._groundContactPt);
            Vector3 forceAcceleration = __instance._forceDetector.GetForceAcceleration();
            __instance._normalAcceleration = Vector3.Project(pointAcceleration - forceAcceleration, __instance._groundNormal);
        }
        bool flag = !OWInput.IsPressed(InputLibrary.rollMode, InputMode.Character | InputMode.NomaiRemoteCam, 0f) || __instance._heldLanternItem != null;
        float num = flag ? ((vector.y < 0f) ? __instance._strafeSpeed : __instance._runSpeed) : __instance._walkSpeed;
        float num2 = flag ? __instance._strafeSpeed : __instance._walkSpeed;
        if (__instance._heldLanternItem != null)
        {
            __instance._heldLanternItem.OverrideMaxRunSpeed(ref num2, ref num);
        }
        if (Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer())
        {
            num = Mathf.Min(num, __instance._walkSpeed);
            num2 = Mathf.Min(num2, __instance._walkSpeed);
        }
        if (__instance._jumpChargeTime > 0f && !__instance._useChargeCurve)
        {
            float t = Mathf.InverseLerp(1f, 2f, __instance._jumpChargeTime);
            num = Mathf.Min(num, Mathf.Lerp(num, 2f * Instance.Scale, t)); //
            num2 = Mathf.Min(num2, Mathf.Lerp(num2, 2f * Instance.Scale, t)); //
        }
        Vector3 a = new Vector3(vector.x * num2, 0f, vector.y * num);
        if (__instance._isStaggered)
        {
            float num3 = Mathf.Clamp01((Time.time - __instance._initStaggerTime) / __instance._staggerLength);
            a *= num3;
            if (num3 == 1f)
            {
                __instance._isStaggered = false;
            }
        }
        if (PlayerState.IsCameraUnderwater())
        {
            a *= 0.5f;
        }
        else if (!flag || a.magnitude <= __instance._walkSpeed)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(__instance._transform.position + __instance._transform.TransformDirection(new Vector3(vector.x, 0f, vector.y).normalized * 0.1f * Instance.Scale), -__instance._transform.up, out raycastHit, 20f * Instance.Scale, OWLayerMask.groundMask)) //
            {
                float num4 = raycastHit.distance / Instance.Scale - 1f; //
                if (num4 > 0.2f && (Vector3.Angle(__instance._owRigidbody.GetLocalUpDirection(), raycastHit.normal) > (float)__instance._maxAngleToBeGrounded || num4 > 1.5f))
                {
                    a = Vector3.zero;
                    vector = Vector2.zero;
                }
            }
            else
            {
                a = Vector3.zero;
                vector = Vector2.zero;
            }
        }
        __instance.SetPhysicsMaterial((magnitude > 0.01f || __instance._movingPlatform != null) ? __instance._runningPhysicMaterial : __instance._standingPhysicMaterial);
        Vector3 b = __instance._transform.InverseTransformDirection(__instance._owRigidbody.GetVelocity());
        Vector3 vector2 = a + __instance.GetLocalGroundFrameVelocity() - b;
        vector2.y = 0f;
        if (vector2.magnitude > __instance._tumbleThreshold)
        {
            __instance.InitTumble();
            return false;
        }
        float num5 = Time.fixedDeltaTime * 60f;
        float num6 = __instance._acceleration * num5;
        vector2.x = Mathf.Clamp(vector2.x, -num6, num6);
        vector2.z = Mathf.Clamp(vector2.z, -num6, num6);
        Vector3 vector3 = __instance._transform.TransformDirection(vector2);
        vector3 -= Vector3.Project(vector3, __instance._groundNormal);
        __instance._owRigidbody.AddVelocityChange(vector3);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(JetpackThrusterModel), nameof(JetpackThrusterModel.FireTranslationalThrusters))]
    private static bool JetpackThrusterModel_FireTranslationalThrusters(JetpackThrusterModel __instance)
    {
        if (Config.UseScaledPlayerSpeed == false) return true;

        float y = __instance._translationalInput.y * __instance._maxTranslationalThrust;
        if (__instance._boostActivated)
        {
            float num = 0.06666667f - Time.fixedDeltaTime * 0.5f;
            float num2 = 0f;
            if (Time.time < __instance._lastJumpTime + num)
            {
                y = 0f;
            }
            else if (Locator.GetPlayerRulesetDetector().IsJetpackBoosterNerfed(out num2))
            {
                float num3 = Mathf.InverseLerp(__instance._lastJumpTime + num, __instance._lastJumpTime + num2, Time.time);
                y = __instance._boostThrust * num3;
            }
            else
            {
                y = __instance._boostThrust;
            }
            __instance._boostChargeFraction -= Time.deltaTime / __instance._boostSeconds;
            __instance._boostChargeFraction = Mathf.Clamp01(__instance._boostChargeFraction);
            if (__instance._boostChargeFraction == 0f)
            {
                __instance._boostActivated = false;
                RumbleManager.StopJetpackBoost();
            }
        }
        else
        {
            __instance._boostChargeFraction += Time.deltaTime / __instance._chargeSeconds;
            __instance._boostChargeFraction = Mathf.Clamp01(__instance._boostChargeFraction);
        }

        __instance._localAcceleration = new Vector3(__instance._translationalInput.x * __instance._maxTranslationalThrust, y, __instance._translationalInput.z * __instance._maxTranslationalThrust);
        __instance._isTranslationalFiring = (__instance._localAcceleration.magnitude > 0f);
        if (__instance._isTranslationalFiring)
        {
            __instance._owRigidbody.AddLocalAcceleration(__instance._localAcceleration * Instance.Scale);
            if (Locator.GetPlayerRulesetDetector().GetRingRiverRuleset() != null)
            {
                Vector3 localAcceleration = Locator.GetPlayerRulesetDetector().GetRingRiverRuleset().CalculateJetpackCounterAcceleration(__instance._localAcceleration, __instance.transform, __instance._owRigidbody);
                __instance._owRigidbody.AddLocalAcceleration(localAcceleration * Instance.Scale);
            }
        }

        return false;
    }

    // this prefix is added manually if Hiker's Mod is not installed
    internal static bool DreamLanternItem_OverrideMaxRunSpeed(ref float maxSpeedX, ref float maxSpeedZ, DreamLanternItem __instance)
    {
        if (!Config.UseScaledPlayerSpeed) return true;

        float lerpPosition = 1f - __instance._lanternController.GetFocus();
        lerpPosition *= lerpPosition;
        maxSpeedX = Mathf.Lerp(2f * Instance.Scale, maxSpeedX, lerpPosition);
        maxSpeedZ = Mathf.Lerp(2f * Instance.Scale, maxSpeedZ, lerpPosition);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerBody), nameof(PlayerBody.Awake))]
    private static void AddToPlayerBody(PlayerBody __instance)
    {
        PlayerScaleController scaleController = __instance.gameObject.AddComponent<PlayerScaleController>();
        // fire on the next update to avoid breaking things
        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            if (Config.UseCustomPlayerScale)
            {
                scaleController.Scale = Config.PlayerScale;
            }
            else
            {
                scaleController.Scale = DefaultScale;
            }

            __instance.transform.position += __instance.GetLocalUpDirection() * (-1 + Instance.Scale);
            ModMain.HikersModAPI?.UpdateConfig();
        });
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.OnPressInteract))]
    private static void EditShipCockpitAttachPoint(ShipCockpitController __instance)
    {
        __instance._origAttachPointLocalPos = new Vector3(0, 0.3353f + 0.8496f * (1 - Instance.TargetScale), 4.2307f + 0.15f * (1 - Instance.TargetScale));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Campfire), nameof(Campfire.StartRoasting))]
    private static void EditCampfireAttachOffset(Campfire __instance)
    {
        __instance._attachPoint._attachOffset *= 0.5f * (1 + Instance.TargetScale);
        __instance._attachPoint._attachOffset.y = Instance.TargetScale;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
    private static void EditGhostAttachOffset(GhostGrabController __instance)
    {
        __instance._attachPoint._attachOffset = new Vector3(0, 0.8496f * (1 - Instance.Scale), 0.15f * (1 - Instance.Scale));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RemoteFlightConsole), nameof(RemoteFlightConsole.OnPressInteract))]
    private static void EditModelRocketAttachOffset(RemoteFlightConsole __instance)
    {
        __instance._attachPoint._attachOffset.y = -1 + Instance.TargetScale;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Elevator), nameof(Elevator.OnPressInteract))]
    private static void EditElevatorAttachOffset(Elevator __instance)
    {
        __instance._attachPoint._attachOffset.y = -1 + Instance.TargetScale;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipLogController), nameof(ShipLogController.OnPressInteract))]
    private static void EditShipLogAttachOffset(ShipLogController __instance)
    {
        __instance._attachPoint._attachOffset = new Vector3(0, 0.8496f * (1 - Instance.TargetScale), 0.15f * (1 - Instance.TargetScale));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StationaryProbeLauncher), nameof(StationaryProbeLauncher.OnPressInteract))]
    private static void EditStationaryProbeLauncherAttachOffset(StationaryProbeLauncher __instance)
    {
        __instance._attachPoint._attachOffset = new Vector3(0, 0.8496f * (1 - Instance.TargetScale), 0.15f * (1 - Instance.TargetScale));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    private static void SetRunAnimFloats(PlayerAnimController __instance)
    {
        // scale run speed anim
        Vector3 groundVelocity = Vector3.zero;
        if (!PlayerState.IsAttached())
        {
            groundVelocity = Locator.GetPlayerController().GetRelativeGroundVelocity();
        }

        if (Mathf.Abs(groundVelocity.x) < 0.05f)
        {
            groundVelocity.x = 0f;
        }

        if (Mathf.Abs(groundVelocity.z) < 0.05f)
        {
            groundVelocity.z = 0f;
        }
        __instance._animator.SetFloat("RunSpeedX", groundVelocity.x / (3f * Instance.Scale));
        __instance._animator.SetFloat("RunSpeedY", groundVelocity.z / (3f * Instance.Scale));

        // scale freefall speed anim
        __instance._animator.SetFloat("FreefallSpeed", __instance._animator.GetFloat("FreefallSpeed") / Instance.Scale);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.GetMinImpactSpeed))]
    [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.GetMaxImpactSpeed))]
    private static void GetImpactSpeed(ref float __result)
    {
        if (Config.UseScaledPlayerDamage)
        {
            __result *= Mathf.Max(Instance.Scale, Instance.TargetScale);
        }
    }
}

/*      
 *      
 *  ISSUES
 *  - Footstep particles stay huge when you shrink back down (may have fixed itself??? be on lookout) (nope...nevermind. rare)
 *  - flashlight distance doesn't scale
 *  - maybe i should reduce wind volume when big?
 *  
 */