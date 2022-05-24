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

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : HandlerTestBase
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
					var rootView = handler.PlatformView.Content;
					ContentPage backgroundColorContentPage = new ContentPage();

					if (useColor)
						backgroundColorContentPage.BackgroundColor = Colors.Purple;
					else
						backgroundColorContentPage.Background = SolidColorBrush.Purple;

					await navPage.CurrentPage.Navigation.PushModalAsync(backgroundColorContentPage);

					// Root should now be a ContentPanel
					var rootPanel = (ContentPanel)handler.PlatformView.Content;
					Assert.Contains(rootView, rootPanel.Children);
					var modalRootView =
						backgroundColorContentPage.FindMauiContext().GetNavigationRootManager().RootView;
					Assert.Contains(modalRootView, rootPanel.Children);

					await navPage.CurrentPage.Navigation.PopModalAsync();
					Assert.Equal(rootView, handler.PlatformView.Content);
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
