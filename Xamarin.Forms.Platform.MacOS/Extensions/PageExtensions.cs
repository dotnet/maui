using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public static class PageExtensions
	{
		public static NSViewController CreateViewController(this Page view)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = view;
			}

			var result = new Platform();
			result.SetPage(view);
			return result.ViewController;
		}

		class DefaultApplication : Application
		{
		}
	}
}