using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Android.Material.AppBar;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	public partial class NavigationPageTests : HandlerTestBase
	{
		MaterialToolbar GetPlatformToolbar(IElementHandler handler) =>
			GetPlatformToolbar(handler.MauiContext);

		MaterialToolbar GetPlatformToolbar(IMauiContext mauiContext)
		{
			var navManager = mauiContext.GetNavigationRootManager();
			return navManager.ToolbarElement.Toolbar.Handler.PlatformView as
				MaterialToolbar;
		}

		public bool IsBackButtonVisible(IElementHandler handler) =>
			IsBackButtonVisible(handler.MauiContext);

		public bool IsBackButtonVisible(IMauiContext mauiContext)
		{
			return GetPlatformToolbar(mauiContext).NavigationIcon != null;
		}

		public bool IsNavigationBarVisible(IElementHandler handler) =>
			IsNavigationBarVisible(handler.MauiContext);

		public bool IsNavigationBarVisible(IMauiContext mauiContext)
		{
			return GetPlatformToolbar(mauiContext)
					.LayoutParameters.Height > 0;
		}

		public bool ToolbarItemsMatch(
			IElementHandler handler,
			params ToolbarItem[] toolbarItems)
		{
			var toolbar = GetPlatformToolbar(handler);
			var menu = toolbar.Menu;

			Assert.Equal(toolbarItems.Length, menu.Size());

			for (var i = 0; i < toolbarItems.Length; i++)
			{
				ToolbarItem toolbarItem = toolbarItems[i];
				var primaryCommand = menu.GetItem(i);
				Assert.Equal(toolbarItem.Text, $"{primaryCommand.TitleFormatted}");
			}

			return true;
		}

		string GetToolbarTitle(IElementHandler handler) =>
			GetPlatformToolbar(handler).Title;
	}
}
