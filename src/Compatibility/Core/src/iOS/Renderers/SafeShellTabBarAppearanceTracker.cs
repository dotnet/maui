using System.ComponentModel;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SafeShellTabBarAppearanceTracker : IShellTabBarAppearanceTracker
	{
		UIColor _defaultBarTint;
		UIColor _defaultTint;
		UIColor _defaultUnselectedTint;

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
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor; // currently unused
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor; // unused on iOS
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;
			bool operatingSystemSupportsUnselectedTint = Forms.IsiOS10OrNewer;

			if (_defaultTint == null)
			{
				_defaultBarTint = tabBar.BarTintColor;
				_defaultTint = tabBar.TintColor;

				if (operatingSystemSupportsUnselectedTint)
				{
					_defaultUnselectedTint = tabBar.UnselectedItemTintColor;
				}
			}

			if (backgroundColor != null)
				tabBar.BarTintColor = backgroundColor.ToUIColor();
			if (titleColor != null)
				tabBar.TintColor = titleColor.ToUIColor();

			if (operatingSystemSupportsUnselectedTint)
			{
				if (unselectedColor != null)
					tabBar.UnselectedItemTintColor = unselectedColor.ToUIColor();
			}
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

	}
}