using UnityEngine;

namespace SmolHatchling.Components;

public class ScaleController : MonoBehaviour
{
    public virtual float Scale
    {
        get
        {
            return transform.localScale.x;
        }

        set
        {
            transform.localScale = Vector3.one * value;
            TargetScale = value;
        }
    }

    public float TargetScale { get; protected set; }

    public virtual void SetTargetScale(float scale)
    {
        TargetScale = scale;
    }

    protected virtual void Awake()
    {
        TargetScale = Scale;
    }

    protected virtual void FixedUpdate()
    {
        if (Scale != TargetScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.Lerp(transform.localScale, Vector3.one * TargetScale, 0.1f), Time.deltaTime * Scale);
            if (Mathf.Abs(Scale - TargetScale) < Scale * 0.005f) Scale = TargetScale;
        }
    }
}