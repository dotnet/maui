using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using WindowSoftInputModeAdjust = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ChangeModalStackWhileDeactivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					IWindow iWindow = window;
					await page.Navigation.PushModalAsync(new ContentPage());
					await page.Navigation.PushModalAsync(modalPage);
					await page.Navigation.PushModalAsync(new ContentPage());
					await page.Navigation.PushModalAsync(new ContentPage());
					iWindow.Deactivated();
					await page.Navigation.PopModalAsync();
					await page.Navigation.PopModalAsync();
					iWindow.Activated();
					await OnLoadedAsync(modalPage);
				});
		}

		[Fact]
		public async Task DontPushModalPagesWhenWindowIsDeactivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					IWindow iWindow = window;
					iWindow.Deactivated();
					await page.Navigation.PushModalAsync(modalPage);
					Assert.False(modalPage.IsLoaded);
					iWindow.Activated();
					await OnLoadedAsync(modalPage);
				});
		}
	}
}
