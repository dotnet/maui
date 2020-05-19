using System;
using AppKit;

namespace System.Maui.Platform.MacOS
{
	public static class PageExtensions
	{
		public static NSViewController CreateViewController(this Page view)
		{
			if (!System.Maui.Maui.IsInitialized)
				throw new InvalidOperationException("call System.Maui.Maui.Init() before this");

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