// once we implement material configurations on core elements this can all be moved up to that
// for now just leaving this as an internal class to make matching colors between two platforms easier

#if __ANDROID__
using Android.Content.Res;
using Android.Graphics;
using AProgressBar = Android.Widget.ProgressBar;
using ASeekBar = Android.Widget.AbsSeekBar;
using PlatformColor = Android.Graphics.Color;
#else
using MaterialComponents;
using PlatformColor = UIKit.UIColor;
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
		// https://github.com/material-components/material-components-ios/blob/v76.0.0/components/Slider/src/ColorThemer/MDCSliderColorThemer.m#L21
		const float kSliderBaselineDisabledFillAlpha = 0.32f;
		const float kSliderBaselineEnabledBackgroundAlpha = 0.24f;
		const float kSliderBaselineDisabledBackgroundAlpha = 0.12f;
		const float kSliderBaselineEnabledTicksAlpha = 0.54f;
		const float kSliderBaselineDisabledTicksAlpha = 0.12f;

		public const float SliderTrackAlpha = kSliderBaselineEnabledBackgroundAlpha;

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
		public static PlatformColor CreateEntryFilledInputBackgroundColor(Color backgroundColor, Color textColor)
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

		public static (PlatformColor InlineColor, PlatformColor FloatingColor) GetPlaceHolderColor(Color placeholderColor, Color textColor)
		{
			PlatformColor color;

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

		public static (PlatformColor FocusedColor, PlatformColor UnFocusedColor) GetUnderlineColor(Color textColor)
		{
			PlatformColor color = GetEntryTextColor(textColor);
			return (color, WithAlpha(color, kFilledTextFieldIndicatorLineAlpha));
		}

		public static PlatformColor GetEntryTextColor(Color textColor)
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
		public static ColorStateList CreateButtonBackgroundColors(PlatformColor primary)
		{
			var colors = new int[] { primary, primary.WithAlpha(0.12) };
			return new ColorStateList(ButtonStates, colors);
		}

		// State list from material-components-android
		// https://github.com/material-components/material-components-android/blob/3637c23078afc909e42833fd1c5fd47bb3271b5f/lib/java/com/google/android/material/button/res/color/mtrl_btn_text_color_selector.xml
		public static ColorStateList CreateButtonTextColors(PlatformColor primary, PlatformColor text)
		{
			var colors = new int[] { text, primary.WithAlpha(0.38) };
			return new ColorStateList(ButtonStates, colors);
		}

		public static ColorStateList CreateEntryFilledPlaceholderColors(PlatformColor inlineColor, PlatformColor floatingColor)
		{
			int[][] States =
			{
				new []{ global::Android.Resource.Attribute.StateEnabled, global::Android.Resource.Attribute.StatePressed  },
				new int[0] { }
			};

			var colors = new int[] { floatingColor, inlineColor };
			return new ColorStateList(States, colors);
		}

		public static ColorStateList CreateEntryUnderlineColors(PlatformColor focusedColor, PlatformColor unfocusedColor)
		{
			var colors = new int[] { focusedColor, unfocusedColor };
			return new ColorStateList(EntryUnderlineStates, colors);
		}

		internal static PlatformColor WithAlpha(this PlatformColor color, double alpha) =>
			new PlatformColor(color.R, color.G, color.B, (byte)(alpha * 255));

		internal static void ApplySeekBarColors(this ASeekBar seekBar, Color progressColor, Color backgroundColor, Color thumbColor)
		{
			seekBar.ApplyProgressBarColors(progressColor, backgroundColor);

			if (thumbColor.IsDefault)
			{
				// reset everything to defaults
				seekBar.ThumbTintList = seekBar.ProgressTintList;
			}
			else
			{
				// handle the case where the thumb is set
				var thumb = thumbColor.ToAndroid();

				seekBar.ThumbTintList = ColorStateList.ValueOf(thumb);
			}
		}

		internal static void ApplyProgressBarColors(this AProgressBar progressBar, Color progressColor, Color backgroundColor)
		{
			PlatformColor defaultProgress = Dark.PrimaryColor;

			if (progressColor.IsDefault)
			{
				if (backgroundColor.IsDefault)
				{
					// reset everything to defaults
					progressBar.ProgressTintList = ColorStateList.ValueOf(defaultProgress);
					progressBar.ProgressBackgroundTintList = ColorStateList.ValueOf(defaultProgress);
					progressBar.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
				}
				else
				{
					// handle the case where only the background is set
					var background = backgroundColor.ToAndroid();

					progressBar.ProgressTintList = ColorStateList.ValueOf(defaultProgress);
					progressBar.ProgressBackgroundTintList = ColorStateList.ValueOf(background);
					progressBar.ProgressBackgroundTintMode = PorterDuff.Mode.SrcOver;
				}
			}
			else
			{
				if (backgroundColor.IsDefault)
				{
					// handle the case where only the progress is set
					var progress = progressColor.ToAndroid();

					progressBar.ProgressTintList = ColorStateList.ValueOf(progress);
					progressBar.ProgressBackgroundTintList = ColorStateList.ValueOf(progress);
					progressBar.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
				}
				else
				{
					// handle the case where both are set
					var background = backgroundColor.ToAndroid();
					var progress = progressColor.ToAndroid();

					progressBar.ProgressTintList = ColorStateList.ValueOf(progress);
					progressBar.ProgressBackgroundTintList = ColorStateList.ValueOf(background);
					progressBar.ProgressBackgroundTintMode = PorterDuff.Mode.SrcOver;
				}
			}
		}
#endif


		public static class Light
		{
			// the Colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly PlatformColor PrimaryColor = FromRgb(33, 33, 33);
			public static readonly PlatformColor PrimaryColorVariant = PlatformColor.Black;
			public static readonly PlatformColor OnPrimaryColor = PlatformColor.White;
			public static readonly PlatformColor SecondaryColor = FromRgb(33, 33, 33);
			public static readonly PlatformColor OnSecondaryColor = PlatformColor.White;

			// the Colors for "UI"
			public static readonly PlatformColor BackgroundColor = PlatformColor.White;
			public static readonly PlatformColor OnBackgroundColor = PlatformColor.Black;
			public static readonly PlatformColor SurfaceColor = PlatformColor.White;
			public static readonly PlatformColor OnSurfaceColor = PlatformColor.Black;
			public static readonly PlatformColor ErrorColor = FromRgb(176, 0, 32);
			public static readonly PlatformColor OnErrorColor = PlatformColor.White;

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
			public static readonly PlatformColor PrimaryColor = FromRgb(33, 33, 33);
			public static readonly PlatformColor PrimaryColorVariant = PlatformColor.Black;
			public static readonly PlatformColor OnPrimaryColor = PlatformColor.White;
			public static readonly PlatformColor SecondaryColor = FromRgb(33, 33, 33);
			public static readonly PlatformColor OnSecondaryColor = PlatformColor.White;

			// the Colors for "UI"
			public static readonly PlatformColor BackgroundColor = FromRgb(20, 20, 20);
			public static readonly PlatformColor OnBackgroundColor = PlatformColor.White;
			public static readonly PlatformColor SurfaceColor = FromRgb(40, 40, 40);
			public static readonly PlatformColor OnSurfaceColor = PlatformColor.White;
			public static readonly PlatformColor ErrorColor = FromRgb(194, 108, 122);
			public static readonly PlatformColor OnErrorColor = PlatformColor.White;

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


		static PlatformColor ToPlatformColor(Color color)
		{
#if __ANDROID__
			return color.ToAndroid();
#else
			return color.ToUIColor();
#endif
		}



		static PlatformColor WithMultipliedAlpha(PlatformColor color, float alpha)
		{
#if __ANDROID__
			return color.WithAlpha(color.A / 255f * alpha);
#else
			return color.ColorWithAlpha(color.CGColor.Alpha / 255f * alpha);
#endif
		}

		static PlatformColor WithAlpha(PlatformColor color, float alpha)
		{
#if __ANDROID__
			return color.WithAlpha(alpha);
#else
			return color.ColorWithAlpha(alpha);
#endif
		}


		static PlatformColor FromRgb(int red, int green, int blue)
		{
#if __ANDROID__
			return PlatformColor.Rgb(red, green, blue);
#else
			return PlatformColor.FromRGB(red, green, blue);

#endif
		}
	}
}
