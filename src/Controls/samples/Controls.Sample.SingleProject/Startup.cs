using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			System.Console.WriteLine("asdasdasd-startup");
			System.Diagnostics.Debug.WriteLine("eiloneilon-startup");
			appBuilder
				.UseFormsCompatibility()
			    .RegisterBlazorMauiWebView()
				.UseMauiApp<MyApp>();
		}
	}
}