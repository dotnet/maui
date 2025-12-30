namespace VisualTestUtils
{
    public class ImageSizeDifference : ImageDifference
    {
        int baselineWidth;
        int baselineHeight;
        int actualWidth;
        int actualHeight;

        public ImageSizeDifference(int baselineWidth, int baselineHeight, int actualWidth, int actualHeight)
        {
            this.baselineWidth = baselineWidth;
            this.baselineHeight = baselineHeight;
            this.actualWidth = actualWidth;
            this.actualHeight = actualHeight;
        }

        public override string Description =>
            $"size differs - baseline is {this.baselineWidth}x{this.baselineHeight} pixels, actual is {this.actualWidth}x{this.actualHeight} pixels";

        public static ImageSizeDifference Compare(int baselineWidth, int baselineHeight, int actualWidth, int actualHeight) =>
            baselineWidth != actualWidth || baselineHeight != actualHeight
                ? new ImageSizeDifference(baselineWidth, baselineHeight, actualWidth, actualHeight)
                : null;
    }
}
