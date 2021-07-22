#nullable enable
using Microsoft.Maui.LifecycleEvents;

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
			builder.ConfigureAnimations();
			builder.ConfigureCrossPlatformLifecycleEvents();

			return builder;
		}
	}
}