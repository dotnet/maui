#nullable enable

using System.ComponentModel;

namespace Microsoft.Maui.Hosting
{
	public static class AppHost
	{
		/// <summary>
		/// Designed for library authors needing to dynamically register handlers.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IAppHost? Current { get; internal set; } = null;

		public static IAppHostBuilder CreateDefaultBuilder()
		{
			var builder = new AppHostBuilder();

			builder.UseMauiServiceProviderFactory(false);

			builder.UseMauiHandlers();
			builder.ConfigureFonts();

			return builder;
		}
	}
}