using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;

namespace MauiApp1
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.UseCompatibilityForms()
				.UseDefaultCompatibilityRenderers()
				.UseMauiApp<App>();
		}
	}
}