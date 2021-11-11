using System;
using System.ComponentModel;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class ShellNavBarAppearanceTracker : IShellNavBarAppearanceTracker
	{
		UIColor _defaultBarTint;
		UIColor _defaultTint;
		UIStringAttributes _defaultTitleAttributes;
		float _shadowOpacity = float.MinValue;
		CGColor _shadowColor;

		public void UpdateLayout(UINavigationController controller)
		{
		}

		public void ResetAppearance(UINavigationController controller)
		{
			if (_defaultTint != null)
			{
				var navBar = controller.NavigationBar;
				navBar.BarTintColor = _defaultBarTint;
				navBar.TintColor = _defaultTint;
				navBar.TitleTextAttributes = _defaultTitleAttributes;
			}
		}

		public void SetAppearance(UINavigationController controller, ShellAppearance appearance)
		{
			var navBar = controller.NavigationBar;

			if (_defaultTint == null)
			{
				_defaultBarTint = navBar.BarTintColor;
				_defaultTint = navBar.TintColor;
				_defaultTitleAttributes = navBar.TitleTextAttributes;
			}

			if (Forms.IsiOS15OrNewer)
				UpdateiOS15NavigationBarAppearance(controller, appearance);
			else
				UpdateNavigationBarAppearance(controller, appearance);
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public virtual void SetHasShadow(UINavigationController controller, bool hasShadow)
		{
			var navigationBar = controller.NavigationBar;
			if (_shadowOpacity == float.MinValue)
			{
				// Don't do anything if user hasn't changed the shadow to true
				if (!hasShadow)
					return;

				_shadowOpacity = navigationBar.Layer.ShadowOpacity;
				_shadowColor = navigationBar.Layer.ShadowColor;
			}

			if (hasShadow)
			{
				navigationBar.Layer.ShadowColor = UIColor.Black.CGColor;
				navigationBar.Layer.ShadowOpacity = 1.0f;
			}
			else
			{
				navigationBar.Layer.ShadowColor = _shadowColor;
				navigationBar.Layer.ShadowOpacity = _shadowOpacity;
			}
		}

		#endregion

		void UpdateiOS15NavigationBarAppearance(UINavigationController controller, ShellAppearance appearance)
		{
			var navBar = controller.NavigationBar;

			var navigationBarAppearance = new UINavigationBarAppearance();
			navigationBarAppearance.ConfigureWithOpaqueBackground();

			navBar.Translucent = false;

			// Set ForegroundColor
			var foreground = appearance.ForegroundColor;

			if (foreground != null)
				navBar.TintColor = foreground.ToUIColor();

			// Set BackgroundColor
			var background = appearance.BackgroundColor;

			if (background != null)
				navigationBarAppearance.BackgroundColor = background.ToUIColor();

			// Set TitleColor
			var titleColor = appearance.TitleColor;

			if (titleColor != null)
				navigationBarAppearance.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor.ToUIColor() };

			navBar.StandardAppearance = navBar.ScrollEdgeAppearance = navigationBarAppearance;
		}

		void UpdateNavigationBarAppearance(UINavigationController controller, ShellAppearance appearance)
		{
			var background = appearance.BackgroundColor;
			var foreground = appearance.ForegroundColor;
			var titleColor = appearance.TitleColor;

			var navBar = controller.NavigationBar;

			if (background != null)
				navBar.BarTintColor = background.ToUIColor();
			if (foreground != null)
				navBar.TintColor = foreground.ToUIColor();
			if (titleColor != null)
			{
				navBar.TitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = titleColor.ToUIColor()
				};
			}
		}
	}
}