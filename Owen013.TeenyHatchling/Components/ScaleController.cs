using UnityEngine;

namespace SmolHatchling.Components;

public class ScaleController : MonoBehaviour
{
    public virtual float scale
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

    public float targetScale { get; protected set; }

    public virtual void EaseToScale(float scale)
    {
        targetScale = scale;
    }

    protected virtual void Awake()
    {
        targetScale = scale;
    }

    protected virtual void FixedUpdate()
    {
        if (scale != targetScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.Lerp(transform.localScale, Vector3.one * targetScale, 0.1f), Time.deltaTime * scale);
            if (Mathf.Abs(scale - targetScale) < scale * 0.005f) scale = targetScale;
        }
    }
}