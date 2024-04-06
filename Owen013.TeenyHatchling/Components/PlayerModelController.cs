using UnityEngine;

namespace SmolHatchling.Components;

public class PlayerModelController : MonoBehaviour
{
    public static float AnimSpeed { get; private set; }

    private PlayerAnimController _animController;

    private void Start()
    {
        _animController = GetComponent<PlayerAnimController>();
        ScaleController.Instance.OnUpdateScale += UpdatePlayerModel;
        UpdatePlayerModel();
    }

    private void OnDestroy()
    {
        ScaleController.Instance.OnUpdateScale -= UpdatePlayerModel;
    }

    private void UpdatePlayerModel()
    {
        transform.localScale = ScaleController.Instance.CurrentScale * 0.1f;
        transform.localPosition = new Vector3(0, -1.03f, -0.2f * ScaleController.Instance.CurrentScale.z);

        if (!ModMain.Instance.IsHikersModInstalled)
        {
            _animController._animator.speed = ScaleController.Instance.AnimSpeed;
        }
    }
}