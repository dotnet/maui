#if !IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	public partial class NavigationPageTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
				});
			});
		}

		[Fact(DisplayName = "Back Button Visibility Changes with push/pop")]
		public async Task BackButtonVisibilityChangesWithPushPop()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				Assert.False(IsBackButtonVisible(handler.MauiContext));
				await navPage.PushAsync(new ContentPage());
				Assert.True(IsBackButtonVisible(handler.MauiContext));
				await navPage.PopAsync();
				Assert.False(IsBackButtonVisible(handler.MauiContext));
			});
		}

		[Fact(DisplayName = "Set Has Back Button")]
		public async Task SetHasBackButton()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(new ContentPage());
				NavigationPage.SetHasBackButton(navPage.CurrentPage, false);
				Assert.False(IsBackButtonVisible(handler.MauiContext));
				NavigationPage.SetHasBackButton(navPage.CurrentPage, true);
				Assert.True(IsBackButtonVisible(handler.MauiContext));
			});
		}

		[Fact(DisplayName = "Set Has Navigation Bar")]
		public async Task SettHasNavigationBar()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				Assert.True(IsNavigationBarVisible(handler));
				NavigationPage.SetHasNavigationBar(navPage.CurrentPage, false);
				Assert.False(IsNavigationBarVisible(handler));
				NavigationPage.SetHasNavigationBar(navPage.CurrentPage, true);
				Assert.True(IsNavigationBarVisible(handler));
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "NavigationBar Removes When MainPage Set To ContentPage")]
		public async Task NavigationBarRemovesWhenMainPageSetToContentPage()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());
			var window = new Window(navPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var contentPage = new ContentPage();
				window.Page = contentPage;
				await OnLoadedAsync(contentPage);
				Assert.False(IsNavigationBarVisible(handler));
			});
		}

		[Fact(DisplayName = "Toolbar Items Map Correctly")]
		public async Task ToolbarItemsMapCorrectly()
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem() { Text = "Toolbar Item 1" };
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					toolbarItem
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				ToolbarItemsMatch(handler, toolbarItem);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Title")]
		public async Task ToolbarTitle()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				string title = GetToolbarTitle(handler);
				Assert.Equal("Page Title", title);
			});
		}
	}
}
#endif
