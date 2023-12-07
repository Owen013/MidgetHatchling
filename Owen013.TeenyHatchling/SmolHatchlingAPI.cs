namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        public float GetAnimSpeed()
        {
            return ModController.s_instance._animSpeed;
        }

        public void SetHikersModEnabled()
        {
            ModController.s_instance._hikersModEnabled = true;
        }
    }
}