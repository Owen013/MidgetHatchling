using UnityEngine;

namespace SmolHatchling
{
    public class StoolItem : OWItem
    {
        StoolController _stoolController;
        public GameObject _realModel;
        public GameObject _dreamModel;
        public BoxCollider _collider;
        float _height;

        //public StoolItem()
        //{
        //    _stoolController = StoolController.Instance;
        //    GameObject stool = Instantiate(_stoolController._models.LoadAsset<GameObject>("SH_Stool"));
        //    stool.transform.parent = gameObject.transform;
        //    stool.transform.DetachChildren();
        //    _realModel = transform.Find("Real").gameObject;
        //    _dreamModel = transform.Find("Simulation").gameObject;
        //    _collider = gameObject.AddComponent<BoxCollider>();
        //    StoolItem stoolItem = stool.AddComponent<StoolItem>();
        //    stool.name = $"SmolHatchlingStool_{_stoolController._stools.Count}";
        //    StoolController.Instance._stools.Add(stool);
        //    stoolItem._realModel.GetComponent<MeshRenderer>().material = _stoolController._hearthTexture;
        //    stoolItem._dreamModel.GetComponent<MeshRenderer>().material = _stoolController._simTexture;
        //    stoolItem._dreamModel.layer = 28;
        //    SetHeight(StoolController.Instance._stoolHeight);
        //}

        //public StoolItem(Material texture)
        //{
        //    _stoolController = StoolController.Instance;
        //    GameObject stool = Instantiate(_stoolController._models.LoadAsset<GameObject>("SH_Stool"));
        //    stool.transform.parent = gameObject.transform;
        //    stool.transform.DetachChildren();
        //    _realModel = transform.Find("Real").gameObject;
        //    _dreamModel = transform.Find("Simulation").gameObject;
        //    _collider = gameObject.AddComponent<BoxCollider>();
        //    StoolItem stoolItem = stool.AddComponent<StoolItem>();
        //    stool.name = $"SmolHatchlingStool_{_stoolController._stools.Count}";
        //    StoolController.Instance._stools.Add(stool);
        //    stoolItem._realModel.GetComponent<MeshRenderer>().material = texture;
        //    stoolItem._dreamModel.GetComponent<MeshRenderer>().material = _stoolController._simTexture;
        //    stoolItem._dreamModel.layer = 28;
        //    SetHeight(StoolController.Instance._stoolHeight);
        //}

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
            base.Awake();
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

        public void SetHeight(float height)
        {
            _height = height;
            _realModel.transform.localScale = _dreamModel.transform.localScale = new Vector3(0.5f, _height, 0.5f);
            _collider.size = new Vector3(0.875f, 1.8f * _height, 0.875f);
            _collider.center = new Vector3(0, 0.5f * _collider.size.y, 0);
            gameObject.SetActive(_height > 0);
        }
    }
}