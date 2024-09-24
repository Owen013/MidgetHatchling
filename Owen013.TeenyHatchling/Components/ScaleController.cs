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
        }
    }
}