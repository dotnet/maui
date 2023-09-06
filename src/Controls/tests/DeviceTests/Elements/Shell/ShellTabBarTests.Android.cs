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
		BottomNavigationItemView GetTab(ShellSection item)
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

			var navigationMenu = (BottomNavigationMenuView)bottomView.MenuView;
			var navItems = navigationMenu.GetChildrenOfType<BottomNavigationItemView>();

			var navItemView =
				navItems.Single(x =>
				{
					return x.GetChildrenOfType<TextView>()
						.Where(tv => String.Equals(tv.Text, item.Title, StringComparison.OrdinalIgnoreCase))
						.Count() > 0;
				});

			return navItemView;
		}

		async Task ValidateTabBarIconColor(ShellSection item, Color expectedColor, bool hasColor)
		{
			var navItemView = (AView)GetTab(item).GetFirstChildOfType<ImageView>().Parent;

			if (hasColor)
				await navItemView.AssertContainsColor(expectedColor.ToPlatform(), item.FindMauiContext());
			else
				await navItemView.AssertDoesNotContainColor(expectedColor.ToPlatform(), item.FindMauiContext());
		}

		async Task ValidateTabBarTextColor(ShellSection item, Color expectedColor, bool hasColor)
		{
			var navItemView = (AView)GetTab(item).GetFirstChildOfType<TextView>().Parent;

			if (hasColor)
				await navItemView.AssertContainsColor(expectedColor.ToPlatform(), item.FindMauiContext());
			else
				await navItemView.AssertDoesNotContainColor(expectedColor.ToPlatform(), item.FindMauiContext());
		}
	}
}
