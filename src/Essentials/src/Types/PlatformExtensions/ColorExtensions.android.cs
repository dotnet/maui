using System.Drawing;
using AndroidColor = Android.Graphics.Color;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToSystemColor(this AndroidColor color) =>
			Color.FromArgb(color.A, color.R, color.G, color.B);

		public static AndroidColor ToPlatformColor(this Color color) =>
			new AndroidColor(color.R, color.G, color.B, color.A);
	}
}
