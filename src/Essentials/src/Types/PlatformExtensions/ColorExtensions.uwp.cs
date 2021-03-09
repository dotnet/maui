using System.Drawing;
using WindowsColor = Windows.UI.Color;

namespace Microsoft.Maui.Essentials
{
    public static partial class ColorExtensions
    {
        public static Color ToSystemColor(this WindowsColor color) =>
            Color.FromArgb(color.A, color.R, color.G, color.B);

        public static WindowsColor ToPlatformColor(this Color color) =>
               WindowsColor.FromArgb(color.A, color.R, color.G, color.B);
    }
}
