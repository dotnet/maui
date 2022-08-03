using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using ITNavigtaionView = Tizen.UIExtensions.ElmSharp.INavigationView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ThemeManager
	{
		#region ShellMoreTabs
		static double s_shellMoreToolBarIconPadding = -1;
		public static double GetDefaultIconPadding(this ShellMoreTabs self)
		{
			if (s_shellMoreToolBarIconPadding > 0)
				return s_shellMoreToolBarIconPadding;
			return s_shellMoreToolBarIconPadding = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultIconPadding);
		}

		static double s_shellMoreToolBarIconSize = -1;
		public static double GetDefaultIconSize(this ShellMoreTabs self)
		{
			if (s_shellMoreToolBarIconSize > 0)
				return s_shellMoreToolBarIconSize;
			return s_shellMoreToolBarIconSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultIconSize);
		}
		#endregion

		#region ShellNavBar
		static double s_shellNavBarDefaultHeight = -1;
		public static double GetDefaultNavBarHeight(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultHeight > 0)
				return s_shellNavBarDefaultHeight;
			return s_shellNavBarDefaultHeight = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultNavBarHeight);
		}

		static double s_shellNavBarDefaultMenuSize = -1;
		public static double GetDefaultMenuSize(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultMenuSize > 0)
				return s_shellNavBarDefaultMenuSize;
			return s_shellNavBarDefaultMenuSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(
				Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.TV ? ThemeConstants.Shell.Resources.TV.DefaultMenuSize : ThemeConstants.Shell.Resources.DefaultMenuSize);
		}

		static double s_shellNavBarDefaultMargin = -1;
		public static double GetDefaultMargin(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultMargin > 0)
				return s_shellNavBarDefaultMargin;
			return s_shellNavBarDefaultMargin = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultMargin);
		}

		static double s_shellNavBarTitleVDefaultMargin = -1;
		public static double GetDefaultTitleVMargin(this ShellNavBar navBar)
		{
			if (s_shellNavBarTitleVDefaultMargin > 0)
				return s_shellNavBarTitleVDefaultMargin;
			return s_shellNavBarTitleVDefaultMargin = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultTitleMargin);
		}

		static double s_shellNavBarTitleFontSize = -1;
		public static double GetDefaultTitleFontSize(this ShellNavBar navBar)
		{
			if (s_shellNavBarTitleFontSize > 0)
				return s_shellNavBarTitleFontSize;
			return s_shellNavBarTitleFontSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultTitleFontSize);
		}
		#endregion

		#region TVShell
		static double s_navigationViewFlyoutItemHeight = -1;
		public static double GetTvFlyoutItemHeight(this ITNavigtaionView nav)
		{
			if (s_navigationViewFlyoutItemHeight > 0)
				return s_navigationViewFlyoutItemHeight;
			return s_navigationViewFlyoutItemHeight = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultFlyoutItemHeight);
		}

		static double s_navigationViewFlyoutItemWidth = -1;
		public static double GetTvFlyoutItemWidth(this ITNavigtaionView nav)
		{
			if (s_navigationViewFlyoutItemWidth > 0)
				return s_navigationViewFlyoutItemWidth;
			return s_navigationViewFlyoutItemWidth = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultFlyoutItemWidth);
		}

		static double s_navigationViewFlyoutIconColumnSize = -1;
		public static double GetTvFlyoutIconColumnSize(this ITNavigtaionView nav)
		{
			if (s_navigationViewFlyoutIconColumnSize > 0)
				return s_navigationViewFlyoutIconColumnSize;
			return s_navigationViewFlyoutIconColumnSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.TV.DefaultFlyoutIconColumnSize);
		}

		static double s_navigationViewFlyoutIconSize = -1;
		public static double GetTvFlyoutIconSize(this ITNavigtaionView nav)
		{
			if (s_navigationViewFlyoutIconSize > 0)
				return s_navigationViewFlyoutIconSize;
			return s_navigationViewFlyoutIconSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.TV.DefaultFlyoutIconSize);
		}

		static double s_navigationViewFlyoutMargin = -1;
		public static double GetTvFlyoutMargin(this ITNavigtaionView nav)
		{
			if (s_navigationViewFlyoutMargin > 0)
				return s_navigationViewFlyoutMargin;
			return s_navigationViewFlyoutMargin = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.DefaultMargin);
		}

		static double s_navigationViewFlyoutItemFontSize = -1;
		public static double GetTvFlyoutItemFontSize(this ITNavigtaionView nav)
		{
			if (s_navigationViewFlyoutItemFontSize > 0)
				return s_navigationViewFlyoutItemFontSize;
			return s_navigationViewFlyoutItemFontSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(ThemeConstants.Shell.Resources.TV.DefaultFlyoutItemfontSize);
		}

		public static Graphics.Color GetTvFlyoutItemColor(this ITNavigtaionView nav)
		{
			return ThemeConstants.Shell.ColorClass.TV.DefaultFlyoutItemColor;
		}

		public static Graphics.Color GetTvFlyoutItemFocusedColor(this ITNavigtaionView nav)
		{
			return ThemeConstants.Shell.ColorClass.TV.DefaultFlyoutItemFocusedColor;
		}
		#endregion
	}
}
