using Android.Content;
using Android.Content.Res;
using AndroidX.Core.Content;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Android;
using Microsoft.Maui.Graphics.Platform;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	public static class ColorExtensions
	{
		public static AColor ToPlatform(this Color self) => self.AsColor();

		public static AColor ToPlatform(this Color self, int defaultColorResourceId, Context context)
			=> self?.ToPlatform() ?? new AColor(ContextCompat.GetColor(context, defaultColorResourceId));

		public static AColor ToPlatform(this Color? self, Color defaultColor)
			=> self?.ToPlatform() ?? defaultColor.ToPlatform();

		public static Color ToColor(this uint color)
		{
			return Color.FromUint(color);
		}

		public static Color ToColor(this AColor color)
		{
			return Color.FromUint((uint)color.ToArgb());
		}

		public static ColorStateList ToDefaultColorStateList(this Color color) =>
			ColorStateListExtensions.CreateDefault(color.ToPlatform());
	}
}