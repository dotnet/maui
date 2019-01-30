// once we implement material configurations on core elements this can all be moved up to that
// for now just leaving this as an internal class to make matching colors between two platforms easier

#if __ANDROID__
using Android.Content.Res;
using AColor = Android.Graphics.Color;
#else
using MaterialComponents;
using AColor = UIKit.UIColor;
#endif

#if __ANDROID__
namespace Xamarin.Forms.Platform.Android.Material
#else
namespace Xamarin.Forms.Platform.iOS.Material
#endif
{
	// Colors from material-components-android
	// https://github.com/material-components/material-components-android/blob/3637c23078afc909e42833fd1c5fd47bb3271b5f/lib/java/com/google/android/material/color/res/values/colors.xml
	internal static class MaterialColors
	{
		// values based on
		// copying to match iOS
		// TODO generalize into xplat classes
		// https://github.com/material-components/material-components-ios/blob/develop/components/TextFields/src/ColorThemer/MDCFilledTextFieldColorThemer.m		
		const float kFilledTextFieldActiveAlpha = 0.87f;
		const float kFilledTextFieldOnSurfaceAlpha = 0.6f;
		const float kFilledTextFieldDisabledAlpha = 0.38f;
		const float kFilledTextFieldSurfaceOverlayAlpha = 0.04f;
		const float kFilledTextFieldIndicatorLineAlpha = 0.42f;
		const float kFilledTextFieldIconAlpha = 0.54f;

		// the idea of this value is that I want Active to be the exact color the user specified
		// and then all the other colors decrease according to the Material theme setup
		static float kFilledPlaceHolderOffset = 1f - kFilledTextFieldActiveAlpha;

		// State list from material-components-android
		// https://github.com/material-components/material-components-android/blob/71694616056012fe1162adb9144be903d1e510d5/lib/java/com/google/android/material/textfield/res/values/colors.xml#L28
		public static AColor CreateEntryFilledInputBackgroundColor(Color backgroundColor, Color textColor)
		{
			var platformTextColor = GetEntryTextColor(textColor);

			if (backgroundColor == Color.Default)
			{
				if (textColor != Color.Default)
					return WithAlpha(platformTextColor, 0.0392f);
				else
					return WithAlpha(MaterialColors.Light.PrimaryColorVariant, 0.0392f);
			}

			return ToPlatformColor(backgroundColor);
		}

		public static (AColor InlineColor, AColor FloatingColor) GetPlaceHolderColor(Color placeholderColor, Color textColor)
		{
			AColor color;

			if (placeholderColor == Color.Default)
			{
				if (textColor == Color.Default)
					color = MaterialColors.Light.OnSurfaceColor;
				else
					color = ToPlatformColor(textColor);
			}
			else
				color = ToPlatformColor(placeholderColor);

			var inlineColor = WithAlpha(color, kFilledTextFieldOnSurfaceAlpha + kFilledPlaceHolderOffset);
			var floatingColor = WithAlpha(color, kFilledTextFieldActiveAlpha + kFilledPlaceHolderOffset);

			return (inlineColor, floatingColor);
		}

		public static (AColor FocusedColor, AColor UnFocusedColor) GetUnderlineColor(Color textColor)
		{
			AColor color = GetEntryTextColor(textColor);
			return (color, WithAlpha(color, kFilledTextFieldIndicatorLineAlpha));
		}

		public static AColor GetEntryTextColor(Color textColor)
		{
			return textColor != Color.Default ? ToPlatformColor(textColor) : MaterialColors.Light.PrimaryColor;
		}

#if __ANDROID__
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

		public static ColorStateList CreateEntryFilledPlaceholderColors(AColor inlineColor, AColor floatingColor)
		{
			int[][] States =
			{
				new []{ global::Android.Resource.Attribute.StateEnabled, global::Android.Resource.Attribute.StatePressed  },
				new int[0] { }
			};

			var colors = new int[] { floatingColor, inlineColor };
			return new ColorStateList(States, colors);
		}

		public static ColorStateList CreateEntryUnderlineColors(AColor focusedColor, AColor unfocusedColor)
		{
			var colors = new int[] { focusedColor, unfocusedColor };
			return new ColorStateList(EntryUnderlineStates, colors);
		}

		internal static AColor WithAlpha(this AColor color, double alpha) =>
			new AColor(color.R, color.G, color.B, (byte)(alpha * 255));
#endif


		public static class Light
		{
			// the Colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly AColor PrimaryColor = FromRgb(33, 33, 33);
			public static readonly AColor PrimaryColorVariant = AColor.Black;
			public static readonly AColor OnPrimaryColor = AColor.White;
			public static readonly AColor SecondaryColor = FromRgb(33, 33, 33);
			public static readonly AColor OnSecondaryColor = AColor.White;

			// the Colors for "UI"
			public static readonly AColor BackgroundColor = AColor.White;
			public static readonly AColor OnBackgroundColor = AColor.Black;
			public static readonly AColor SurfaceColor = AColor.White;
			public static readonly AColor OnSurfaceColor = AColor.Black;
			public static readonly AColor ErrorColor = FromRgb(176, 0, 32);
			public static readonly AColor OnErrorColor = AColor.White;

#if __IOS__
			public static SemanticColorScheme CreateColorScheme()
			{
				return new SemanticColorScheme
				{
					PrimaryColor = PrimaryColor,
					PrimaryColorVariant = PrimaryColorVariant,
					SecondaryColor = SecondaryColor,
					OnPrimaryColor = OnPrimaryColor,
					OnSecondaryColor = OnSecondaryColor,

					BackgroundColor = BackgroundColor,
					ErrorColor = ErrorColor,
					SurfaceColor = SurfaceColor,
					OnBackgroundColor = OnBackgroundColor,
					OnSurfaceColor = OnSurfaceColor,
				};
			}
#endif

		}

		public static class Dark
		{
			// the Colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly AColor PrimaryColor = FromRgb(33, 33, 33);
			public static readonly AColor PrimaryColorVariant = AColor.Black;
			public static readonly AColor OnPrimaryColor = AColor.White;
			public static readonly AColor SecondaryColor = FromRgb(33, 33, 33);
			public static readonly AColor OnSecondaryColor = AColor.White;

			// the Colors for "UI"
			public static readonly AColor BackgroundColor = FromRgb(20, 20, 20);
			public static readonly AColor OnBackgroundColor = AColor.White;
			public static readonly AColor SurfaceColor = FromRgb(40, 40, 40);
			public static readonly AColor OnSurfaceColor = AColor.White;
			public static readonly AColor ErrorColor = FromRgb(194, 108, 122);
			public static readonly AColor OnErrorColor = AColor.White;

#if __IOS__
			public static SemanticColorScheme CreateColorScheme()
			{
				return new SemanticColorScheme
				{
					PrimaryColor = PrimaryColor,
					PrimaryColorVariant = PrimaryColorVariant,
					SecondaryColor = SecondaryColor,
					OnPrimaryColor = OnPrimaryColor,
					OnSecondaryColor = OnSecondaryColor,

					BackgroundColor = BackgroundColor,
					ErrorColor = ErrorColor,
					SurfaceColor = SurfaceColor,
					OnBackgroundColor = OnBackgroundColor,
					OnSurfaceColor = OnSurfaceColor,
				};
			}
#endif

		}


		static AColor ToPlatformColor(Color color)
		{
#if __ANDROID__
			return color.ToAndroid();
#else
			return color.ToUIColor();
#endif
		}



		static AColor WithAlpha(AColor color, float alpha)
		{
#if __ANDROID__
			return color.WithAlpha(alpha);
#else
			return color.ColorWithAlpha(alpha);
#endif
		}


		static AColor FromRgb(int red, int green, int blue)
		{
#if __ANDROID__
			return AColor.Rgb(red, green, blue);
#else
			return AColor.FromRGB(red, green, blue);

#endif
		}
	}
}
