using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class PageControllerDelegate : NSPageControllerDelegate
	{
		public override string GetIdentifier(NSPageController pageController, NSObject targetObject)
		{
			return nameof(PageRenderer);
		}

		public override NSViewController GetViewController(NSPageController pageController, string identifier)
		{
			return new PageRenderer();
		}

		public override void PrepareViewController(NSPageController pageController, NSViewController viewController,
			NSObject targetObject)
		{
			var pageContainer = targetObject as NSPageContainer;
			var pageRenderer = (viewController as PageRenderer);
			if (pageContainer == null || pageRenderer == null)
				return;
			Page page = pageContainer.Page;
			pageRenderer.SetElement(page);
			Platform.SetRenderer(page, pageRenderer);
		}
	}
}