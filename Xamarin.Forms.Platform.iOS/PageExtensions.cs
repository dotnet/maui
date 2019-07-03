using System;
using UIKit;

namespace Xamarin.Forms
{
	public static class PageExtensions
	{
		public static UIViewController CreateViewController(this Page page)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (!(page.RealParent is Application))
			{
				Application app = new EmbeddedApplication();
				app.MainPage = page;
			}

			var result = new Platform.iOS.Platform();
			result.SetPage(page);
			return result.ViewController;
		}

		sealed internal class EmbeddedApplication : Application, IDisposable
		{
			public void Dispose()
			{
				CleanUp();
			}
		}
	}
}

namespace Xamarin.Forms.Platform.iOS
{
	public static class PageExtensions
	{
		public static UIViewController CreateViewController(this ContentPage page)
		{
			return Xamarin.Forms.PageExtensions.CreateViewController(page);
		}
	}
}