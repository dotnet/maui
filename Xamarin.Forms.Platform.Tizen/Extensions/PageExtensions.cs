using System;
using ElmSharp;

namespace Xamarin.Forms
{
	public static class PageExtensions
	{
		public static EvasObject CreateEvasObject(this Page page, EvasObject parent, bool hasAlpha = false)
		{
			if (!Platform.Tizen.Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (parent == null)
				throw new InvalidOperationException("Window could not be null");

			if (!(page.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = page;
			}

			var platform = new Platform.Tizen.Platform(Platform.Tizen.Forms.Context, parent)
			{
				HasAlpha = hasAlpha
			};
			platform.SetPage(page);
			return platform.InternalNaviframe;
		}

		class DefaultApplication : Application
		{
		}
	}
}

namespace Xamarin.Forms.Platform.Tizen
{
	public static class PageExtensions
	{
		public static EvasObject CreateEvasObject(this ContentPage page, EvasObject parent, bool hasAlpha = false)
		{
			return Xamarin.Forms.PageExtensions.CreateEvasObject(page, parent, hasAlpha);
		}
	}
}