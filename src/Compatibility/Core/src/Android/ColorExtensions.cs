using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using AndroidX.Core.Content;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class ColorExtensions
	{
		public static readonly int[][] States = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		public static AColor ToAndroid(this Color self)
		{
			return new AColor((byte)(byte.MaxValue * self.Red), (byte)(byte.MaxValue * self.Green), (byte)(byte.MaxValue * self.Blue), (byte)(byte.MaxValue * self.Alpha));
		}

		public static AColor ToAndroid(this Color self, int defaultColorResourceId, Context context)
		{
			if (self == null)
			{
				return new AColor(ContextCompat.GetColor(context, defaultColorResourceId));
			}

			return ToAndroid(self);
		}

		public static AColor ToAndroid(this Color self, Color defaultColor)
		{
			if (self == null)
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