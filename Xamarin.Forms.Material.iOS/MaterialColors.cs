using MaterialComponents;
using UIKit;

namespace Xamarin.Forms.Platform.iOS.Material
{
	// colors from material-components-android
	// https://github.com/material-components/material-components-android/blob/3637c23078afc909e42833fd1c5fd47bb3271b5f/lib/java/com/google/android/material/color/res/values/colors.xml
	internal static class MaterialColors
	{
		public static class Light
		{
			// the colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly UIColor PrimaryColor = UIColor.FromRGB(33, 33, 33);
			public static readonly UIColor PrimaryColorVariant = UIColor.Black;
			public static readonly UIColor OnPrimaryColor = UIColor.White;
			public static readonly UIColor SecondaryColor = UIColor.FromRGB(33, 33, 33);
			public static readonly UIColor OnSecondaryColor = UIColor.White;

			// the colors for "UI"
			public static readonly UIColor BackgroundColor = UIColor.White;
			public static readonly UIColor OnBackgroundColor = UIColor.Black;
			public static readonly UIColor SurfaceColor = UIColor.White;
			public static readonly UIColor OnSurfaceColor = UIColor.Black;
			public static readonly UIColor ErrorColor = UIColor.FromRGB(176, 0, 32);
			public static readonly UIColor OnErrorColor = UIColor.White;

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
		}

		public static class Dark
		{
			// the colors for "branding"
			//  - we selected the "black" theme from the default DarkActionBar theme
			public static readonly UIColor PrimaryColor = UIColor.FromRGB(33, 33, 33);
			public static readonly UIColor PrimaryColorVariant = UIColor.Black;
			public static readonly UIColor OnPrimaryColor = UIColor.White;
			public static readonly UIColor SecondaryColor = UIColor.FromRGB(33, 33, 33);
			public static readonly UIColor OnSecondaryColor = UIColor.White;

			// the colors for "UI"
			public static readonly UIColor BackgroundColor = UIColor.FromRGB(20, 20, 20);
			public static readonly UIColor OnBackgroundColor = UIColor.White;
			public static readonly UIColor SurfaceColor = UIColor.FromRGB(40, 40, 40);
			public static readonly UIColor OnSurfaceColor = UIColor.White;
			public static readonly UIColor ErrorColor = UIColor.FromRGB(194, 108, 122);
			public static readonly UIColor OnErrorColor = UIColor.White;

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
		}
	}
}
