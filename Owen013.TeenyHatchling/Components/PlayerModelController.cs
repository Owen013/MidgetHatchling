using UnityEngine;

namespace SmolHatchling.Components;

public class PlayerModelController : MonoBehaviour
{
    public static float AnimSpeed { get; private set; }

    private PlayerCharacterController _characterController;
    private PlayerAnimController _animController;

    private void Start()
    {
        _characterController = Locator.GetPlayerController();
        _animController = GetComponent<PlayerAnimController>();
    }

    private void LateUpdate()
    {
        transform.localScale = ScaleController.Instance.CurrentScale * 0.1f;
        transform.localPosition = new Vector3(0, -1.03f, -0.2f * ScaleController.Instance.CurrentScale.z);

        AnimSpeed = 1f / ScaleController.Instance.TargetScale.z;

        // yield to Hiker's Mod or Immersion if they are installed
        if (!ModMain.Instance.IsHikersModInstalled)
        {
            AnimSpeed = Mathf.Max(Mathf.Sqrt(_characterController.GetRelativeGroundVelocity().magnitude * AnimSpeed / 6f), 1f);
            if (ModMain.Instance.ImmersionAPI == null)
            {
                _animController._animator.speed = AnimSpeed;
            }
        }
    }
}