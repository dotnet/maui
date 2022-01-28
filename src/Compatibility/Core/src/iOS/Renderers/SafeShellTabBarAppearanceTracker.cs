using System.ComponentModel;
using ObjCRuntime;
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
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor; // Currently unused
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor; // Unused on iOS
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

			if (Forms.IsiOS15OrNewer)
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

		void UpdateiOS15TabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;

			var tabBar = controller.TabBar;

			var tabBarAppearance = new UITabBarAppearance();
			tabBarAppearance.ConfigureWithOpaqueBackground();

			// Set TabBarBackgroundColor
			var tabBarBackgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;

			if (tabBarBackgroundColor != null)
				tabBarAppearance.BackgroundColor = tabBarBackgroundColor.ToUIColor();

			// Set TabBarTitleColor
			var tabBarTitleColor = appearanceElement.EffectiveTabBarTitleColor;

			// Update colors for all variations of the appearance to also make it work for iPads, etc. which use different layouts for the tabbar
			// Also, set ParagraphStyle explicitly. This seems to be an iOS bug. If we don't do this, tab titles will be truncat...
			if (tabBarTitleColor != null)
			{
				tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarTitleColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = tabBarTitleColor.ToUIColor();

				tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarTitleColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = tabBarTitleColor.ToUIColor();

				tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarTitleColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = tabBarTitleColor.ToUIColor();
			}

			//Set TabBarUnselectedColor
			var tabBarUnselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;

			if (tabBarUnselectedColor != null)
			{
				tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarUnselectedColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = tabBarUnselectedColor.ToUIColor();

				tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarUnselectedColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = tabBarUnselectedColor.ToUIColor();

				tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarUnselectedColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = tabBarUnselectedColor.ToUIColor();
			}

			// Set TabBarDisabledColor
			var tabBarDisabledColor = appearanceElement.EffectiveTabBarDisabledColor;

			if (tabBarDisabledColor != null)
			{
				tabBarAppearance.StackedLayoutAppearance.Disabled.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarDisabledColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.StackedLayoutAppearance.Disabled.IconColor = tabBarDisabledColor.ToUIColor();

				tabBarAppearance.InlineLayoutAppearance.Disabled.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarDisabledColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.InlineLayoutAppearance.Disabled.IconColor = tabBarDisabledColor.ToUIColor();

				tabBarAppearance.CompactInlineLayoutAppearance.Disabled.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarDisabledColor.ToUIColor(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.CompactInlineLayoutAppearance.Disabled.IconColor = tabBarDisabledColor.ToUIColor();
			}

			tabBar.StandardAppearance = tabBar.ScrollEdgeAppearance = tabBarAppearance;
		}

		void UpdateTabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (backgroundColor != null)
				tabBar.BarTintColor = backgroundColor.ToUIColor();
			if (titleColor.IsDefault != null)
				tabBar.TintColor = titleColor.ToUIColor();

			bool operatingSystemSupportsUnselectedTint = Forms.IsiOS10OrNewer;

			if (operatingSystemSupportsUnselectedTint)
			{
				if (unselectedColor.IsDefault != null)
					tabBar.UnselectedItemTintColor = unselectedColor.ToUIColor();
			}
		}
	}
}