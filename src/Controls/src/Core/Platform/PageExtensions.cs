#nullable disable
using System;
using System.Linq;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PageExtensions
	{
		internal static Page GetCurrentPage(this Page currentPage)
		{
			if (currentPage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
				return modal;
			else if (currentPage is FlyoutPage fp)
				return GetCurrentPage(fp.Detail);
			else if (currentPage is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController ssc)
				return ssc.PresentedPage;
			else if (currentPage is IPageContainer<Page> pc)
				return GetCurrentPage(pc.CurrentPage);
			else
				return currentPage;
		}

		internal static bool ParentHasAppeared(this Page currentPage, Func<Page, bool> hasPageAppeared)
		{
			Element parent = currentPage.Parent as Page;

			bool hasAppeared = true;

			while (parent is Page page && hasAppeared)
			{
				hasAppeared = hasPageAppeared.Invoke(page);
				if (hasAppeared)
				{
					parent = page.Parent;
				}
			}

			return hasAppeared;
		}
	}
}