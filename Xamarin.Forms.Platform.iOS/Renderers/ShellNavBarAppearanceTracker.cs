using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellNavBarAppearanceTracker : IShellNavBarAppearanceTracker
	{
		UIView _blurView;
		UIView _colorView;
		UIImage _defaultBackgroundImage;
		UIColor _defaultTint;
		UIStringAttributes _defaultTitleAttributes;
		bool _disposed = false;

		public void UpdateLayout (UINavigationController controller)
		{
			if (_blurView?.Superview == null)
				return;

			var navBar = controller.NavigationBar;
			navBar.SendSubviewToBack(_colorView);
			navBar.SendSubviewToBack(_blurView);

			var frame = navBar.Frame;
			frame.Height += frame.Y;
			frame.Y = -frame.Y;

			_blurView.Frame = frame;
			_colorView.Frame = frame;
		}

		public void ResetAppearance(UINavigationController controller)
		{
			if (_blurView != null)
			{
				var navBar = controller.NavigationBar;
				navBar.SetBackgroundImage(_defaultBackgroundImage, UIBarMetrics.Default);
				navBar.TintColor = _defaultTint;
				navBar.TitleTextAttributes = _defaultTitleAttributes;

				_blurView.RemoveFromSuperview();
				_colorView.RemoveFromSuperview();
			}
		}

		public void SetAppearance(UINavigationController controller, ShellAppearance appearance)
		{
			var background = appearance.BackgroundColor;
			var foreground = appearance.ForegroundColor;
			var titleColor = appearance.TitleColor;

			var navBar = controller.NavigationBar;

			if (_blurView == null)
			{
				_defaultBackgroundImage = navBar.GetBackgroundImage(UIBarMetrics.Default);
				_defaultTint = navBar.TintColor;
				_defaultTitleAttributes = navBar.TitleTextAttributes;

				var frame = navBar.Frame;
				frame.Height += frame.Y;
				frame.Y = -frame.Y;

				var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Regular);
				_blurView = new UIVisualEffectView(effect);
				_blurView.UserInteractionEnabled = false;
				_blurView.Frame = frame;

				_colorView = new UIView(frame);
				_colorView.UserInteractionEnabled = false;

				if (Forms.IsiOS11OrNewer)
				{
					_blurView.Layer.ShadowColor = UIColor.Black.CGColor;
					_blurView.Layer.ShadowOpacity = 1f;
					_blurView.Layer.ShadowRadius = 3;
				}
			}

			navBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);

			navBar.InsertSubview(_colorView, 0);
			navBar.InsertSubview(_blurView, 0);

			if (!background.IsDefault)
			{
				_colorView.BackgroundColor = background.ToUIColor();
			}

			if (!foreground.IsDefault)
				navBar.TintColor = foreground.ToUIColor();
			if (!titleColor.IsDefault)
			{
				navBar.TitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = titleColor.ToUIColor()
				};
			}
		}

		#region IDisposable Support
		
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_blurView != null)
					{
						_blurView.RemoveFromSuperview();
						_blurView.Dispose();
					}

					if (_colorView != null)
					{
						_colorView.RemoveFromSuperview();
						_colorView.Dispose();
					}
				}

				_colorView = null;
				_blurView = null;

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}