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
				services.AddSingleton<IWindowFactory, WindowFactory>();
			});

			return builder;
		}
	}
}