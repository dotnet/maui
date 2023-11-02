using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using WAppBarButton = Microsoft.UI.Xaml.Controls.AppBarButton;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using WWindow = Microsoft.UI.Xaml.Window;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Back Button Enabled Changes with push/pop")]
		public async Task BackButtonEnabledChangesWithPushPop()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<NavigationViewHandler>(navPage, async (handler) =>
			{
				var navView = (RootNavigationView)GetMauiNavigationView(handler.MauiContext);
				Assert.False(navView.IsBackEnabled);
				await navPage.PushAsync(new ContentPage());
				Assert.True(navView.IsBackEnabled);
				await navPage.PopAsync();
				Assert.False(navView.IsBackEnabled);
			});
		}

		[Fact(DisplayName = "Back Button Enabled Changes with push/pop + page change")]
		public async Task BackButtonEnabledChangesWithPushPopAndPageChanges()
		{
			SetupBuilder();

			var flyoutPage = new FlyoutPage();
			flyoutPage.Title = "Hello world";
            flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
			flyoutPage.Flyout = new ContentPage() { Title = "Hello world" };

			var first = new NavigationPage(new ContentPage());
			var second = new NavigationPage(new ContentPage());

            flyoutPage.Detail = first;

            await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var navView = (RootNavigationView)GetMauiNavigationView(handler.MauiContext);
				Assert.False(navView.IsBackEnabled);

				await first.PushAsync(new ContentPage());
				Assert.True(navView.IsBackEnabled);

				flyoutPage.Detail = second;
				Assert.False(navView.IsBackEnabled);

				await second.PushAsync(new ContentPage());
				Assert.True(navView.IsBackEnabled);
			});
		}

		[Fact(DisplayName = "App Title Bar Margin Sets when Back Button Visible")]
		public async Task AppTitleBarMarginSetsWhenBackButtonVisible()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<NavigationViewHandler>(navPage, async (handler) =>
			{
				var navView = (RootNavigationView)GetMauiNavigationView(handler.MauiContext);
				await navPage.PushAsync(new ContentPage());

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var rootView = rootManager.RootView as WindowRootView;
				Assert.True(rootView.AppTitleBarContainer.Margin.Left > 20);
			});
		}
	}
}
