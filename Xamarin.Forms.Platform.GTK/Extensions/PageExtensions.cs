using System;

namespace Xamarin.Forms.Platform.GTK.Extensions
{
	public static class PageExtensions
	{
		public static GtkFormsContainer CreateContainer(this Page view)
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

			return result.PlatformRenderer;
		}

		class DefaultApplication : Application
		{
		}
	}
}
