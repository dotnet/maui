using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.TextView;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		BottomNavigationView GetTab(ShellSection item)
		{
			var shell = item.FindParentOfType<Shell>();
			var renderer = (ShellRenderer)shell.Handler;
			var bottomView = GetDrawerLayout(renderer).GetFirstChildOfType<BottomNavigationView>();
			var menu = bottomView.Menu;
			var index = shell.CurrentItem.Items.IndexOf(item);

			if (index < 0 || index >= menu.Size())
				Assert.Fail("ShellSection not found in Shell");

			if (index >= menu.Size())
				Assert.Fail("Menu Item has not been created for this item");

			return bottomView;
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTab(item),
					item.Title, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTab(item),
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
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTab(item),
					item.Title, textColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTab(item),
					item.Title, textColor, MauiContext);
			}
		}
	}
}
