using HarmonyLib;

namespace SmolHatchling.Components;

[HarmonyPatch]
public class AnglerfishScaleController : ScaleController
{
    public static float DefaultScale = 1;

    private AnglerfishController _anglerfishController;

    protected override void Awake()
    {
        base.Awake();
        _anglerfishController = GetComponent<AnglerfishController>();
    }

    protected override void FixedUpdate()
    {
        if (ModMain.UseOtherCustomScales && TargetScale != ModMain.CustomAnglerfishScale)
        {
            SetTargetScale(ModMain.CustomAnglerfishScale);
        }

        base.FixedUpdate();
        _anglerfishController._acceleration = 40 * Scale;
        _anglerfishController._chaseSpeed = 75 * Scale;
        _anglerfishController._investigateSpeed = 20 * Scale;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.Start))]
    private static void AddScaleControllerToAnglerfish(AnglerfishController __instance)
    {
        ScaleController scaleController = __instance.gameObject.AddComponent<AnglerfishScaleController>();
        // fire on the next update to avoid breaking things
        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            if (ModMain.UseOtherCustomScales)
            {
                scaleController.Scale = ModMain.CustomAnglerfishScale;
            }
            else
            {
                scaleController.Scale = DefaultScale;
            }
        });
    }
}