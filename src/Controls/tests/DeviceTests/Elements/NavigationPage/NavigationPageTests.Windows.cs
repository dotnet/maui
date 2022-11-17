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
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{

		string GetToolbarTitle(IElementHandler handler) =>
			GetPlatformToolbar(handler).Title;


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
