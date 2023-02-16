using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task PushingPageWithBackgroundStacksAndUnStacksCorrectly(bool useColor)
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					var windowRootViewContainer = (WPanel)handler.PlatformView.Content;
					ContentPage backgroundColorContentPage = new ContentPage();

					if (useColor)
						backgroundColorContentPage.BackgroundColor = Colors.Purple;
					else
						backgroundColorContentPage.Background = SolidColorBrush.Purple;

					await navPage.CurrentPage.Navigation.PushModalAsync(backgroundColorContentPage);

					var modalRootView =
						backgroundColorContentPage.FindMauiContext().GetNavigationRootManager().RootView;
					var rootPageRootView =
						navPage.FindMauiContext().GetNavigationRootManager().RootView;

					Assert.Equal(1, windowRootViewContainer.Children.IndexOf(modalRootView));
					Assert.Equal(0, windowRootViewContainer.Children.IndexOf(rootPageRootView));

					await navPage.CurrentPage.Navigation.PopModalAsync();

					Assert.Equal(0, windowRootViewContainer.Children.IndexOf(rootPageRootView));
					Assert.DoesNotContain(modalRootView, windowRootViewContainer.Children);
				});
		}

		[Fact]
		public async Task WindowTitleSetToModalTitleContainer()
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					var rootView = handler.PlatformView.Content;
					ContentPage modalPage = new ContentPage();

					await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					var customTitleBar = modalPage
						.FindMauiContext()
						.GetNavigationRootManager()
						.AppTitleBarContentControl;


					var mauiWindow = (MauiWinUIWindow)handler.PlatformView;
					Assert.Equal(mauiWindow.MauiCustomTitleBar, customTitleBar);
					(handler.VirtualView as Window).Title = "Update Title";

					var customTitle = mauiWindow.MauiCustomTitleBar.GetDescendantByName<UI.Xaml.Controls.TextBlock>("AppTitle");

					Assert.Equal("Update Title", customTitle.Text);
				});
		}
	}
}
