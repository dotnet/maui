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
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Toolbar)]
	public partial class ToolbarTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Toolbar FontImageSource icon is shown with text")]
		public async Task ToolbarFontImageSourceIconIsShownWithText()
		{
			SetupBuilder();

			var toolbarItem = new ToolbarItem()
			{
				Text = "Letter X",
				IconImageSource = new FontImageSource()
				{
					Glyph = "X",
					Color = Microsoft.Maui.Graphics.Colors.Black
				}
			};

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					toolbarItem
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var commandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				var button = Assert.IsType<AppBarButton>(commandBar.PrimaryCommands[0]);
				var icon = Assert.IsType<FontIcon>(button.Icon);

				Assert.Equal("Letter X", button.Label);
				Assert.Equal("X", icon.Glyph);
				Assert.Null(button.Content);
				await AssertionExtensions.AssertEventually(() => icon.ActualWidth > 0 && icon.ActualHeight > 0);

				toolbarItem.IconImageSource = new FontImageSource()
				{
					Glyph = "Z",
					Color = Microsoft.Maui.Graphics.Colors.Black
				};

				await AssertionExtensions.AssertEventually(() =>
					button.Icon is FontIcon { Glyph: "Z" } replacementIcon &&
					replacementIcon.ActualWidth > 0 &&
					replacementIcon.ActualHeight > 0);

				toolbarItem.IconImageSource = ImageSource.FromStream(() =>
					new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAIAAAD/gAIDAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAJOgAACToAYJjBRwAAADLSURBVHhe7cfBCQAgAMSw239p/RcsDlDIJzvLLz6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+Aj+Ag+go/gI/gIPoKP4CP4CD6Cj+DztF2eLesr6yjgngAAAABJRU5ErkJggg==")));

				var buttonWithFileImage = Assert.IsType<AppBarButton>(commandBar.PrimaryCommands[0]);
				Assert.Null(buttonWithFileImage.Icon);
				var streamImage = Assert.IsType<Microsoft.UI.Xaml.Controls.Image>(buttonWithFileImage.Content);

				await AssertionExtensions.AssertEventually(() => streamImage.Source is not null);
			});
		}

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
