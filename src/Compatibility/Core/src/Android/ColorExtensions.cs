using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using AndroidX.Core.Content;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class ColorExtensions
	{
		public static readonly int[][] States = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		public static AColor ToAndroid(this Color self)
		{
			return new AColor((byte)(byte.MaxValue * self.R), (byte)(byte.MaxValue * self.G), (byte)(byte.MaxValue * self.B), (byte)(byte.MaxValue * self.A));
		}

		public static AColor ToAndroid(this Color self, int defaultColorResourceId, Context context)
		{
			if (self == Color.Default)
			{
				return new AColor(ContextCompat.GetColor(context, defaultColorResourceId));
			}

			return ToAndroid(self);
		}

		public static AColor ToAndroid(this Color self, Color defaultColor)
		{
			if (self == Color.Default)
				return defaultColor.ToAndroid();

			return ToAndroid(self);
		}

		public static ColorStateList ToAndroidPreserveDisabled(this Color color, ColorStateList defaults)
		{
			int disabled = defaults.GetColorForState(States[1], color.ToAndroid());
			return new ColorStateList(States, new[] { color.ToAndroid().ToArgb(), disabled });
		}

		public static Color ToColor(this AColor color)
		{
			return Color.FromUint((uint)color.ToArgb());
		}

		internal static ColorStateList ToDefaultColorStateList(this Color color)
		{
			return ToDefaultColorStateList(color.ToAndroid());
		}

		internal static ColorStateList ToDefaultColorStateList(int aColor)
		{
			int[][] States =
			{
				new int[0] { }
			};

			var colors = new int[] { aColor };
			return new ColorStateList(States, colors);
		}

		internal static ColorStateList ToDefaultOnlyColorStateList(this ColorStateList stateList)
		{
			return ToDefaultColorStateList(stateList.DefaultColor);
		}
	}
}