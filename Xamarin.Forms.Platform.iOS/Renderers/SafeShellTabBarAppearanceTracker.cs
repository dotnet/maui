using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class SafeShellTabBarAppearanceTracker : IShellTabBarAppearanceTracker
	{
		UIColor _defaultBarTint;
		UIColor _defaultTint;
		UIColor _defaultUnselectedTint;

		public void ResetAppearance(UITabBarController controller)
		{
			if (_defaultTint == null)
				return;

			var tabBar = controller.TabBar;
			tabBar.BarTintColor = _defaultBarTint;
			tabBar.TintColor = _defaultTint;
			tabBar.UnselectedItemTintColor = _defaultUnselectedTint;
		}

		public void SetAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor; // currently unused
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor; // unused on iOS
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (_defaultTint == null)
			{
				_defaultBarTint = tabBar.BarTintColor;
				_defaultTint = tabBar.TintColor;
				_defaultUnselectedTint = tabBar.UnselectedItemTintColor;
			}

			if (!backgroundColor.IsDefault)
				tabBar.BarTintColor = backgroundColor.ToUIColor();
			if (!titleColor.IsDefault)
				tabBar.TintColor = titleColor.ToUIColor();
			if (!unselectedColor.IsDefault)
				tabBar.UnselectedItemTintColor = unselectedColor.ToUIColor();
		}

		public void UpdateLayout(UITabBarController controller)
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

	}
}