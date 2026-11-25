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
		const string TestPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXY/jPwPAfAAUAAf+mXJtdAAAAAElFTkSuQmCC";

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PendingNavigationTitleFontDoesNotOverwriteClearedOrDisconnectedToolbar(bool disconnect)
		{
			SetupBuilder();
			var page = new ContentPage();
			var navigationPage = new NavigationPage(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navigationPage), async handler =>
			{
				await OnLoadedAsync(page);
				var toolbar = (Toolbar)((IToolbarElement)navigationPage.Window).Toolbar;
				var nativeToolbar = (MauiToolbar)toolbar.Handler.PlatformView;
				var context = toolbar.Handler.MauiContext;
				context.Services.GetRequiredService<IFontRegistrar>().Register("dokdo_regular.ttf", "Dokdo");
				var font = new FontImageSource { FontFamily = "Dokdo", Glyph = "X", Size = 24 };
				NavigationPage.SetTitleIconImageSource(page, font);
				Assert.Same(font, toolbar.TitleIcon);
				using var initial = await ((IImageSource)font).GetPlatformImageAsync(context);
				Assert.NotNull(initial);
				Assert.Same(nativeToolbar, toolbar.Handler.PlatformView);
				Assert.NotNull(nativeToolbar.TitleIconImageSource);

				var pendingFont = new FontImageSource { FontFamily = "Dokdo", Glyph = "Y", Size = 24 };
				NavigationPage.SetTitleIconImageSource(page, pendingFont);
				if (disconnect)
					toolbar.Handler = null;
				else
					NavigationPage.SetTitleIconImageSource(page, null);

				var expected = nativeToolbar.TitleIconImageSource;
				if (!disconnect)
					Assert.Null(expected);
				using var completed = await ((IImageSource)pendingFont).GetPlatformImageAsync(context);
				Assert.Same(expected, nativeToolbar.TitleIconImageSource);
			});
		}

		[Theory]
		[InlineData("Clear")]
		[InlineData("Replace")]
		[InlineData("Disconnect")]
		[InlineData("Reassign")]
		public async Task ObsoleteTitleIconResultIsDisposed(string update)
		{
			var completion = new TaskCompletionSource<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>>();
			var service = new WindowsTitleIconService { Result = completion.Task };
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureImageSources(services => services.AddService<IFontImageSource>(_ => service));
				builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Toolbar, ToolbarHandler>());
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				var toolbar = new Toolbar(new ContentPage()) { ToolbarItems = Array.Empty<ToolbarItem>() };
				var handler = CreateHandler<ToolbarHandler>(toolbar);
				var nativeToolbar = handler.PlatformView;
				toolbar.TitleIcon = new FontImageSource { Glyph = "X" };
				Assert.Equal(1, service.LoadCount);
				var current = new ImageSourceServiceResult(new UI.Xaml.Media.Imaging.WriteableBitmap(12, 18));
				service.Result = Task.FromResult<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>>(current);
				if (update == "Disconnect")
					toolbar.Handler = null;
				else if (update == "Reassign")
					handler.SetVirtualView(new Toolbar(new ContentPage())
					{
						ToolbarItems = Array.Empty<ToolbarItem>(),
						TitleIcon = new FontImageSource { Glyph = "Y" }
					});
				else
					toolbar.TitleIcon = update == "Clear" ? null : new FontImageSource { Glyph = "Y" };
				var expected = nativeToolbar.TitleIconImageSource;
				var obsolete = new ImageSourceServiceResult(new UI.Xaml.Media.Imaging.WriteableBitmap(37, 43));
				completion.SetResult(obsolete);
				await AssertionExtensions.AssertEventually(() => obsolete.IsDisposed);
				Assert.Same(expected, nativeToolbar.TitleIconImageSource);
				Assert.False(current.IsDisposed);
			});
		}

		sealed class WindowsTitleIconService : IImageSourceService<IFontImageSource>
		{
			public Task<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>> Result { get; set; }
			public int LoadCount { get; private set; }

			public Task<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>> GetImageSourceAsync(
				IImageSource source, float scale = 1, System.Threading.CancellationToken cancellationToken = default)
			{
				LoadCount++;
				return Result;
			}
		}

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
					new MemoryStream(Convert.FromBase64String(TestPng)));

				var buttonWithFileImage = Assert.IsType<AppBarButton>(commandBar.PrimaryCommands[0]);
				Assert.Null(buttonWithFileImage.Icon);
				var streamImage = Assert.IsType<Microsoft.UI.Xaml.Controls.Image>(buttonWithFileImage.Content);

				await AssertionExtensions.AssertEventually(() => streamImage.Source is not null);
			});
		}

		[Fact(DisplayName = "Toolbar secondary stream image does not throw with dynamic overflow")]
		public async Task ToolbarSecondaryStreamImageDoesNotThrowWithDynamicOverflow()
		{
			SetupBuilder();

			var page = new ContentPage();
			Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Page.SetToolbarDynamicOverflowEnabled(page, true);
			page.ToolbarItems.Add(new ToolbarItem
			{
				Text = "Secondary",
				Order = ToolbarItemOrder.Secondary,
				IconImageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(TestPng)))
			});

			var navPage = new NavigationPage(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var commandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				var button = Assert.IsType<AppBarButton>(commandBar.SecondaryCommands[0]);

				Assert.Equal("Secondary", button.Label);
				Assert.Null(button.Icon);
				return Task.CompletedTask;
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
