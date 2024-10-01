namespace SmolHatchling.Components;

public class AnglerfishScaleController : ScaleController
{
    private AnglerfishController _anglerfishController;

    protected override void Awake()
    {
        base.Awake();
        _anglerfishController = GetComponent<AnglerfishController>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _anglerfishController._acceleration = 40 * Scale;
        _anglerfishController._chaseSpeed = 75 * Scale;
        _anglerfishController._investigateSpeed = 20 * Scale;
    }
}