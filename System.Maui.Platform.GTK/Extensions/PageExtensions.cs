using System;

namespace System.Maui.Platform.GTK.Extensions
{
	public static class PageExtensions
	{
		public static GtkFormsContainer CreateContainer(this Page view)
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

			return result.PlatformRenderer;
		}

		class DefaultApplication : Application
		{
		}
	}
}
