using System;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class PageExtensions
	{
		public static EvasObject CreateEvasObject(this Page page, EvasObject parent, bool hasAlpha = false)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (parent == null)
				throw new InvalidOperationException("Window could not be null");

			if (!(page.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = page;
			}

			var platform = Platform.Tizen.Platform.CreatePlatform(parent);
			platform.HasAlpha = hasAlpha;
			platform.SetPage(page);
			return platform.GetRootNativeView();
		}

		class DefaultApplication : Application
		{
		}
	}
}

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class PageExtensions
	{
		public static EvasObject CreateEvasObject(this ContentPage page, EvasObject parent, bool hasAlpha = false)
		{
			return Microsoft.Maui.Controls.Compatibility.PageExtensions.CreateEvasObject(page, parent, hasAlpha);
		}

		public static void UpdateFocusTreePolicy<T>(this MultiPage<T> multiPage) where T : Page
		{
			foreach (var pageItem in multiPage.Children)
			{
				if (Platform.GetRenderer(pageItem)?.NativeView is ElmSharp.Widget nativeWidget)
				{
					if (pageItem == multiPage.CurrentPage)
					{
						nativeWidget.AllowTreeFocus = true;
						continue;
					}
					nativeWidget.AllowTreeFocus = false;
				}
			}
		}
	}
}