using System;
using UIKit;

namespace Xamarin.Forms
{
	public static class PageExtensions
	{
		public static UIViewController CreateViewController(this Page view)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = view;
			}

			var result = new Platform.iOS.Platform();
			result.SetPage(view);
			return result.ViewController;
		}

		class DefaultApplication : Application
		{
		}
	}
}