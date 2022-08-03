using System;
using System.Linq;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility
{
	[System.Obsolete]
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

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete]
	public static class PageExtensions
	{
		public static UIViewController CreateViewController(this ContentPage page)
		{
			return Microsoft.Maui.Controls.Compatibility.PageExtensions.CreateViewController(page);
		}
	}
}