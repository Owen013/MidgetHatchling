namespace SmolHatchling
{
    public class StoolItem : OWItem
    {
        public override string GetDisplayName()
        {
            return "Stool";
        }

        public override void Awake()
        {
            _type = (ItemType)256;
            base.Awake();
        }
    }
}