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