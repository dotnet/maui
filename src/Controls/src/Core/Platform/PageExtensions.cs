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
			{
			{
				return modal;
			}
			else if (currentPage is FlyoutPage fp)
			{
			{
				return GetCurrentPage(fp.Detail);
			}
			else if (currentPage is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController ssc)
			{
			{
				return ssc.PresentedPage;
			}
			else if (currentPage is IPageContainer<Page> pc)

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return GetCurrentPage(pc.CurrentPage);
			else
After:
			{
				return GetCurrentPage(pc.CurrentPage);
			}
			else
			{
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return GetCurrentPage(pc.CurrentPage);
			else
After:
			{
				return GetCurrentPage(pc.CurrentPage);
			}
			else
			{
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return GetCurrentPage(pc.CurrentPage);
			else
After:
			{
				return GetCurrentPage(pc.CurrentPage);
			}
			else
			{
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return GetCurrentPage(pc.CurrentPage);
			else
After:
			{
				return GetCurrentPage(pc.CurrentPage);
			}
			else
			{
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return GetCurrentPage(pc.CurrentPage);
			else
After:
			{
				return GetCurrentPage(pc.CurrentPage);
			}
			else
			{
*/
			{
				return GetCurrentPage(pc.CurrentPage);

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
		}
After:
			}
		}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
		}
After:
			}
		}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
		}
After:
			}
		}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
		}
After:
			}
		}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
		}
After:
			}
		}
*/
			}
			else
			{
				return currentPage;
			}
		}
	}
}