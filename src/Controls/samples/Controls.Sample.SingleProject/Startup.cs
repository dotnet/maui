using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.SingleProject
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.RegisterCompatibilityRenderers()
				.UseMauiApp<MyApp>();
		}
	}
}