using HarmonyLib;
using UnityEngine;

namespace ScaleManipulator.Components;

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
            targetScale = value;
        }
    }

    public float targetScale;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.CastForGrounded))]
    private static bool PlayerCharacterController_CastForGrounded(PlayerCharacterController __instance)
    {
        if (ModMain.Instance.GetConfigSetting<float>("PlayerScale") == 1) return true;

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
                __instance.transform.position += __instance.GetLocalUpDirection() * (-1 + 1 * scaleController.Scale);
            }
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
        __result *= Instance.Scale;
    }

    // this patch is added manually if Hiker's Mod is not installed
    public static bool OverrideMaxRunSpeed(ref float maxSpeedX, ref float maxSpeedZ, DreamLanternItem __instance)
    {
        if (!ModMain.Instance.GetConfigSetting<bool>("UseScaledPlayerAttributes")) return true;

        float lerpPosition = 1f - __instance._lanternController.GetFocus();
        lerpPosition *= lerpPosition;
        maxSpeedX = Mathf.Lerp(2f * Instance.Scale, maxSpeedX, lerpPosition);
        maxSpeedZ = Mathf.Lerp(2f * Instance.Scale, maxSpeedZ, lerpPosition);
        return false;
    }
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (ModMain.Instance.GetConfigSetting<bool>("UseCustomPlayerScale"))
        {
            targetScale = ModMain.Instance.GetConfigSetting<float>("PlayerScale");

            PlayerCharacterController player = GetComponent<PlayerCharacterController>();
            if (ModMain.Instance.GetConfigSetting<bool>("UseScaledPlayerAttributes") && Scale != 1)
            {
                player._runSpeed = 6f * Scale;
                player._strafeSpeed = 4f * Scale;
                player._walkSpeed = 3f * Scale;
                player._airSpeed = 3f * Scale;
                player._acceleration = 0.5f * Scale;
                player._airAcceleration = 0.09f * Scale;
                player._minJumpSpeed = 3f * Scale;
                player._maxJumpSpeed = 7f * Scale;
            }
            else
            {
                player._runSpeed = 6f;
                player._strafeSpeed = 4f;
                player._walkSpeed = 3f;
                player._airSpeed = 3f;
                player._acceleration = 0.5f;
                player._airAcceleration = 0.09f;
                player._minJumpSpeed = 3f;
                player._maxJumpSpeed = 7f;
            }
        }

        Locator.GetPlayerCamera().nearClipPlane = 0.1f * Scale;
    }

    private void FixedUpdate()
    {
        if (Scale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, 0.1f);
        }
    }

    private void LateUpdate()
    {
        AnimSpeed = Mathf.Max(Mathf.Sqrt(Locator.GetPlayerController().GetRelativeGroundVelocity().magnitude * (1f / Instance.Scale) / 6f), 1f);

        // yield to Hiker's Mod or Immersion if they are installed
        if (!ModMain.Instance.ModHelper.Interaction.ModExists("Owen013.MovementMod") && !ModMain.Instance.ModHelper.Interaction.ModExists("Owen_013.FirstPersonPresence"))
        {
            Locator.GetPlayerController().GetComponentInChildren<Animator>().speed = AnimSpeed;
        }
    }
}

/*      
 *  ISSUES
 *  - Footstep particles stay huge when you shrink back down
 *  - player jump curve slowdown doesn't scale
 *  - player jetpack power doesn't scale
 *  - camera is in wrong place in most attach points
 *  
 */