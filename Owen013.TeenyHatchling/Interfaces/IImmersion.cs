using UnityEngine;

namespace ScaleManipulator.Interfaces;

public interface IImmersion
{
    public float GetAnimSpeed();

    public GameObject NewViewmodelArm(Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, bool useDefaultShader = false);
}