using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
				});
			});
		}

		[Fact("SafeAreaInset Property is Set")]
		public async Task SafeAreaInsetIsSet()
		{
			SetupBuilder();

			var page = new ContentPage { Background = Colors.Blue };

			var shell = new Shell() { CurrentItem = page };

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, (handler) =>
			{
				if (handler.VirtualView is Window window && window.Page is Shell shellPage)
				{
					Assert.NotEqual(shellPage.CurrentPage.On<iOS>().SafeAreaInsets(), Thickness.Zero);
				}
			});
		}
	}
}
