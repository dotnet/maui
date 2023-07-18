using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
	public partial class WebViewTests : ControlsHandlerTestBase
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			base.ConfigureBuilder(mauiAppBuilder)
				.ConfigureMauiHandlers(handlers =>
					handlers.AddHandler<WebView, WebViewHandler>());
	}
}
