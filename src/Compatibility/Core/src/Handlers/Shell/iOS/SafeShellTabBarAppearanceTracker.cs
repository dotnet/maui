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

			if (NativeVersion.IsAtLeast(15))
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
				tabBarAppearance.BackgroundColor = tabBarBackgroundColor.ToNative();

			// Set TabBarTitleColor
			var tabBarTitleColor = appearanceElement.EffectiveTabBarTitleColor;

			// Update colors for all variations of the appearance to also make it work for iPads, etc. which use different layouts for the tabbar
			// Also, set ParagraphStyle explicitly. This seems to be an iOS bug. If we don't do this, tab titles will be truncat...
			if (tabBarTitleColor != null)
			{
				tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarTitleColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = tabBarTitleColor.ToNative();

				tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarTitleColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = tabBarTitleColor.ToNative();

				tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarTitleColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = tabBarTitleColor.ToNative();
			}

			//Set TabBarUnselectedColor
			var tabBarUnselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;

			if (tabBarUnselectedColor != null)
			{
				tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarUnselectedColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = tabBarUnselectedColor.ToNative();

				tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarUnselectedColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = tabBarUnselectedColor.ToNative();

				tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarUnselectedColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = tabBarUnselectedColor.ToNative();
			}

			// Set TabBarDisabledColor
			var tabBarDisabledColor = appearanceElement.EffectiveTabBarDisabledColor;

			if (tabBarDisabledColor != null)
			{
				tabBarAppearance.StackedLayoutAppearance.Disabled.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarDisabledColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.StackedLayoutAppearance.Disabled.IconColor = tabBarDisabledColor.ToNative();

				tabBarAppearance.InlineLayoutAppearance.Disabled.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarDisabledColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.InlineLayoutAppearance.Disabled.IconColor = tabBarDisabledColor.ToNative();

				tabBarAppearance.CompactInlineLayoutAppearance.Disabled.TitleTextAttributes = new UIStringAttributes { ForegroundColor = tabBarDisabledColor.ToNative(), ParagraphStyle = NSParagraphStyle.Default };
				tabBarAppearance.CompactInlineLayoutAppearance.Disabled.IconColor = tabBarDisabledColor.ToNative();
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
				tabBar.BarTintColor = backgroundColor.ToNative();
			if (titleColor.IsDefault != null)
				tabBar.TintColor = titleColor.ToNative();
			if (unselectedColor.IsDefault != null)
				tabBar.UnselectedItemTintColor = unselectedColor.ToNative();
		}
	}
}
