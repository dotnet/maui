namespace Microsoft.Maui.Graphics
{
    public static class FontUtils
    {
        public const int Thin = 100;
        public const int ExtraLight = 200;
        public const int UltraLight = 200;
        public const int Light = 300;
        public const int SemiLight = 400;
        public const int Normal = 400;
        public const int Regular = 400;
        public const int Medium = 500;
        public const int DemiBold = 600;
        public const int SemiBold = 600;
        public const int Bold = 700;
        public const int ExtraBold = 800;
        public const int UltraBold = 800;
        public const int Black = 900;
        public const int Heavy = 900;
        public const int ExtraBlack = 950;
        public const int UltraBlack = 950;

        public static int GetFontWeight(string styleName)
        {
            if (styleName == null) return Regular;

            if (styleName.Equals("Regular")) return Regular;
            if (styleName.Equals("Normal")) return Normal;
            if (styleName.Contains("Thin")) return Thin;
            if (styleName.Contains("Extra Light")) return ExtraLight;
            if (styleName.Contains("Ultra Light")) return UltraLight;
            if (styleName.Contains("Semi Light")) return SemiLight;
            if (styleName.Contains("Light")) return Light;
            if (styleName.Contains("Medium")) return Medium;
            if (styleName.Contains("Demi Bold")) return DemiBold;
            if (styleName.Contains("Semi Bold")) return SemiBold;
            if (styleName.Contains("Bold")) return Bold;
            if (styleName.Contains("Extra Bold")) return ExtraBold;
            if (styleName.Contains("Ultra Bold")) return UltraBold;
            if (styleName.Contains("Black")) return Black;
            if (styleName.Contains("Extra Black")) return ExtraBlack;
            if (styleName.Contains("Ultra Black")) return UltraBlack;

            return Regular;
        }

        public static FontStyleType GetStyleType(string styleName)
        {
            if (styleName == null) return FontStyleType.Normal;

            if (styleName.Contains("Italic")) return FontStyleType.Italic;
            if (styleName.Contains("Oblique")) return FontStyleType.Oblique;

            return FontStyleType.Normal;
        }
    }
}
