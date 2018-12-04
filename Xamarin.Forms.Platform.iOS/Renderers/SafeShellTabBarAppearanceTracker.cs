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
			var background = appearance.BackgroundColor;
			var foreground = appearance.ForegroundColor;
			var unselectedColor = appearance.UnselectedColor;
			var tabBar = controller.TabBar;

			if (_defaultTint == null)
			{
				_defaultBarTint = tabBar.BarTintColor;
				_defaultTint = tabBar.TintColor;
				_defaultUnselectedTint = tabBar.UnselectedItemTintColor;
			}

			if (!background.IsDefault)
				tabBar.BarTintColor = background.ToUIColor();
			if (!foreground.IsDefault)
				tabBar.TintColor = foreground.ToUIColor();
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