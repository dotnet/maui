namespace VisualTestUtils
{
    public class ImagePercentageDifference : ImageDifference
    {
        double percentage;

        public ImagePercentageDifference(double percentage)
        {
            this.percentage = percentage;
        }

        public override string Description =>
            string.Format("{0:0.00}% difference", this.percentage * 100.0);
    }
}
