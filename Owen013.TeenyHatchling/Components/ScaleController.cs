using UnityEngine;

namespace SmolHatchling.Components;

public class ScaleController : MonoBehaviour
{
    public virtual float scale
    {
        get
        {
            return gameObject.transform.localScale.x;
        }

        set
        {
            gameObject.transform.localScale = Vector3.one * value;
        }
    }
}