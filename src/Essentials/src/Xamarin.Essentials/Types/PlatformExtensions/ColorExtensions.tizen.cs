using System.Drawing;
using EColor = ElmSharp.Color;

namespace Xamarin.Essentials
{
    public static partial class ColorExtensions
    {
        public static Color ToSystemColor(this EColor color) =>
            Color.FromArgb(color.A, color.R, color.G, color.B);

        public static EColor ToPlatformColor(this Color color) =>
            new EColor(color.R, color.G, color.B, color.A);
    }
}
