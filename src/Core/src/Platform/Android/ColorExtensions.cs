using Android.Content;
using Android.Content.Res;
using AndroidX.Core.Content;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Android;
using Microsoft.Maui.Graphics.Native;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui
{
	public static class ColorExtensions
	{
		public static readonly int[][] States = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		public static AColor ToNative(this Color self) => self.AsColor();

		public static AColor ToNative(this Color self, int defaultColorResourceId, Context context)
			=> self?.ToNative() ?? new AColor(ContextCompat.GetColor(context, defaultColorResourceId));

		public static AColor ToNative(this Color? self, Color defaultColor)
			=> self?.ToNative() ?? defaultColor.ToNative();

		public static ColorStateList ToAndroidPreserveDisabled(this Color color, ColorStateList defaults)
		{
			int disabled = defaults.GetColorForState(States[1], color.ToNative());
			return new ColorStateList(States, new[] { color.ToNative().ToArgb(), disabled });
		}

		public static Color ToColor(this uint color)
		{
			return Color.FromUint(color);
		}

		public static Color ToColor(this AColor color)
		{
			return Color.FromUint((uint)color.ToArgb());
		}

		public static ColorStateList ToDefaultColorStateList(this Color color)
		{
			return new ColorStateList(
				new int[][] { new int[0] },
				new[] { color.ToNative().ToArgb() }
			);
		}
	}
}