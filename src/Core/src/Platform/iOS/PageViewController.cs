using Microsoft.Maui.ApplicationModel;
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
				CrossPlatformLayout = ((IContentView)view)
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
				var application = handler.GetRequiredService<IApplication>();

				application?.UpdateUserInterfaceStyle();
				application?.ThemeChanged();
			}

#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
		}
	}
}