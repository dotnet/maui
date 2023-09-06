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
		UIView GetTabItemView(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			var tabBar = pagerParent.Subviews.FirstOrDefault(v => v.GetType() == typeof(UITabBar)) as UITabBar;

			Assert.NotNull(tabBar);

			var tabBarItem = tabBar.Items.Single(t => string.Equals(t.Title, item.Title, StringComparison.OrdinalIgnoreCase));
			var tabBarItemView = tabBarItem.ValueForKey(new Foundation.NSString("view")) as UIView;
			return tabBarItemView;
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			var tabBarItemView = GetTabItemView(item);
			Assert.NotNull(tabBarItemView);

			if (hasColor)
			{
				await tabBarItemView.FindDescendantView<UIImageView>().AssertContainsColor(iconColor, MauiContext);
			}
			else
			{
				await tabBarItemView.FindDescendantView<UIImageView>()
					.AssertDoesNotContainColor(iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
				ShellSection item,
				Color textColor,
				bool hasColor)
		{
			var tabBarItemView = GetTabItemView(item);
			Assert.NotNull(tabBarItemView);

			if (hasColor)
			{
				await tabBarItemView.FindDescendantView<UILabel>()
					.AssertContainsColor(textColor, MauiContext, 0.1);
			}
			else
			{
				await tabBarItemView.FindDescendantView<UILabel>()
					.AssertDoesNotContainColor(textColor, MauiContext);
			}
		}
	}
}