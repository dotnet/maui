namespace Microsoft.Maui.Hosting
{
	public static class StartupExtensions
	{
		public static IAppHostBuilder CreateAppHostBuilder(this IStartup startup)
		{
			if (startup is IAppHostBuilderStartup hostBuilderStartup)
				return hostBuilderStartup.CreateAppHostBuilder();

			return AppHost.CreateDefaultBuilder();
		}

		public static IAppHostBuilder ConfigureUsing(this IAppHostBuilder appHostBuilder, IStartup startup)
		{
			startup.Configure(appHostBuilder);

			return appHostBuilder;
		}
	}
}