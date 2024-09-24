using UnityEngine;

namespace SmolHatchling.Components;

public class ScaleController : MonoBehaviour
{
    public virtual Vector3 scale
    {
        get
        {
            return gameObject.transform.localScale;
        }

        set
        {
            gameObject.transform.localScale = value;
        }
    }
}