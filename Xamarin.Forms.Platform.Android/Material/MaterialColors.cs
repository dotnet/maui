using Android.Content.Res;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android.Material
{
	// Colors from material-components-android
	// https://github.com/material-components/material-components-android/blob/3637c23078afc909e42833fd1c5fd47bb3271b5f/lib/java/com/google/android/material/color/res/values/colors.xml
	internal static class MaterialColors
	{
		public static readonly int[][] ButtonStates =
		{
			new int[] { global::Android.Resource.Attribute.StateEnabled },
			new int[] { }
		};

		public static readonly int[][] EntryHintTextStates =
		{
			new []{ global::Android.Resource.Attribute.StateEnabled, global::Android.Resource.Attribute.StatePressed  },
			new int[0] { }
		};

		public static readonly int[][] EntryUnderlineStates =
		{
			new []{ global::Android.Resource.Attribute.StateFocused  },
			new []{ -global::Android.Resource.Attribute.StateFocused  },
		};

		// State list from material-components-android
		// https://github.com/material-components/material-components-android/blob/3637c23078afc909e42833fd1c5fd47bb3271b5f/lib/java/com/google/android/material/button/res/color/mtrl_btn_bg_color_selector.xml
		public static ColorStateList CreateButtonBackgroundColors(AColor primary)
		{
			var colors = new int[] { primary, primary.WithAlpha(0.12) };
			return new ColorStateList(ButtonStates, colors);
		}

		// State list from material-components-android
		// https://github.com/material-components/material-components-android/blob/3637c23078afc909e42833fd1c5fd47bb3271b5f/lib/java/com/google/android/material/button/res/color/mtrl_btn_text_color_selector.xml
		public static ColorStateList CreateButtonTextColors(AColor primary, AColor text)
		{
			var colors = new int[] { text, primary.WithAlpha(0.38) };
			return new ColorStateList(ButtonStates, colors);
		}

		// State list from material-components-android
		// https://github.com/material-components/material-components-android/blob/71694616056012fe1162adb9144be903d1e510d5/lib/java/com/google/android/material/textfield/res/values/colors.xml#L28
		public static int CreateEntryFilledInputBackgroundColor(AColor primary)
		{
			return primary.WithAlpha(0.0392);
		}

		public static ColorStateList CreateEntryFilledPlaceholderColors(AColor color)
		{
			int[][] States =
			{
				new []{ global::Android.Resource.Attribute.StateEnabled, global::Android.Resource.Attribute.StatePressed  },
				new int[0] { }
			};

			var colors = new int[] { color, color };
			return new ColorStateList(States, colors);
		}

		public static ColorStateList CreateEntryUnderlineColors(AColor focusedColor, AColor unfocusedColor)
		{
			var colors = new int[] { focusedColor, unfocusedColor };
			return new ColorStateList(EntryUnderlineStates, colors);
		}

		internal static AColor WithAlpha(this AColor color, double alpha) =>
			new AColor(color.R, color.G, color.B, (byte)(alpha * 255));

		public static class Light
		{
			// the Colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly AColor PrimaryColor = AColor.Rgb(33, 33, 33);
			public static readonly AColor PrimaryColorVariant = AColor.Black;
			public static readonly AColor OnPrimaryColor = AColor.White;
			public static readonly AColor SecondaryColor = AColor.Rgb(33, 33, 33);
			public static readonly AColor OnSecondaryColor = AColor.White;

			// the Colors for "UI"
			public static readonly AColor BackgroundColor = AColor.White;
			public static readonly AColor OnBackgroundColor = AColor.Black;
			public static readonly AColor SurfaceColor = AColor.White;
			public static readonly AColor OnSurfaceColor = AColor.Black;
			public static readonly AColor ErrorColor = AColor.Rgb(176, 0, 32);
			public static readonly AColor OnErrorColor = AColor.White;
		}

		public static class Dark
		{
			// the Colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly AColor PrimaryColor = AColor.Rgb(33, 33, 33);
			public static readonly AColor PrimaryColorVariant = AColor.Black;
			public static readonly AColor OnPrimaryColor = AColor.White;
			public static readonly AColor SecondaryColor = AColor.Rgb(33, 33, 33);
			public static readonly AColor OnSecondaryColor = AColor.White;

			// the Colors for "UI"
			public static readonly AColor BackgroundColor = AColor.Rgb(20, 20, 20);
			public static readonly AColor OnBackgroundColor = AColor.White;
			public static readonly AColor SurfaceColor = AColor.Rgb(40, 40, 40);
			public static readonly AColor OnSurfaceColor = AColor.White;
			public static readonly AColor ErrorColor = AColor.Rgb(194, 108, 122);
			public static readonly AColor OnErrorColor = AColor.White;
		}
	}
}
