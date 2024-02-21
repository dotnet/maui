using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		UITabBar GetTabBar(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			return pagerParent.Subviews.FirstOrDefault(v => v.GetType() == typeof(UITabBar)) as UITabBar;
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTabBar(item),
					item.Title, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTabBar(item),
					item.Title, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
				ShellSection item,
				Color textColor,
				bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTabBar(item),
					item.Title, textColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTabBar(item),
					item.Title, textColor, MauiContext);
			}
		}
	}
}