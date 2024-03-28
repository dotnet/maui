using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task NavigatingBackViaBackButtonFiresNavigatedEvent()
		{
			SetupBuilder();
			var page = new ContentPage();

			var navPage = new NavigationPage(page) { Title = "App Page" };

			await navPage.PushAsync(new ContentPage());
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(navPage.CurrentPage);
				var navController = navPage.Handler as UINavigationController;

				Assert.False(page.HasNavigatedTo);
				navController.NavigationBar.TapBackButton();
				await OnNavigatedToAsync(page);
				Assert.True(page.HasNavigatedTo);
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task PrefersLargeTitles(bool enabled)
		{
			SetupBuilder();
			var page = new ContentPage();
			var navPage = new NavigationPage(page) { Title = "App Page" };
			Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(navPage, enabled);

			var largeTitles = await GetValueAsync(navPage, (handler) => (handler.ViewController as UINavigationController).NavigationBar.PrefersLargeTitles);
			Assert.Equal(enabled, largeTitles);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task TranslucentNavigationBar(bool enabled)
		{
			SetupBuilder();
			var page = new ContentPage();
			var navPage = new NavigationPage(page) { Title = "App Page" };
			Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetIsNavigationBarTranslucent(navPage, enabled);

			var translucent = await GetValueAsync(navPage, (handler) => (handler.ViewController as UINavigationController).NavigationBar.Translucent);
			Assert.Equal(enabled, translucent);
		}

		[Fact(DisplayName = "Navigating Back Via Back Button Fires OnBackButtonPressed Event")]
		public async Task NavigatingBackViaBackButtonFiresOnBackButtonPressedEvent()
		{
			SetupBuilder();
			var firstPage = new ContentPage();
			var navPage = new NavigationPage(firstPage) { Title = "App Page" };

			var secondPage = new DoubleBackContentPage();
			await navPage.PushAsync(secondPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(navPage.CurrentPage);
				var navController = navPage.Handler as UINavigationController;

				navController.NavigationBar.TapBackButton();

				// race condition that if we can the is back button pressed, it will always return false.
				// wait a little bit
				await AssertionExtensions.Wait(() => navPage.CurrentPage == firstPage);

				Assert.True(navPage.CurrentPage == secondPage);
				Assert.True(secondPage.IsBackButtonPressed);

				navController.NavigationBar.TapBackButton();

				// wait a little bit
				await AssertionExtensions.Wait(() => navPage.CurrentPage == firstPage);

				Assert.True(navPage.CurrentPage == firstPage);
				Assert.False(IsBackButtonVisible(navPage.Handler));
			});
		}

		class DoubleBackContentPage : ContentPage
		{
			public bool IsBackButtonPressed { get; set; }

			protected override bool OnBackButtonPressed()
			{
				if (!IsBackButtonPressed)
				{
					IsBackButtonPressed = true;
					return true;
				}

				return base.OnBackButtonPressed();
			}
		}
	}
}
