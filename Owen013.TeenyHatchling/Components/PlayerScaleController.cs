using HarmonyLib;
using UnityEngine;

namespace SmolHatchling.Components;

[HarmonyPatch]
public class PlayerScaleController : ScaleController
{
    public static PlayerScaleController Instance { get; private set; }

    public static float animSpeed { get; private set; }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.CastForGrounded))]
    private static bool PlayerCharacterController_CastForGrounded(PlayerCharacterController __instance)
    {
        if (ModMain.Instance.GetConfigSetting<float>("PlayerScale") == 1) return true;

        float num = Time.fixedDeltaTime * 60f;
        bool flag = __instance._fluidDetector.InFluidType(FluidVolume.Type.TRACTOR_BEAM) || __instance._fluidDetector.InFluidType(FluidVolume.Type.SAND) || __instance._fluidDetector.InFluidType(FluidVolume.Type.WATER);
        bool flag2 = __instance._groundSnappingEnabled && __instance._wasGrounded && !flag && __instance._jetpackModel.GetLocalAcceleration().y <= 0f && Time.time > __instance._lastJumpTime + 0.5f;
        float num2 = (flag2 ? 0.1f : 0.06f) * num;
        Vector3 localUpDirection = __instance._owRigidbody.GetLocalUpDirection();
        float num3 = 0.46f;
        float maxDistance = num2 + (1f - num3);
        float scale = __instance.GetComponent<PlayerScaleController>().scale;
        int num4 = Physics.SphereCastNonAlloc(__instance._owRigidbody.GetPosition(), num3 * scale, -localUpDirection, __instance._raycastHits, maxDistance * scale, OWLayerMask.groundMask, QueryTriggerInteraction.Ignore);
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
        __instance.gameObject.AddComponent<PlayerScaleController>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnStartLiftPlayer))]
    public static void GhostLiftedPlayer(GhostGrabController __instance)
    {
        // Offset attachment so that camera is where it normally is
        __instance._attachPoint._attachOffset = new Vector3(0, 0.8496f * (1 - Instance.scale), 0.15f * (1 - Instance.scale));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    public static void SetRunAnimFloats(PlayerAnimController __instance)
    {
        Vector3 vector = Vector3.zero;
        if (!PlayerState.IsAttached())
        {
            vector = Locator.GetPlayerController().GetRelativeGroundVelocity();
        }

        if (Mathf.Abs(vector.x) < 0.05f)
        {
            vector.x = 0f;
        }

        if (Mathf.Abs(vector.z) < 0.05f)
        {
            vector.z = 0f;
        }
        __instance._animator.SetFloat("RunSpeedX", vector.x / (3f * Instance.scale));
        __instance._animator.SetFloat("RunSpeedY", vector.z / (3f * Instance.scale));
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (ModMain.Instance.GetConfigSetting<bool>("UseCustomPlayerScale"))
        {
            scale = ModMain.Instance.GetConfigSetting<float>("PlayerScale");
        }

        Locator.GetPlayerCamera().nearClipPlane = 0.1f * scale;
    }

    private void LateUpdate()
    {
        animSpeed = 1f / Instance.scale;

        // yield to Hiker's Mod or Immersion if they are installed
        if (!ModMain.Instance.ModHelper.Interaction.ModExists("Owen013.MovementMod"))
        {
            animSpeed = Mathf.Max(Mathf.Sqrt(Locator.GetPlayerController().GetRelativeGroundVelocity().magnitude * animSpeed / 6f), 1f);
            if (!ModMain.Instance.ModHelper.Interaction.ModExists("Owen_013.FirstPersonPresence"))
            {
                Locator.GetPlayerController().GetComponentInChildren<Animator>().speed = animSpeed;
            }
        }
    }
}

/*      
 *      ISSUES
 *  - Footstep particles stay huge when you shrink back down
 *  - helmet HUD disappears at small player scale
 *  
 */