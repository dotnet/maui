using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Xunit.Runners
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureTestRunner(this IAppHostBuilder appHostBuilder, TestRunnerOptions options)
		{
			appHostBuilder.ConfigureServices(services => services.AddSingleton(options));
			appHostBuilder.UseMauiApp(svc => new FormsRunner(svc.GetRequiredService<TestRunnerOptions>()));

			return appHostBuilder;
		}
	}

	public class TestRunnerOptions
	{
		public List<Assembly> Assemblies { get; set; } = new List<Assembly>();
	}
}
