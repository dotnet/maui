#nullable disable
using System;
using System.ComponentModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SafeShellTabBarAppearanceTracker : IShellTabBarAppearanceTracker
	{
		UIColor _defaultBarTint;
		UIColor _defaultTint;
		UIColor _defaultUnselectedTint;
		UITabBarAppearance _tabBarAppearance;
		public virtual void ResetAppearance(UITabBarController controller)
		{
			if (_defaultTint == null)
				return;

			var tabBar = controller.TabBar;
			tabBar.BarTintColor = _defaultBarTint;
			tabBar.TintColor = _defaultTint;
			tabBar.UnselectedItemTintColor = _defaultUnselectedTint;

			// Clear UITabBarAppearance state and direct BackgroundColor set by SetAppearance,
			// otherwise the tab bar remains styled after the Shell appearance is removed.
			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
				ResetModernAppearance(tabBar);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst15.0")]
		void ResetModernAppearance(UITabBar tabBar)
		{
			if (_tabBarAppearance is not null)
			{
				_tabBarAppearance.Dispose();
				_tabBarAppearance = null;
			}

			tabBar.Translucent = true;
			tabBar.BackgroundColor = null;
			var defaultAppearance = new UITabBarAppearance();
			defaultAppearance.ConfigureWithDefaultBackground();
			tabBar.StandardAppearance = tabBar.ScrollEdgeAppearance = defaultAppearance;
		}

		public virtual void SetAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (_defaultTint == null)
			{
				_defaultBarTint = tabBar.BarTintColor;
				_defaultTint = tabBar.TintColor;
				_defaultUnselectedTint = tabBar.UnselectedItemTintColor;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
				UpdateiOS15TabBarAppearance(controller, appearance);
			else
				UpdateTabBarAppearance(controller, appearance);
		}

		public virtual void UpdateLayout(UITabBarController controller)
		{
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst15.0")]
		void UpdateiOS15TabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;

			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor;

			controller.TabBar
				.UpdateiOS15TabBarAppearance(
					ref _tabBarAppearance,
					null,
					null,
					foregroundColor ?? titleColor,
					unselectedColor,
					backgroundColor,
					titleColor ?? foregroundColor,
					unselectedColor);

			// Set disabled color in the global appearance for text-only tabs
			if (disabledColor is not null && _tabBarAppearance is not null)
			{
				var disabledUIColor = disabledColor.ToPlatform();
				var disabledAttributes = new UIStringAttributes { ForegroundColor = disabledUIColor };

				_tabBarAppearance.StackedLayoutAppearance.Disabled.TitleTextAttributes = disabledAttributes;
				_tabBarAppearance.InlineLayoutAppearance.Disabled.TitleTextAttributes = disabledAttributes;
				_tabBarAppearance.CompactInlineLayoutAppearance.Disabled.TitleTextAttributes = disabledAttributes;

				controller.TabBar.StandardAppearance = controller.TabBar.ScrollEdgeAppearance = _tabBarAppearance;
			}
		}

		void UpdateTabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor;

			var tabBar = controller.TabBar;

			if (backgroundColor is not null && backgroundColor.IsNotDefault())
				tabBar.BarTintColor = backgroundColor.ToPlatform();

			if (unselectedColor is not null && unselectedColor.IsNotDefault())
			{
				tabBar.UnselectedItemTintColor = unselectedColor.ToPlatform();
				UITabBarItem.Appearance.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = unselectedColor.ToPlatform() }, UIControlState.Normal);
			}

			if (titleColor is not null && titleColor.IsNotDefault() ||
				foregroundColor is not null && foregroundColor.IsNotDefault())
			{
				tabBar.TintColor = (foregroundColor ?? titleColor).ToPlatform();
				UITabBarItem.Appearance.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = (titleColor ?? foregroundColor).ToPlatform() }, UIControlState.Selected);
			}

			// Set disabled color for text-only tabs
			if (disabledColor is not null && disabledColor.IsNotDefault())
				UITabBarItem.Appearance.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = disabledColor.ToPlatform() }, UIControlState.Disabled);
		}
	}
}
