using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class PageViewController : ContainerViewController
	{
		public PageViewController(IView page, IMauiContext mauiContext)
		{
			CurrentView = page;
			Context = mauiContext;

			LoadFirstView(page);
		}

		protected override UIView CreatePlatformView(IElement view)
		{
			return new ContentView
			{
				CrossPlatformLayout = (IContentView)view
			};
		}

		public override bool PrefersHomeIndicatorAutoHidden
			=> CurrentView is IiOSPageSpecifics pageSpecifics && pageSpecifics.IsHomeIndicatorAutoHidden;

		public override bool PrefersStatusBarHidden()
		{
			if (CurrentView is IiOSPageSpecifics pageSpecifics)
			{
				return pageSpecifics.PrefersStatusBarHiddenMode switch
				{
					1 => true,
					2 => false,
					_ => base.PrefersStatusBarHidden(),
				};
			}

			return base.PrefersStatusBarHidden();
		}

		public override UIStatusBarAnimation PreferredStatusBarUpdateAnimation
		{
			get
			{
				if (CurrentView is IiOSPageSpecifics pageSpecifics)
				{
					return pageSpecifics.PreferredStatusBarUpdateAnimationMode switch
					{
						0 => UIStatusBarAnimation.Fade,
						1 => UIStatusBarAnimation.Slide,
						_ => UIStatusBarAnimation.None,
					};
				}
				return base.PreferredStatusBarUpdateAnimation;
			}
		}

		public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
		{
			if (CurrentView?.Handler is ElementHandler handler)
			{
				// Check if the window is being destroyed by verifying its handler is still connected.
				// Window.Destroying() calls Handler?.DisconnectHandler() before DisposeWindowScope(),
				// so checking window.Handler == null tells us if we're in the teardown phase.
				var window = handler.MauiContext?.GetPlatformWindow()?.GetWindow();
				if (window?.Handler == null)
				{
					// Window is being destroyed, skip theme update to avoid accessing disposed services
					return;
				}

				try
				{
					var application = handler.GetRequiredService<IApplication>();
					application.UpdateUserInterfaceStyle();
					application.ThemeChanged();
				}
				catch (ObjectDisposedException)
				{
					// Extra safety net in case we hit a race condition where the service provider
					// is disposed between our check and the actual service access.
				}
			}

#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
		}
	}
}

