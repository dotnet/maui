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
		}

		public virtual void SetAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor; // Currently unused
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor; // Unused on iOS
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (_defaultTint == null)
			{
				_defaultBarTint = tabBar.BarTintColor;
				_defaultTint = tabBar.TintColor;
				_defaultUnselectedTint = tabBar.UnselectedItemTintColor;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
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
		void UpdateiOS15TabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;

			controller.TabBar
				.UpdateiOS15TabBarAppearance(
					ref _tabBarAppearance,
					null,
					null,
					appearanceElement.EffectiveTabBarForegroundColor,
					appearanceElement.EffectiveTabBarUnselectedColor,
					appearanceElement.EffectiveTabBarBackgroundColor,
					appearanceElement.EffectiveTabBarTitleColor);
		}

		void UpdateTabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (backgroundColor != null)
				tabBar.BarTintColor = backgroundColor.ToPlatform();
			if (titleColor.IsDefault != null)
				tabBar.TintColor = titleColor.ToPlatform();
			if (unselectedColor.IsDefault != null)
				tabBar.UnselectedItemTintColor = unselectedColor.ToPlatform();
		}
	}
}
