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
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.FlyoutPage)]
	public partial class FlyoutPageTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "FlyoutPage Initializes with FlyoutCustomContent Set")]
		public async Task FlyoutPageInitializesWithFlyoutCustomContentSet()
		{
			SetupBuilder();
			var flyoutPage = CreateBasicFlyoutPage();

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, (handler) =>
			{
				Assert.NotNull(handler.PlatformView.FlyoutCustomContent);
				Assert.NotNull(handler.PlatformView.PaneCustomContent);
				return Task.CompletedTask;
			});
		}


		[Fact(DisplayName = "FlyoutPage Initializes with Header Set")]
		public async Task FlyoutPageInitializesWithHeaderSet()
		{
			SetupBuilder();
			var flyoutPage = CreateBasicFlyoutPage();

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(flyoutPage), (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				Assert.NotNull(navView.Header);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Flyout Locked Offset")]
		public async Task FlyoutLockedOffset()
		{
			SetupBuilder();
			var flyout = CreateBasicFlyoutPage();
			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyout, (handler) =>
			{
				flyout.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
				var distance = DistanceYFromTheBottomOfTheAppTitleBar(flyout.Flyout);
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Flyout Locked Offset from App Title Bar With Pushed Page")]
		public async Task FlyoutLockedOffsetFromAppTitleBarWithPushedPage()
		{
			SetupBuilder();
			var flyout = CreateBasicFlyoutPage();
			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyout, async (handler) =>
			{
				flyout.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
				await flyout.Detail.Navigation.PushAsync(new ContentPage());
				var distance = DistanceYFromTheBottomOfTheAppTitleBar(flyout.Flyout);
				Assert.True(Math.Abs(distance) < 1);
			});
		}

		NavigationView FindPlatformFlyoutView(WFrameworkElement aView) =>
			aView.GetParentOfType<NavigationView>();

		FlyoutPage CreateBasicFlyoutPage()
		{
			return new FlyoutPage()
			{
				Detail = new NavigationPage(new ContentPage() { Title = "Detail" }) { Title = "NavigationPage Detail" },
				Flyout = new ContentPage() { Title = "Flyout" }
			};
		}
	}
}
