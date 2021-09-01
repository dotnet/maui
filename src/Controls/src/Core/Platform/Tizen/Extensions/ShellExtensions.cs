using Tizen.UIExtensions.Common;
using TINavigtaionView = Tizen.UIExtensions.ElmSharp.INavigationView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ShellExtensions
	{
		static double s_navigationViewFlyoutItemHeight = -1;
		public static double GetFlyoutItemHeight(this TINavigtaionView nav)
		{
			if (s_navigationViewFlyoutItemHeight > 0)
				return s_navigationViewFlyoutItemHeight;
			return s_navigationViewFlyoutItemHeight = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(60);
		}

		static double s_navigationViewFlyoutItemWidth = -1;
		public static double GetFlyoutItemWidth(this TINavigtaionView nav)
		{
			if (s_navigationViewFlyoutItemWidth > 0)
				return s_navigationViewFlyoutItemWidth;
			return s_navigationViewFlyoutItemWidth = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(200);
		}

		static double s_navigationViewFlyoutIconColumnSize = -1;
		public static double GetFlyoutIconColumnSize(this TINavigtaionView nav)
		{
			if (s_navigationViewFlyoutIconColumnSize > 0)
				return s_navigationViewFlyoutIconColumnSize;
			return s_navigationViewFlyoutIconColumnSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(40);
		}

		static double s_navigationViewFlyoutIconSize = -1;
		public static double GetFlyoutIconSize(this TINavigtaionView nav)
		{
			if (s_navigationViewFlyoutIconSize > 0)
				return s_navigationViewFlyoutIconSize;
			return s_navigationViewFlyoutIconSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(25);
		}

		static double s_navigationViewFlyoutMargin = -1;
		public static double GetFlyoutMargin(this TINavigtaionView nav)
		{
			if (s_navigationViewFlyoutMargin > 0)
				return s_navigationViewFlyoutMargin;
			return s_navigationViewFlyoutMargin = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(10);
		}

		static double s_navigationViewFlyoutItemFontSize = -1;
		public static double GetFlyoutItemFontSize(this TINavigtaionView nav)
		{
			if (s_navigationViewFlyoutItemFontSize > 0)
				return s_navigationViewFlyoutItemFontSize;
			return s_navigationViewFlyoutItemFontSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(25);
		}

		#region ShellMoreToolbar

		static double s_shellMoreToolBarIconPadding = -1;
		public static double GetIconPadding(this ShellMoreTabs self)
		{
			if (s_shellMoreToolBarIconPadding > 0)
				return s_shellMoreToolBarIconPadding;
			return s_shellMoreToolBarIconPadding = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(15);
		}

		static double s_shellMoreToolBarIconSize = -1;
		public static double GetIconSize(this ShellMoreTabs self)
		{
			if (s_shellMoreToolBarIconSize > 0)
				return s_shellMoreToolBarIconSize;
			return s_shellMoreToolBarIconSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(30);
		}
		#endregion

		#region ShellNavBar
		static double s_shellNavBarDefaultHeight = -1;
		public static double GetDefaultHeight(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultHeight > 0)
				return s_shellNavBarDefaultHeight;
			return s_shellNavBarDefaultHeight = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(70);
		}

		static double s_shellNavBarDefaultMenuSize = -1;
		public static double GetDefaultMenuSize(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultMenuSize > 0)
				return s_shellNavBarDefaultMenuSize;
			return s_shellNavBarDefaultMenuSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(Device.Idiom == TargetIdiom.TV ? 70 : 40);
		}

		static double s_shellNavBarDefaultMargin = -1;
		public static double GetDefaultMargin(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultMargin > 0)
				return s_shellNavBarDefaultMargin;
			return s_shellNavBarDefaultMargin = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(10);
		}

		static double s_shellNavBarTitleVDefaultMargin = -1;
		public static double GetDefaultTitleVMargin(this ShellNavBar navBar)
		{
			if (s_shellNavBarTitleVDefaultMargin > 0)
				return s_shellNavBarTitleVDefaultMargin;
			return s_shellNavBarTitleVDefaultMargin = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(23);
		}

		static double s_shellNavBarTitleFontSize = -1;
		public static double GetDefaultTitleFontSize(this ShellNavBar navBar)
		{
			if (s_shellNavBarTitleFontSize > 0)
				return s_shellNavBarTitleFontSize;
			return s_shellNavBarTitleFontSize = DeviceInfo.CalculateDoubleScaledSizeInLargeScreen(23);
		}
		#endregion
	}
}
