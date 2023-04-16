using UnityEngine;

namespace SmolHatchling
{
    public class StoolSocket : OWItemSocket
    {
        public override bool AcceptsItem(OWItem item)
        {
            return item.GetComponent<StoolItem>() != null;
        }

        public override void Awake()
        {
            _acceptableType = (ItemType)256;
            _socketTransform = transform;
            gameObject.layer = 21;

            if (_sector == null)
            {
                _sector = GetComponentInParent<Sector>();
            }
            if (_sector == null)
            {
                //Debug.LogError("Could not find Sector in OWItemSocket parents", this);
                Debug.Break();
            }
            if (_socketTransform.childCount > 0)
            {
                _socketedItem = _socketTransform.GetComponentInChildren<OWItem>();
            }
        }

        public StoolItem GetSocketedStoolItem()
        {
            return GetComponentInChildren<StoolItem>();
        }
    }
}