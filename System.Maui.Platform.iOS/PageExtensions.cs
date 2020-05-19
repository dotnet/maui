using System;
using System.Linq;
using UIKit;

namespace System.Maui
{
	public static class PageExtensions
	{
		public static UIViewController CreateViewController(this Page page)
		{
			if (!System.Maui.Maui.IsInitialized)
				throw new InvalidOperationException("call System.Maui.Maui.Init() before this");

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

namespace System.Maui.Platform.iOS
{
	public static class PageExtensions
	{
		public static UIViewController CreateViewController(this ContentPage page)
		{
			return System.Maui.PageExtensions.CreateViewController(page);
		}

		internal static Page GetCurrentPage(this Page currentPage)
		{
			if (currentPage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
				return modal;
			else if (currentPage is MasterDetailPage mdp)
				return GetCurrentPage(mdp.Detail);
			else if (currentPage is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController ssc)
				return ssc.PresentedPage;
			else if (currentPage is IPageContainer<Page> pc)
				return GetCurrentPage(pc.CurrentPage);
			else
				return currentPage;
		}
	}
}