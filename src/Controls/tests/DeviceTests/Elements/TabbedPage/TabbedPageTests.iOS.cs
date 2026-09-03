using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests
	{
		UITabBar GetTabBar(TabbedPage tabbedPage)
		{
			var pagerParent = (tabbedPage.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			return pagerParent.Subviews.FirstOrDefault(v => v.GetType() == typeof(UITabBar)) as UITabBar;
		}

		async Task ValidateTabBarIconColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
		}
	}
}
