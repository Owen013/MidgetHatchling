namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        public float GetAnimSpeed()
        {
            return SmolHatchlingController.Instance._animSpeed;
        }

        public void SetHikersModEnabled()
        {
            SmolHatchlingController.Instance._hikersModEnabled = true;
        }
    }
}