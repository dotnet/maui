namespace Microsoft.Maui.Graphics
{
    public static class PatternExtensions
    {
        public static Paint AsPaint(this IPattern target)
        {
            return AsPaint(target, Colors.Black);
        }

        public static Paint AsPaint(this IPattern target, Color foregroundColor)
        {
            if (target != null)
            {
                var paint = new Paint
                {
                    Pattern = target,
                    ForegroundColor = foregroundColor,
                    BackgroundColor = null
                };
                return paint;
            }

            return null;
        }
    }
}
