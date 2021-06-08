using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;

namespace Maui.Controls.Sample.SingleProject
{
	public class Startup : IStartup
	{
		internal static bool UseBlazor = false;

		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.RegisterBlazorMauiWebView(typeof(Startup).Assembly)
				.UseMauiApp<MyApp>();

			if (UseBlazor)
            {
				appBuilder.UseMicrosoftExtensionsServiceProviderFactory(); // Blazor requires service scopes, which are supported only with Microsoft.Extensions.DependencyInjection
				appBuilder
					.ConfigureServices(services =>
					{
						services.AddBlazorWebView();
					});
			}
		}
	}
}
