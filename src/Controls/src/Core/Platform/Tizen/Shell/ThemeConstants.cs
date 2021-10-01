namespace Microsoft.Maui.Controls.Platform
{
	public class ThemeConstants
	{
		public class Shell
		{
			public class Resources
			{
				public const int DefaultMargin = 10;
				public const int DefaultNavBarHeight = 70;
				public const int DefaultMenuSize = 40;

				public const int DefaultTitleFontSize = 23;
				public const int DefaultTitleMargin = 23;

				public const int DefaultIconSize = 30;
				public const int DefaultIconPadding = 15;

				public const double DefaultFlyoutRatio = 0.83;
				public const int DefaultFlyoutItemHeight = 60;
				public const int DefaultFlyoutItemWidth = 250;

				public class TV
				{
					public const int DefaultMenuSize = 70;

					public const double DefaultFlyoutRatio = 0.3;
					public const double DefaultFlyoutRatioMin = 0.05;

					public const int DefaultFlyoutIconColumnSize = 40;
					public const int DefaultFlyoutIconSize = 25;

					public const int DefaultFlyoutItemfontSize = 25;
				}
			}

			public class ColorClass
			{
				public class TV
				{
					public static readonly Graphics.Color DefaultBackgroundColor = Graphics.Colors.Black;
					public static readonly Graphics.Color DefaultFlyoutItemColor = Graphics.Colors.Transparent;
					public static readonly Graphics.Color DefaultFlyoutItemFocusedColor = new Graphics.Color(0.95f);
				}
			}
		}
	}
}
