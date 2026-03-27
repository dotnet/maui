#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Platform;
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

		// iOS 26+ stores pending colors to re-apply during layout
		UIColor _pendingUnselectedTintColor;
		UIColor _pendingSelectedTintColor;

		public virtual void ResetAppearance(UITabBarController controller)
		{
			if (_defaultTint == null)
				return;

			var tabBar = controller.TabBar;
			tabBar.BarTintColor = _defaultBarTint;
			tabBar.TintColor = _defaultTint;
			tabBar.UnselectedItemTintColor = _defaultUnselectedTint;
			_pendingUnselectedTintColor = null;
			_pendingSelectedTintColor = null;
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
			// iOS 26+: Re-apply colors on every layout pass.
			// The liquid glass tab bar resets subview properties during layout.
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				Console.WriteLine("DEBUG: UpdateLayout iOS 26+ path");
				var tabBar = controller.TabBar;
				if (_pendingSelectedTintColor is not null)
					tabBar.TintColor = _pendingSelectedTintColor;
				if (_pendingUnselectedTintColor is not null)
				{
					Console.WriteLine("DEBUG: Calling ApplyPreColoredImagesForIOS26");
					tabBar.UnselectedItemTintColor = _pendingUnselectedTintColor;
					tabBar.ApplyPreColoredImagesForIOS26(_pendingUnselectedTintColor, _pendingSelectedTintColor);
				}
			}
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

			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			// iOS 26+: The UITabBarAppearance Normal state (TitleTextAttributes, IconColor) is
			// ignored by the liquid glass tab bar. Skip the full appearance pipeline and use
			// direct UITabBar properties plus subview coloring instead.
			// See: https://github.com/dotnet/maui/issues/32125, https://github.com/dotnet/maui/issues/34605
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				var tabBar = controller.TabBar;

				// Background color via appearance (this still works on iOS 26)
				if (_tabBarAppearance == null)
				{
					_tabBarAppearance = new UITabBarAppearance();
					_tabBarAppearance.ConfigureWithDefaultBackground();
				}
				if (backgroundColor is not null)
					_tabBarAppearance.BackgroundColor = backgroundColor.ToPlatform();

				tabBar.StandardAppearance = _tabBarAppearance;
				tabBar.ScrollEdgeAppearance = _tabBarAppearance;

				// Selected color via TintColor (works on iOS 26)
				var selectedColor = foregroundColor ?? titleColor;
				if (selectedColor is not null)
				{
					_pendingSelectedTintColor = selectedColor.ToPlatform();
					tabBar.TintColor = _pendingSelectedTintColor;
				}
				else
				{
					_pendingSelectedTintColor = null;
					tabBar.TintColor = _defaultTint;
				}

				// Unselected color: set property + pre-colored images for visual rendering
				if (unselectedColor is not null)
				{
					_pendingUnselectedTintColor = unselectedColor.ToPlatform();
					tabBar.UnselectedItemTintColor = _pendingUnselectedTintColor;
					tabBar.ApplyPreColoredImagesForIOS26(_pendingUnselectedTintColor, _pendingSelectedTintColor);
				}
				else
				{
					_pendingUnselectedTintColor = null;
					tabBar.UnselectedItemTintColor = _defaultUnselectedTint;
				}

				return;
			}

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
		}

		void UpdateTabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

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
		}
	}
}
