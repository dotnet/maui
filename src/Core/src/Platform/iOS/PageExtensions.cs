using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class PageExtensions
	{
		public static void UpdateTitle(this UIViewController viewController, IContentView page)
		{
			if (page is not ITitledElement titled)
				return;

			viewController.Title = titled.Title;
		}

		public static void UpdateBackground(this UIView platformView, IContentView page, IImageSourceServiceProvider? provider)
		{
			if (page.Background is ImageSourcePaint image)
				platformView.UpdateBackgroundImageSourceAsync(image.ImageSource, provider).FireAndForget();
			else
				platformView.UpdateBackground(page);
		}
	}
}