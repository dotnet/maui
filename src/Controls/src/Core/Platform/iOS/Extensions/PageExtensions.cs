using System;
using System.Linq;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PageExtensions
	{
		internal static Page GetCurrentPage(this Page currentPage)
		{
			if (currentPage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
				return modal;
#pragma warning disable CS0618 // Type or member is obsolete
			else if (currentPage is MasterDetailPage mdp)
#pragma warning restore CS0618 // Type or member is obsolete
				return GetCurrentPage(mdp.Detail);
			else if (currentPage is FlyoutPage fp)
				return GetCurrentPage(fp.Detail);
			else if (currentPage is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController ssc)
				return ssc.PresentedPage;
			else if (currentPage is IPageContainer<Page> pc)
				return GetCurrentPage(pc.CurrentPage);
			else
				return currentPage;
		}
	}
}