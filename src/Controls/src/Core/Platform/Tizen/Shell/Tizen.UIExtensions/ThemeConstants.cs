using EColor = ElmSharp.Color;

namespace Tizen.UIExtensions.Shell
{
	public class ThemeConstants
	{
		public class Shell
		{
			public class ColorClass
			{
				public static readonly EColor DefaultBackgroundColor = EColor.FromRgb(33, 150, 243);
				public static readonly EColor DefaultForegroundColor = EColor.White;
				public static readonly EColor DefaultTitleColor = EColor.White;
			}

			public class Resources
			{
				// The source of icon resources is https://materialdesignicons.com/
				public const string MenuIcon = "Platform.Tizen.Resources.menu.png";
				public const string BackIcon = "Platform.Tizen.Resources.arrow_left.png";
				public const string DotsIcon = "Platform.Tizen.Resources.dots_horizontal.png";
			}
		}
	}
}
