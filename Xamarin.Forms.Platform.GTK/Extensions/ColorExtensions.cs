namespace Xamarin.Forms.Platform.GTK.Extensions
{
    public static class ColorExtensions
    {
        public static Gdk.Color ToGtkColor(this Color color)
        {
            string hex = color.ToRgbaColor();
            Gdk.Color gtkColor = new Gdk.Color();
            Gdk.Color.Parse(hex, ref gtkColor);

            return gtkColor;
        }

        internal static string ToRgbaColor(this Color color)
        {
            int red = (int)(color.R * 255);
            int green = (int)(color.G * 255);
            int blue = (int)(color.B * 255);

            return string.Format("#{0:X2}{1:X2}{2:X2}", red, green, blue);
        }

        internal static bool IsDefaultOrTransparent(this Color color)
        {
            return color == Color.Transparent || color == Color.Default;
        }
    }
}