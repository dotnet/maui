using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
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
				TapBackButton(navController.NavigationBar);
				await OnNavigatedToAsync(page);
				Assert.True(page.HasNavigatedTo);
			});
		}

		void TapBackButton(UINavigationBar uINavigationBar)
		{
			var item = uINavigationBar.FindDescendantView<UIView>(result =>
			{
				return result.Class.Name?.Contains("UIButtonBarButton", StringComparison.OrdinalIgnoreCase) == true;
			});

			_ = item ?? throw new Exception("Unable to locate back button view");

			var recognizer = item.GestureRecognizers.OfType<UITapGestureRecognizer>().First();
			recognizer.State = UIGestureRecognizerState.Ended;
		}

	}
}
