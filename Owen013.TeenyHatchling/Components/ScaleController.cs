using UnityEngine;

namespace ScaleManipulator.Components;

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
        }
    }
}