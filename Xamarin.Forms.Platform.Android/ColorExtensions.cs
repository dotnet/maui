using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
#if __ANDROID_29__
using AndroidX.Core.Content;
#else
using Android.Support.V4.Content;
#endif

using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	public static class ColorExtensions
	{
		public static readonly int[][] States = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		public static AColor ToAndroid(this Color self)
		{
			return new AColor((byte)(byte.MaxValue * self.R), (byte)(byte.MaxValue * self.G), (byte)(byte.MaxValue * self.B), (byte)(byte.MaxValue * self.A));
		}

		[Obsolete("ToAndroid(this Color, int) is obsolete as of version 2.5. Please use ToAndroid(this Color, int, Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static AColor ToAndroid(this Color self, int defaultColorResourceId)
		{
			if (self == Color.Default)
			{
				return new AColor(ContextCompat.GetColor(Forms.Context, defaultColorResourceId));
			}

			return ToAndroid(self);
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
	}
}