using UnityEngine;

namespace SmolHatchling.Components;

public class StoolItem : OWItem
{
    private GameObject _realModel;
    private GameObject _dreamModel;
    private BoxCollider _collider;
    float _height;

    public override string GetDisplayName()
    {
        return "Stool";
    }

    public override void Awake()
    {
        _realModel = transform.Find("Real").gameObject;
        _dreamModel = transform.Find("Simulation").gameObject;
        _collider = GetComponent<BoxCollider>();
        _type = (ItemType)256;

        Config.OnConfigure += UpdateHeight;
        UpdateHeight();

        base.Awake();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Config.OnConfigure -= UpdateHeight;
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
        EnableInteraction(true);
        SetColliderActivation(true);
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        transform.localPosition = new Vector3(0.2f, -_height, 0.3f);
    }

    public override void SocketItem(Transform socketTransform, Sector sector)
    {
        base.SocketItem(socketTransform, sector);
        EnableInteraction(false);
        SetColliderActivation(true);

    }

    public float GetHeight()
    {
        return _height;
    }

    private void UpdateHeight()
    {
        if (Config.AutoScaleStools) _height = -ScaleController.Instance.TargetScale.y + 1;
        else _height = Config.StoolHeight;

        Vector3 stoolScale = new Vector3(0.5f, _height, 0.5f);

        _realModel.transform.localScale = stoolScale;
        _dreamModel.transform.localScale = stoolScale;
        _collider.size = new Vector3(0.875f, 1.8f * _height, 0.875f);
        _collider.center = new Vector3(0, 0.5f * _collider.size.y, 0);
        gameObject.SetActive(_height > 0);
    }
}