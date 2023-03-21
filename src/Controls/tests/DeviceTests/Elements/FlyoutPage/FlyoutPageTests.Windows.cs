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
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using WWindow = Microsoft.UI.Xaml.Window;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.FlyoutPage)]
	public partial class FlyoutPageTests : ControlsHandlerTestBase
	{
		[Theory(DisplayName = "FlyoutPage Initializes with FlyoutCustomContent Set")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutPageInitializesWithFlyoutCustomContentSet(Type flyoutPageType)
		{
			SetupBuilder();
			var flyoutPage = CreateBasicFlyoutPage(flyoutPageType);

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, (handler) =>
			{
				Assert.NotNull(handler.PlatformView.FlyoutCustomContent);
				Assert.NotNull(handler.PlatformView.PaneCustomContent);
				return Task.CompletedTask;
			});
		}


		[Theory(DisplayName = "FlyoutPage Initializes with Header Set")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutPageInitializesWithHeaderSet(Type flyoutPageType)
		{
			SetupBuilder();
			var flyoutPage = CreateBasicFlyoutPage(flyoutPageType);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(flyoutPage), (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				Assert.NotNull(navView.Header);
				return Task.CompletedTask;
			});
		}

		[Theory(DisplayName = "Flyout Locked Offset")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutLockedOffset(Type flyoutPageType)
		{
			SetupBuilder();
			var flyout = CreateBasicFlyoutPage(flyoutPageType);
			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyout, (handler) =>
			{
				flyout.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
				var distance = DistanceYFromTheBottomOfTheAppTitleBar(flyout.Flyout);
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Theory(DisplayName = "Flyout Locked Offset from App Title Bar With Pushed Page")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutLockedOffsetFromAppTitleBarWithPushedPage(Type flyoutPageType)
		{
			SetupBuilder();
			var flyout = CreateBasicFlyoutPage(flyoutPageType);
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

		FlyoutPage CreateBasicFlyoutPage(Type flyoutPageType)
		{
			return CreateFlyoutPage(
				flyoutPageType,
				new NavigationPage(new ContentPage() { Title = "Detail" }) { Title = "NavigationPage Detail" },
				new ContentPage() { Title = "Flyout" });
		}
	}
}
