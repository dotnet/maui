using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.DeviceTests.TestCases;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Toolbar)]
	public partial class ToolbarTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Toolbar Default Label Position")]
		public async Task ToolbarDefaultLabelPositionWithNoImages()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { Text = "Toolbar Item 1" };
			var item2 = new ToolbarItem() { Text = "Toolbar Item 2" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Right);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position with Image and Text")]
		public async Task ToolbarDefaultLabelPositionWithTextAndImages()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { IconImageSource = "red.png" };
			var item2 = new ToolbarItem() { Text = "Toolbar Item 2" };
			var item3 = new ToolbarItem() { Text = "Toolbar Item 2", IconImageSource = "red.png" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2,
					item3
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Right);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position With Images")]
		public async Task ToolbarDefaultLabelPositionOnlyImages()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { IconImageSource = "red.png" };
			var item2 = new ToolbarItem() { IconImageSource = "red.png" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Collapsed);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position Add Text to Images")]
		public async Task ToolbarDefaultLabelPositionAddText()
		{
			SetupBuilder();

			var item1 = new ToolbarItem() { IconImageSource = "red.png" };
			var item2 = new ToolbarItem() { IconImageSource = "red.png" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				navPage.CurrentPage.ToolbarItems[0].Text = "Toolbar Item 1";

				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Right);
				return Task.CompletedTask;
			});
		}
	}
}
