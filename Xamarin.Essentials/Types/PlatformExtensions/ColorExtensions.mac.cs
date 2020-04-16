using System.Drawing;
using AppKit;

namespace Xamarin.Essentials
{
    public static partial class ColorExtensions
    {
        public static Color ToSystemColor(this NSColor color)
        {
            if (color == null)
                throw new ArgumentNullException(nameof(color));

            color.GetRgba(out var red, out var green, out var blue, out var alpha);
            return Color.FromArgb((int)(alpha * 255), (int)(red * 255), (int)(green * 255), (int)(blue * 255));
        }

        public static NSColor ToPlatformColor(this Color color) =>
            NSColor.FromRgba(color.R, color.G, color.B, color.A);
    }
}
