#nullable enable

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public static class AppHost
	{
		public static IAppHostBuilder CreateDefaultBuilder()
		{
			var builder = new AppHostBuilder();

			builder.UseMicrosoftExtensionsServiceProviderFactory();
			builder.ConfigureFonts();
			builder.ConfigureImageSources();
			builder.ConfigureServices(services =>
			{
				services.AddScoped<IWindow>(sp =>
				{
					var application = sp.GetRequiredService<IApplication>();
					var args = sp.GetRequiredService<StartupActivationState>();
					return application.CreateWindow(args.ActivationState!);

				});
				services.AddScoped<StartupActivationState>();
			});

			return builder;
		}
	}
}