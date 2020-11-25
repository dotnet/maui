using System.Windows.Media;

namespace System.Graphics.Text
{
    public static class ColorExtensions
    {
        public static Windows.Media.Color? ToColor(
            this int[] color)
        {
            if (color == null)
                return null;

            var red = (byte) color[0];
            var green = (byte) color[1];
            var blue = (byte) color[2];
            var alpha = (byte) color[3];

            return Windows.Media.Color.FromArgb(alpha, red, green, blue);
        }
    }
}