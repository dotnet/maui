using Microsoft.Maui.Graphics;
using AndroidColor = Android.Graphics.Color;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToMauiColor(this AndroidColor color) =>
			Color.FromRgba(color.R, color.G, color.B, color.A);

		public static AndroidColor ToPlatformColor(this Color color) =>
			new AndroidColor(color.ToInt());
	}
}
