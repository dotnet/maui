namespace System.Graphics
{
    public static class FontFamilyExtensions
    {
        public static IFontStyle GetBoldStyle(
            this IFontFamily family,
            FontStyleType type = FontStyleType.Normal)
        {
            return family?.GetStyleWithWeightNearestTo(500, type);
        }

        public static IFontStyle GetStyleWithWeightNearestTo(
            this IFontFamily family,
            int weight,
            FontStyleType type = FontStyleType.Normal)
        {
            if (family == null)
                return null;

            IFontStyle closest = null;
            var closestDifference = int.MaxValue;

            foreach (var font in family.GetFontStyles())
            {
                if (font.StyleType == type)
                {
                    var difference = Math.Abs(font.Weight - weight);
                    if (difference == 0)
                        return font;

                    if (difference < closestDifference)
                    {
                        closest = font;
                        closestDifference = difference;
                    }
                }
            }

            return closest;
        }
    }
}