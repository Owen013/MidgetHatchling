using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmolHatchling.Components;

[HarmonyPatch]
public class PlayerScaleController : ScaleController
{
    public static PlayerScaleController Instance { get; private set; }

    public static float AnimSpeed { get; private set; }

    public override float Scale
    {
        get
        {
            return transform.localScale.x;
        }
        set
        {
            transform.localScale = Vector3.one * value;
            TargetScale = value;
            Locator.GetPlayerCamera().nearClipPlane = Mathf.Min(0.1f, 0.1f * Scale);
            ModMain.HikersModAPI?.UpdateConfig();
        }
    }

    public float TargetScale { get; private set; }

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
        float scale = __instance.GetComponent<PlayerScaleController>().Scale;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(JetpackThrusterModel), nameof(JetpackThrusterModel.FireTranslationalThrusters))]
    private static bool JetpackThrusterModel_FireTranslationalThrusters(JetpackThrusterModel __instance)
    {
        if (ModMain.Instance.GetConfigSetting<bool>("ScalePlayerSpeed") == false) return true;

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerBody), nameof(PlayerBody.Awake))]
    private static void AddToPlayerBody(PlayerBody __instance)
    {
        PlayerScaleController scaleController = __instance.gameObject.AddComponent<PlayerScaleController>();
        // fire on the next update to avoid breaking things
        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            if (ModMain.Instance.GetConfigSetting<bool>("UseCustomPlayerScale"))
            {
                scaleController.Scale = ModMain.Instance.GetConfigSetting<float>("PlayerScale");
                __instance.transform.position += __instance.GetLocalUpDirection() * (-1 + 1 * Instance.Scale);
            }
            else
            {
                Instance.TargetScale = 1;
            }

            ModMain.HikersModAPI?.UpdateConfig();
        });
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
    public static void GhostLiftedPlayer(GhostGrabController __instance)
    {
        // Offset attachment so that camera is where it normally is
        __instance._attachPoint._attachOffset = new Vector3(0, 0.8496f * (1 - Instance.Scale), 0.15f * (1 - Instance.Scale));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    public static void SetRunAnimFloats(PlayerAnimController __instance)
    {
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
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.GetMinImpactSpeed))]
    [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.GetMaxImpactSpeed))]
    public static void GetImpactSpeed(ref float __result)
    {
        if (ModMain.Instance.GetConfigSetting<bool>("ScalePlayerImpacts"))
        {
            __result *= Instance.Scale;
        }
    }

    // this patch is added manually if Hiker's Mod is not installed
    public static bool OverrideMaxRunSpeed(ref float maxSpeedX, ref float maxSpeedZ, DreamLanternItem __instance)
    {
        if (!ModMain.Instance.GetConfigSetting<bool>("ScalePlayerSpeed")) return true;

        float lerpPosition = 1f - __instance._lanternController.GetFocus();
        lerpPosition *= lerpPosition;
        maxSpeedX = Mathf.Lerp(2f * Instance.Scale, maxSpeedX, lerpPosition);
        maxSpeedZ = Mathf.Lerp(2f * Instance.Scale, maxSpeedZ, lerpPosition);
        return false;
    }

    public void EaseToScale(float scale)
    {
        TargetScale = scale;
        ModMain.HikersModAPI?.UpdateConfig();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (ModMain.Instance.GetConfigSetting<bool>("UseCustomPlayerScale") && ModMain.Instance.GetConfigSetting<bool>("UseScaleHotkeys"))
        {
            if (Keyboard.current[Key.Comma].wasPressedThisFrame)
            {
                float newScale = ModMain.Instance.GetConfigSetting<float>("PlayerScale") / 2;
                ModMain.Instance.SetConfigSetting("PlayerScale", newScale);
                EaseToScale(newScale);
            }

            if (Keyboard.current[Key.Period].wasPressedThisFrame)
            {
                float newScale = ModMain.Instance.GetConfigSetting<float>("PlayerScale") * 2;
                ModMain.Instance.SetConfigSetting("PlayerScale", newScale);
                EaseToScale(newScale);
            }

            if (Keyboard.current[Key.Slash].wasPressedThisFrame)
            {
                float newScale = 1;
                ModMain.Instance.SetConfigSetting("PlayerScale", newScale);
                EaseToScale(newScale);
            }
        }

        if (ModMain.HikersModAPI == null)
        {
            PlayerCharacterController player = GetComponent<PlayerCharacterController>();
            JetpackThrusterModel jetpack = GetComponent<JetpackThrusterModel>();
            if (ModMain.Instance.GetConfigSetting<bool>("ScalePlayerSpeed"))
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

    private void FixedUpdate()
    {
        if (Scale != TargetScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.Lerp(transform.localScale, Vector3.one * TargetScale, 0.1f), Time.deltaTime * Scale);
            if (Mathf.Abs(Scale - TargetScale) < Scale * 0.005f) Scale = TargetScale;
            Locator.GetPlayerCamera().nearClipPlane = Mathf.Min(0.1f, 0.1f * Scale);
        }
    }

    private void LateUpdate()
    {
        AnimSpeed = 1f / Instance.Scale;
        if (ModMain.HikersModAPI == null)
        {
            AnimSpeed = Mathf.Max(Mathf.Sqrt(Locator.GetPlayerController().GetRelativeGroundVelocity().magnitude * AnimSpeed / 6f), 1f);
            if (!ModMain.Instance.ModHelper.Interaction.ModExists("Owen_013.FirstPersonPresence"))
            {
                Locator.GetPlayerController().GetComponentInChildren<Animator>().speed = AnimSpeed;
            }
        }
    }
}

/*      
 *      
 *  ISSUES
 *  - Footstep particles stay huge when you shrink back down
 *  - player jump curve slowdown doesn't scale
 *  - camera is in wrong place in most attach points
 *  - flashlight distance doesn't scale
 *  - freefall anim floats probably don't scale
 *  - can't slow walk at ~3+ scale
 *  - maybe i should reduce wind volume when big?
 *  
 */