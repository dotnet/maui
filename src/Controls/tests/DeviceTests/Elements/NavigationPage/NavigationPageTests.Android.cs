using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Android.Material.AppBar;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		// We only want to fire BackButtonVisible Toolbar events if the user
		// is changing the default behavior of the BackButtonVisibility
		// this way the platform animations are allowed to just happen naturally
		[Fact(DisplayName = "Pushing And Popping Doesnt Fire BackButtonVisible Toolbar Events")]
		public async Task PushingAndPoppingDoesntFireBackButtonVisibleToolbarEvents()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				bool failed = false;
				var toolbar = (NavigationPageToolbar)navPage.FindMyToolbar();
				toolbar.PropertyChanged += (_, args) =>
				{
					if (args.PropertyName == nameof(Toolbar.BackButtonVisible) ||
						args.PropertyName == nameof(Toolbar.DrawerToggleVisible))
					{
						failed = true;
					}
				};

				await navPage.Navigation.PushAsync(new ContentPage());
				Assert.False(failed);
				await navPage.Navigation.PopAsync();
				Assert.False(failed);
			});
		}

		[Fact(DisplayName = "StackNavigationManager Clears References On Disconnect (Issue 33918)")]
		public async Task StackNavigationManagerClearsReferencesOnDisconnect()
		{
			SetupBuilder();

			var mainPage = new ContentPage { Title = "Main Page" };
			var baseNavPage = new NavigationPage(mainPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(baseNavPage), async (handler) =>
			{
				var tab1Content = new ContentPage { Title = "Tab 1", Content = new Label { Text = "Tab 1 Content" } };
				var tab2Content = new ContentPage { Title = "Tab 2", Content = new Label { Text = "Tab 2 Content" } };

				var tab1Nav = new NavigationPage(tab1Content) { Title = "Tab 1" };
				var tab2Nav = new NavigationPage(tab2Content) { Title = "Tab 2" };

				var tabbedPage = new TabbedPage
				{
					Title = "Tabbed Modal",
					Children = { tab1Nav, tab2Nav }
				};

				await baseNavPage.Navigation.PushModalAsync(tabbedPage, animated: false);
				await OnLoadedAsync(tab1Content);

				var tab1Handler = tab1Nav.Handler as NavigationViewHandler;
				var tab2Handler = tab2Nav.Handler as NavigationViewHandler;
				Assert.NotNull(tab1Handler);
				Assert.NotNull(tab2Handler);

				var tab1SnManager = tab1Handler.StackNavigationManager;
				var tab2SnManager = tab2Handler.StackNavigationManager;
				Assert.NotNull(tab1SnManager);
				Assert.NotNull(tab2SnManager);

				tabbedPage.CurrentPage = tab2Nav;
				await Task.Delay(100);

				await baseNavPage.Navigation.PopModalAsync(animated: false);
				await Task.Delay(100);

				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var currentPageField = typeof(StackNavigationManager).GetField("_currentPage", flags);
				var fragmentContainerViewField = typeof(StackNavigationManager).GetField("_fragmentContainerView", flags);
				var fragmentManagerField = typeof(StackNavigationManager).GetField("_fragmentManager", flags);

				Assert.Null(currentPageField.GetValue(tab1SnManager));
				Assert.Null(fragmentContainerViewField.GetValue(tab1SnManager));
				Assert.Null(fragmentManagerField.GetValue(tab1SnManager));

				Assert.Null(currentPageField.GetValue(tab2SnManager));
				Assert.Null(fragmentContainerViewField.GetValue(tab2SnManager));
				Assert.Null(fragmentManagerField.GetValue(tab2SnManager));
			});
		}
	}
}
