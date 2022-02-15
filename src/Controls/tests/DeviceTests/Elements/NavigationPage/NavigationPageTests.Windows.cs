using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WWindow = Microsoft.UI.Xaml.Window;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using WAppBarButton = Microsoft.UI.Xaml.Controls.AppBarButton;

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
					handlers.AddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
				});
			});
		}

		[Fact(DisplayName = "Back Button Visibility Changes with push/pop")]
		public async Task TabbedPageHandlerDisconnects()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				Assert.Equal(UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed, navView.IsBackButtonVisible);
				await navPage.PushAsync(new ContentPage());
				Assert.Equal(UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible, navView.IsBackButtonVisible);
				await navPage.PopAsync();
				Assert.Equal(UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed, navView.IsBackButtonVisible);
			});
		}

		[Fact(DisplayName = "Set Has Back Button")]
		public async Task SetHasBackButton()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				await navPage.PushAsync(new ContentPage());
				NavigationPage.SetHasBackButton(navPage.CurrentPage, false);
				Assert.Equal(UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed, navView.IsBackButtonVisible);
				NavigationPage.SetHasBackButton(navPage.CurrentPage, true);
				Assert.Equal(UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible, navView.IsBackButtonVisible);
			});
		}

		[Fact(DisplayName = "Set Has Navigation Bar")]
		public async Task SettHasNavigationBar()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				var header = navView.Header as WFrameworkElement;
				Assert.True(header.Visibility == UI.Xaml.Visibility.Visible);
				NavigationPage.SetHasNavigationBar(navPage.CurrentPage, false);
				Assert.True(header.Visibility == UI.Xaml.Visibility.Collapsed);
				NavigationPage.SetHasNavigationBar(navPage.CurrentPage, true);
				Assert.True(header.Visibility == UI.Xaml.Visibility.Visible);
				return Task.CompletedTask;
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
				var navView = (RootNavigationView)GetMauiNavigationView(handler.MauiContext);
				WindowHeader windowHeader = (WindowHeader)navView.Header;
				var primaryCommand = ((WAppBarButton)windowHeader.CommandBar.PrimaryCommands[0]);

				Assert.Equal(toolbarItem, primaryCommand.DataContext);
				return Task.CompletedTask;
			});
		}
	}
}
