using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
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
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task InteractivePopGestureRespectsBackButtonPressedWhenNavigationBarHidden(bool backHandled, bool expectedShouldBegin)
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var backHandlingPage = new BackHandlingPage(backHandled);
			NavigationPage.SetHasNavigationBar(backHandlingPage, false);

			var navPage = new NavigationPage(rootPage);
			await navPage.PushAsync(backHandlingPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				var navController = Assert.IsType<NavigationRenderer>(navPage.Handler);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.Equal(expectedShouldBegin, recognizer.Delegate.ShouldBegin(recognizer));
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				await Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task InteractivePopGestureRespectsBackButtonPressedWhenNavigationBarVisible(bool backHandled, bool expectedShouldBegin)
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var backHandlingPage = new BackHandlingPage(backHandled);
			var navPage = new NavigationPage(rootPage);
			await navPage.PushAsync(backHandlingPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				var navController = Assert.IsType<NavigationRenderer>(navPage.Handler);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.Equal(expectedShouldBegin, recognizer.Delegate.ShouldBegin(recognizer));
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				await Task.CompletedTask;
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
#pragma warning disable CS0618 // Type or member is obsolete
			Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetIsNavigationBarTranslucent(navPage, enabled);
#pragma warning restore CS0618 // Type or member is obsolete

			var translucent = await GetValueAsync(navPage, (handler) => (handler.ViewController as UINavigationController).NavigationBar.Translucent);
			Assert.Equal(enabled, translucent);
		}

		//src/Compatibility/Core/tests/iOS/NavigationTests.cs
		[Fact]
		[Description("Multiple calls to NavigationRenderer.Dispose shouldn't crash")]
		public async Task NavigationRendererDoubleDisposal()
		{
			SetupBuilder();

			var root = new ContentPage()
			{
				Title = "root",
				Content = new Label { Text = "Hello" }
			};

			await root.Dispatcher.DispatchAsync(() =>
			{
				var navPage = new NavigationPage(root);
				var handler = CreateHandler(navPage);

				// Calling Dispose more than once should be fine
				(handler as NavigationRenderer).Dispose();
				(handler as NavigationRenderer).Dispose();
			});
		}

		sealed class BackHandlingPage : ContentPage
		{
			readonly bool _backHandled;

			public BackHandlingPage(bool backHandled)
			{
				_backHandled = backHandled;
			}

			public int BackButtonPressedCount { get; private set; }

			protected override bool OnBackButtonPressed()
			{
				BackButtonPressedCount++;
				return _backHandled;
			}
		}
	}
}
