using Android.Content;
using Android.Content.Res;
using AndroidX.Core.Content;
using Microsoft.Maui;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui
{
	public static class ColorExtensions
	{
		public static readonly int[][] States = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		public static AColor ToNative(this Color self)
		{
			return new AColor((byte)(byte.MaxValue * self.R), (byte)(byte.MaxValue * self.G), (byte)(byte.MaxValue * self.B), (byte)(byte.MaxValue * self.A));
		}

		public static AColor ToNative(this Color self, int defaultColorResourceId, Context context)
		{
			if (self == Color.Default)
			{
				return new AColor(ContextCompat.GetColor(context, defaultColorResourceId));
			}

			return ToNative(self);
		}

		public static AColor ToNative(this Color self, Color defaultColor)
		{
			if (self == Color.Default)
				return defaultColor.ToNative();

			return ToNative(self);
		}

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